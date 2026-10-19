using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace PPDF.TotalAgility.Connector
{
    /// <summary>
    /// One TotalAgility environment (e.g. a customer): its SDK URL, how to log on, and its
    /// document-type profiles. Credentials and the session are not part of the shared JSON;
    /// they live in a separate registry key (see <see cref="ConnectionStore"/>).
    /// </summary>
    public class TAConnection
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = "";
        public string SdkUrl { get; set; } = "";

        /// <summary>0 = Standard, 1 = Windows AD, 2 = Federated.</summary>
        public int AuthType { get; set; }
        public string WindowsUsername { get; set; } = "";

        /// <summary>Name of the profile that was used last, preselected in the Send dialog.</summary>
        public string LastProfile { get; set; } = "";
        public List<TAProfile> Profiles { get; set; } = new List<TAProfile>();

        [JsonIgnore] public string UserId { get; set; } = "";
        [JsonIgnore] public string Password { get; set; } = "";
        [JsonIgnore] public string SessionId { get; set; } = "";

        /// <summary>Host part of the SDK URL, shown next to the name so the target is unmistakable.</summary>
        [JsonIgnore]
        public string Host
        {
            get
            {
                Uri uri;
                if (!string.IsNullOrWhiteSpace(SdkUrl) &&
                    Uri.TryCreate(SdkUrl.Trim(), UriKind.Absolute, out uri))
                    return uri.Host;
                return SdkUrl ?? "";
            }
        }

        public TAProfile FindProfile(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            foreach (var p in Profiles)
                if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
                    return p;
            return null;
        }

        public override string ToString() => Name;
    }

    /// <summary>
    /// Reads and writes the connections.
    ///
    /// HKCU\Software\ScanSoft\Connectors\TotalAgility
    ///     TA_CONNECTIONS         JSON list of connections incl. profiles (safe to share/export)
    ///     TA_ACTIVE_CONNECTION   Id of the connection the Send button uses
    ///     TA_SHOW_CONFIRMATION   "0" = skip the dialog (only when there is a single connection and profile)
    ///
    /// HKCU\Software\ScanSoft\Connectors\TotalAgilitySecrets   (sibling key, so a .reg export of the key above leaves it out)
    ///     &lt;id&gt;.UserId, &lt;id&gt;.Password   DPAPI-encrypted for the current Windows user
    ///     &lt;id&gt;.Session                   temporary session id
    ///
    /// Settings of earlier versions (single connection) are migrated on first load.
    /// </summary>
    public static class ConnectionStore
    {
        /// <summary>Overridable so tests do not touch the real settings.</summary>
        internal static string RegistryPath = @"Software\ScanSoft\Connectors\TotalAgility";
        private static string SecretsPath { get { return RegistryPath + "Secrets"; } }

        private const string TA_CONNECTIONS = "TA_CONNECTIONS";
        private const string TA_ACTIVE_CONNECTION = "TA_ACTIVE_CONNECTION";
        private const string TA_SHOW_CONFIRMATION = "TA_SHOW_CONFIRMATION";

        // Settings of earlier versions
        private const string TA_SDK_URL = "TA_SDK_URL";
        private const string TA_AUTH_TYPE = "TA_AUTH_TYPE";
        private const string TA_WINDOWS_USERNAME = "TA_WINDOWS_USERNAME";
        private const string TA_USER_ID = "TA_USER_ID";
        private const string TA_PASSWORD = "TA_PASSWORD";
        private const string TA_SESSION_ID = "TA_SESSION_ID";
        private const string TA_PROFILES = "TA_PROFILES";
        private const string TA_LAST_PROFILE = "TA_LAST_PROFILE";
        private const string TA_PROCESS_ID = "TA_PROCESS_ID";
        private const string TA_PROCESS_NAME = "TA_PROCESS_NAME";
        private const string TA_INIT_VARIABLES = "TA_INIT_VARIABLES";

        // ── Public API ────────────────────────────────────────────────────

        /// <summary>Returns all connections (at least one), with credentials and session loaded.</summary>
        public static List<TAConnection> Load()
        {
            List<TAConnection> list = DeserializeConnections(ReadValue(RegistryPath, TA_CONNECTIONS));

            if (list.Count == 0)
            {
                list.Add(MigrateLegacy());
                SaveStructure(list);
                SetActive(list[0].Id);
            }

            foreach (var c in list)
            {
                c.UserId = Unprotect(ReadValue(SecretsPath, c.Id + ".UserId"));
                c.Password = Unprotect(ReadValue(SecretsPath, c.Id + ".Password"));
                c.SessionId = ReadValue(SecretsPath, c.Id + ".Session");
            }

            return list;
        }

        /// <summary>The connection the Send button uses.</summary>
        public static TAConnection GetActive(List<TAConnection> list)
        {
            string id = ReadValue(RegistryPath, TA_ACTIVE_CONNECTION);
            foreach (var c in list)
                if (c.Id == id)
                    return c;
            return list[0];
        }

        public static TAConnection GetActive() { return GetActive(Load()); }

        public static TAConnection Get(string id)
        {
            foreach (var c in Load())
                if (c.Id == id)
                    return c;
            return null;
        }

        public static void SetActive(string id)
        {
            WriteValue(RegistryPath, TA_ACTIVE_CONNECTION, id);
        }

        /// <summary>Saves everything, including credentials, and drops credentials of removed connections.</summary>
        public static void SaveAll(List<TAConnection> list)
        {
            SaveStructure(list);

            var keep = new HashSet<string>();
            foreach (var c in list)
            {
                keep.Add(c.Id + ".UserId");
                keep.Add(c.Id + ".Password");
                keep.Add(c.Id + ".Session");

                WriteSecret(c.Id + ".UserId", Protect(c.UserId));
                WriteSecret(c.Id + ".Password", Protect(c.Password));
                WriteSecret(c.Id + ".Session", c.SessionId);
            }

            using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(SecretsPath, true))
            {
                if (rk == null) return;
                foreach (string name in rk.GetValueNames())
                    if (!keep.Contains(name))
                        rk.DeleteValue(name, false);
            }
        }

        /// <summary>Stores a fresh session for one connection, nothing else.</summary>
        public static void SaveSession(TAConnection c)
        {
            WriteSecret(c.Id + ".Session", c.SessionId);
        }

        /// <summary>Replaces the profiles of one connection, leaving the other settings as they are on disk.</summary>
        public static void SaveProfiles(string connectionId, List<TAProfile> profiles, string lastProfile)
        {
            List<TAConnection> list = DeserializeConnections(ReadValue(RegistryPath, TA_CONNECTIONS));
            foreach (var c in list)
            {
                if (c.Id != connectionId) continue;
                c.Profiles = profiles;
                c.LastProfile = lastProfile ?? "";
                SaveStructure(list);
                return;
            }
        }

        public static void SetLastProfile(string connectionId, string name)
        {
            List<TAConnection> list = DeserializeConnections(ReadValue(RegistryPath, TA_CONNECTIONS));
            foreach (var c in list)
            {
                if (c.Id != connectionId) continue;
                c.LastProfile = name ?? "";
                SaveStructure(list);
                return;
            }
        }

        public static bool ShowConfirmation
        {
            get { return ReadValue(RegistryPath, TA_SHOW_CONFIRMATION) != "0"; }
            set { WriteValue(RegistryPath, TA_SHOW_CONFIRMATION, value ? "1" : "0"); }
        }

        // ── Internals ─────────────────────────────────────────────────────

        private static void SaveStructure(List<TAConnection> list)
        {
            WriteValue(RegistryPath, TA_CONNECTIONS, JsonConvert.SerializeObject(list));
        }

        private static List<TAConnection> DeserializeConnections(string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var list = JsonConvert.DeserializeObject<List<TAConnection>>(json);
                    if (list != null) return list;
                }
                catch { }
            }
            return new List<TAConnection>();
        }

        /// <summary>
        /// Builds the first connection from the single-connection settings of earlier versions
        /// (the old values stay in place) or, on a fresh install, an empty one.
        /// </summary>
        private static TAConnection MigrateLegacy()
        {
            var c = new TAConnection
            {
                SdkUrl = ReadValue(RegistryPath, TA_SDK_URL),
                WindowsUsername = ReadValue(RegistryPath, TA_WINDOWS_USERNAME),
                LastProfile = ReadValue(RegistryPath, TA_LAST_PROFILE)
            };

            int authType;
            if (int.TryParse(ReadValue(RegistryPath, TA_AUTH_TYPE), NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out authType))
                c.AuthType = authType;

            c.Name = string.IsNullOrEmpty(c.Host) ? "Connection 1" : c.Host;

            string profilesJson = ReadValue(RegistryPath, TA_PROFILES);
            if (!string.IsNullOrEmpty(profilesJson))
            {
                try
                {
                    c.Profiles = JsonConvert.DeserializeObject<List<TAProfile>>(profilesJson)
                                 ?? new List<TAProfile>();
                }
                catch { }
            }

            string legacyProcessId = ReadValue(RegistryPath, TA_PROCESS_ID);
            if (c.Profiles.Count == 0 && !string.IsNullOrEmpty(legacyProcessId))
            {
                string legacyName = ReadValue(RegistryPath, TA_PROCESS_NAME);
                var p = new TAProfile
                {
                    Name = string.IsNullOrEmpty(legacyName) ? "Default" : legacyName,
                    ProcessId = legacyProcessId,
                    ProcessName = legacyName
                };

                string varsJson = ReadValue(RegistryPath, TA_INIT_VARIABLES);
                if (!string.IsNullOrEmpty(varsJson))
                {
                    try
                    {
                        p.Variables = JsonConvert.DeserializeObject<List<TAInputVariable>>(varsJson)
                                      ?? new List<TAInputVariable>();
                    }
                    catch { }
                }
                c.Profiles.Add(p);
            }

            // The old encrypted values use the same DPAPI format, so they are copied as they are
            WriteSecret(c.Id + ".UserId", ReadValue(RegistryPath, TA_USER_ID));
            WriteSecret(c.Id + ".Password", ReadValue(RegistryPath, TA_PASSWORD));
            WriteSecret(c.Id + ".Session", ReadValue(RegistryPath, TA_SESSION_ID));

            return c;
        }

        private static string Protect(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            byte[] enc = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(value), null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(enc);
        }

        private static string Unprotect(string base64)
        {
            if (string.IsNullOrEmpty(base64)) return "";
            try
            {
                byte[] dec = ProtectedData.Unprotect(
                    Convert.FromBase64String(base64), null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(dec);
            }
            catch { return ""; }
        }

        private static string ReadValue(string path, string name)
        {
            using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(path))
            {
                if (rk == null) return "";
                return rk.GetValue(name, "") as string ?? "";
            }
        }

        private static void WriteValue(string path, string name, string value)
        {
            using (RegistryKey rk = Registry.CurrentUser.CreateSubKey(path))
                rk.SetValue(name, value ?? "", RegistryValueKind.String);
        }

        private static void WriteSecret(string name, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(SecretsPath, true))
                    if (rk != null) rk.DeleteValue(name, false);
                return;
            }
            WriteValue(SecretsPath, name, value);
        }
    }
}
