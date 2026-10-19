using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PPDF.TotalAgility.Connector
{
    /// <summary>
    /// A document type (e.g. "Factuur", "Kantoorartikelen") mapped to a
    /// TotalAgility process and the initialization variable values to send with it.
    /// Persisted as a JSON array in the TA_PROFILES registry value.
    /// </summary>
    public class TAProfile
    {
        public string Name { get; set; }
        public string ProcessId { get; set; }
        public string ProcessName { get; set; }
        public List<TAInputVariable> Variables { get; set; } = new List<TAInputVariable>();

        public override string ToString() => Name;
    }

    public static class ProfileStore
    {
        private const string REG_KEY = @"Software\ScanSoft\Connectors\TotalAgility";
        private const string TA_PROFILES = "TA_PROFILES";
        private const string TA_LAST_PROFILE = "TA_LAST_PROFILE";

        // Legacy single-process settings, migrated into a profile on first load
        private const string TA_PROCESS_ID = "TA_PROCESS_ID";
        private const string TA_PROCESS_NAME = "TA_PROCESS_NAME";
        private const string TA_INIT_VARIABLES = "TA_INIT_VARIABLES";

        public static List<TAProfile> Load()
        {
            var profiles = new List<TAProfile>();

            string json = ReadValue(TA_PROFILES);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    profiles = JsonConvert.DeserializeObject<List<TAProfile>>(json)
                               ?? new List<TAProfile>();
                }
                catch { }
            }

            if (profiles.Count == 0)
            {
                string legacyId = ReadValue(TA_PROCESS_ID);
                if (!string.IsNullOrEmpty(legacyId))
                {
                    string legacyName = ReadValue(TA_PROCESS_NAME);
                    var legacy = new TAProfile
                    {
                        Name = string.IsNullOrEmpty(legacyName) ? "Default" : legacyName,
                        ProcessId = legacyId,
                        ProcessName = legacyName
                    };

                    string varsJson = ReadValue(TA_INIT_VARIABLES);
                    if (!string.IsNullOrEmpty(varsJson))
                    {
                        try
                        {
                            legacy.Variables =
                                JsonConvert.DeserializeObject<List<TAInputVariable>>(varsJson)
                                ?? new List<TAInputVariable>();
                        }
                        catch { }
                    }

                    profiles.Add(legacy);
                    Save(profiles);
                }
            }

            return profiles;
        }

        public static void Save(List<TAProfile> profiles)
        {
            WriteValue(TA_PROFILES, JsonConvert.SerializeObject(profiles));
        }

        public static string GetLastUsedName() => ReadValue(TA_LAST_PROFILE);

        public static void SetLastUsedName(string name) => WriteValue(TA_LAST_PROFILE, name);

        public static TAProfile Find(List<TAProfile> profiles, string name)
        {
            if (profiles == null || string.IsNullOrEmpty(name)) return null;
            foreach (var p in profiles)
                if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
                    return p;
            return null;
        }

        private static string ReadValue(string key)
        {
            using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(REG_KEY))
            {
                if (rk == null) return "";
                return rk.GetValue(key, "") as string ?? "";
            }
        }

        private static void WriteValue(string key, string value)
        {
            using (RegistryKey rk = Registry.CurrentUser.CreateSubKey(REG_KEY))
                rk.SetValue(key, value ?? "", RegistryValueKind.String);
        }
    }
}
