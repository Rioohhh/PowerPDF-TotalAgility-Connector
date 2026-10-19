using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PPDF.TotalAgility.Connector
{
    #region Model Classes

    /// <summary>
    /// Represents a TotalAgility session returned by LogOnWithPassword2.
    /// SessionId is persisted to registry after every successful logon
    /// so it can be reused across calls within the same config session.
    /// </summary>
    public class TASession
    {
        public string SessionId { get; set; }
        public string ResourceId { get; set; }
        public string DisplayName { get; set; }
        public bool IsValid { get; set; }
    }

    /// <summary>
    /// Represents a TotalAgility process returned by GetProcessesSummary.
    /// Version is intentionally excluded — always passed as 0.0 to the API.
    /// ToString() returns Name so this binds directly to the ComboBox.
    /// </summary>
    public class TAProcessIdentity
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public override string ToString() => Name;
    }

    /// <summary>
    /// Represents a single process initialization variable
    /// returned by GetProcessInitialization2.
    /// Saved to registry as a JSON array after config.
    /// </summary>
    public class TAInputVariable
    {
        public string Id { get; set; }
        public int VariableType { get; set; }
        public string Value { get; set; }
        public string DisplayName { get; set; }

        public override string ToString() => DisplayName ?? Id;
    }

    /// <summary>
    /// Passed to TAResultForm after a successful or failed job creation.
    /// </summary>
    public class TAJobResult
    {
        public string JobId { get; set; }
        public string ProcessName { get; set; }
        public string DocumentName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }

    #endregion

    public class TotalAgilityService
    {
        #region Variable Type Constants

        // Confirmed from GetProcessInitialization2 responses
        public const int VARIABLE_TYPE_STRING = 8;
        public const int VARIABLE_TYPE_INTEGER = 2;
        public const int VARIABLE_TYPE_DECIMAL = 3;
        public const int VARIABLE_TYPE_BOOLEAN = 4;
        public const int VARIABLE_TYPE_DATE = 5;
        public const int VARIABLE_TYPE_DATETIME = 6;
        public const int VARIABLE_TYPE_FOLDER = 32759;   // Folder variable — ignored at config and runtime
        public const int VARIABLE_TYPE_DOCUMENT = 32760;   // Document variable — PDF sent via RuntimeDocumentCollection
        public const int VARIABLE_TYPE_DATAOBJECT = 32748;

        #endregion

        private readonly string _sdkUrl;

        /// <summary>
        /// Initialises the service with the TA SDK base URL.
        /// e.g. http://server/TotalAgility/Services/Sdk
        /// The trailing slash is stripped to keep URL construction consistent.
        /// </summary>
        static TotalAgilityService()
        {
            // The Power PDF host process defaults to SSL3/TLS 1.0, which TotalAgility
            // (and Tungsten Cloud) reject. Add TLS 1.2 and, where the framework has it, 1.3.
            // Numeric values because the enum members are missing from older frameworks.
            ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072;
            try { ServicePointManager.SecurityProtocol |= (SecurityProtocolType)12288; }
            catch (NotSupportedException) { /* TLS 1.3 not available on this system */ }
        }

        public TotalAgilityService(string sdkUrl)
        {
            _sdkUrl = sdkUrl.TrimEnd('/');
        }

        #region Step 1 — Authentication

        /// <summary>
        /// Logs on to TotalAgility using username and password.
        /// UnconditionalLogOn = true ensures a new session is always created.
        ///
        /// Called:
        ///   - At config time when user clicks Connect
        ///   - At runtime when user clicks Send to TotalAgility
        ///
        /// After a successful logon the caller is responsible for persisting
        /// the returned SessionId to registry.
        /// </summary>
        public TASession LogOn(string userId, string password)
        {
            string url = _sdkUrl + "/UserService.svc/json/LogOnWithPassword2";

            var payload = new
            {
                userIdentityWithPassword = new
                {
                    UserId = userId,
                    Password = password,
                    UnconditionalLogOn = true
                }
            };

            string response = PostJson(url, JsonConvert.SerializeObject(payload));
            JObject result = JObject.Parse(response);
            JObject d = (JObject)result["d"];

            if (d == null)
                throw new Exception("Invalid response from LogOnWithPassword2.");

            bool isValid = d["IsValid"]?.Value<bool>() ?? false;
            if (!isValid)
                throw new Exception("TotalAgility logon failed. Please check your credentials.");

            return new TASession
            {
                SessionId = d["SessionId"]?.ToString(),
                ResourceId = d["ResourceId"]?.ToString(),
                DisplayName = d["DisplayName"]?.ToString(),
                IsValid = isValid
            };
        }

        #endregion



        #region Federated and Windows AD Authentication

        /// <summary>
        /// Extracts the TotalAgility base URL from the SDK URL.
        /// e.g. https://server/TotalAgility/Services/Sdk -> https://server/TotalAgility
        ///      https://server.tungstencloud.com/Services/Sdk -> https://server.tungstencloud.com
        /// </summary>
        public string GetTaBaseUrl(string sdkUrl)
        {
            int idx = sdkUrl.TrimEnd('/').IndexOf("/Services/",
                StringComparison.OrdinalIgnoreCase);
            if (idx > 0) return sdkUrl.Substring(0, idx);

            Uri uri = new Uri(sdkUrl);
            return uri.Scheme + "://" + uri.Host +
                   (uri.Port != 80 && uri.Port != 443 ? ":" + uri.Port : "");
        }

        /// <summary>
        /// Logs on using Windows AD credentials.
        /// Uses the current Windows username with empty password —
        /// works when TotalAgility is configured to sync with Windows AD.
        /// </summary>
        public TASession LogOnWithWindowsAD(string userId)
        {
            string url = _sdkUrl + "/UserService.svc/json/LogOnWithWindowsAuthentication2";

            var payload = new
            {
                logOnProtocol = 7,
                unconditionalLogOn = true
            };

            string response = PostJsonWithWindowsAuth(url, JsonConvert.SerializeObject(payload));
            JObject result = JObject.Parse(response);
            JObject d = (JObject)result["d"];

            if (d == null)
                throw new Exception("Invalid response from LogOnWithWindowsAuthentication2.");

            bool isValid = d["IsValid"]?.Value<bool>() ?? false;
            if (!isValid)
                throw new Exception("Windows AD logon failed. Ensure your Windows account is mapped in TotalAgility.");

            return new TASession
            {
                SessionId = d["SessionId"]?.ToString(),
                ResourceId = d["ResourceId"]?.ToString(),
                DisplayName = d["DisplayName"]?.ToString(),
                IsValid = isValid
            };
        }


        private string PostJsonWithWindowsAuth(string url, string json)
        {
            var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.UseDefaultCredentials = true;  // sends current Windows identity (NTLM/Kerberos)
            request.PreAuthenticate = true;

            byte[] bytes = Encoding.UTF8.GetBytes(json);
            request.ContentLength = bytes.Length;
            using (var stream = request.GetRequestStream())
                stream.Write(bytes, 0, bytes.Length);

            using (var response = (System.Net.HttpWebResponse)request.GetResponse())
            using (var reader = new System.IO.StreamReader(response.GetResponseStream()))
                return reader.ReadToEnd();
        }


        /// <summary>
        /// Logs on using Federated Security (SAML/Azure AD).
        /// Opens a WebView2 browser window — TotalAgility handles the SAML redirect.
        /// Extracts SessionId from browser session storage after successful login.
        /// </summary>
        public TASession LogOnWithFederated(string taBaseUrl, IWin32Window owner)
        {
            string sessionId = null;
            string logonUrl = taBaseUrl + "/Forms/Custom/logon.html";

            System.Threading.Thread fedThread = new System.Threading.Thread(() =>
            {
                using (var browserForm = new System.Windows.Forms.Form())
                {
                    browserForm.Text = "TotalAgility — Federated Login";
                    browserForm.Size = new System.Drawing.Size(600, 700);
                    browserForm.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

                    try
                    {
                        using (var stream = typeof(TotalAgilityService).Assembly.GetManifestResourceStream("PPDF.TotalAgility.Connector.Resources.Image_TA.png"))
                        {
                            if (stream != null)
                            {
                                using (var bmp = new System.Drawing.Bitmap(stream))
                                    browserForm.Icon = System.Drawing.Icon.FromHandle(bmp.GetHicon());
                            }
                        }
                    }
                    catch { /* non-critical — falls back to default icon */ }

                    var webView = new Microsoft.Web.WebView2.WinForms.WebView2();
                    webView.Dock = System.Windows.Forms.DockStyle.Fill;
                    browserForm.Controls.Add(webView);

                    browserForm.Shown += async (s, args) =>
                    {
                        try
                        {
                            string edgeFolder = System.IO.Path.Combine(
                                Environment.GetFolderPath(
                                    Environment.SpecialFolder.LocalApplicationData),
                                "Microsoft", "Edge", "User Data");

                            if (System.IO.Directory.Exists(edgeFolder))
                            {
                                var env = await Microsoft.Web.WebView2.Core
                                    .CoreWebView2Environment
                                    .CreateAsync(null, edgeFolder);
                                await webView.EnsureCoreWebView2Async(env);
                            }
                            else
                            {
                                await webView.EnsureCoreWebView2Async(null);
                            }
                        }
                        catch
                        {
                            await webView.EnsureCoreWebView2Async(null);
                        }

                        webView.CoreWebView2.Navigate(logonUrl);

                        webView.CoreWebView2.NavigationCompleted += async (nav, navArgs) =>
                        {
                            string currentUrl = webView.CoreWebView2.Source;

                            // Read session immediately when ProviderLogOnSucceeded=true
                            if (currentUrl.Contains("ProviderLogOnSucceeded=true") ||
                                currentUrl.Contains("GeneralWorkQueue"))
                            {
                                try
                                {
                                    string script = @"
                                    (function() {
                                        try {
                                            var raw = window.sessionStorage.getItem('SESSION_ID');
                                            if (raw) {
                                                var parsed = JSON.parse(raw);
                                                return parsed.value || '';
                                            }
                                            return '';
                                        } catch(e) { return ''; }
                                    })()";

                                    string result = await webView.CoreWebView2
                                        .ExecuteScriptAsync(script);
                                    result = result?.Trim('"');

                                    System.IO.File.AppendAllText(
                                        @"C:\Temp\TAConnectorDebug.txt",
                                        DateTime.Now + " | SessionId result: " + result + "\r\n");

                                    if (!string.IsNullOrEmpty(result) &&
                                        result != "null" && result != "")
                                    {
                                        sessionId = result;
                                        browserForm.Invoke(
                                            new Action(() => browserForm.Close()));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.IO.File.AppendAllText(
                                        @"C:\Temp\TAConnectorDebug.txt",
                                        DateTime.Now + " | Script error: " + ex.Message + "\r\n");
                                }
                            }
                        };
                    };

                    browserForm.ShowDialog(owner);
                }
            });

            fedThread.SetApartmentState(System.Threading.ApartmentState.STA);
            fedThread.Start();
            fedThread.Join();
            System.IO.File.AppendAllText(@"C:\Temp\TAConnectorDebug.txt", DateTime.Now + " | After Join — sessionId: " + (sessionId ?? "NULL") + "\r\n");

            if (string.IsNullOrEmpty(sessionId))
                throw new Exception(
                    "Federated login failed or was cancelled.");

            return new TASession
            {
                SessionId = sessionId,
                IsValid = true,
                DisplayName = "Federated User"
            };
        }

        #endregion




        #region Step 2 — Get Processes

        /// <summary>
        /// Returns all available processes from TotalAgility.
        /// Called at config time after Connect to populate the Process dropdown.
        ///
        /// Filter:
        ///   AccessType  = 1  (fixed)
        ///   ProcessType = 0  (fixed)
        ///   UseProcessType = true
        ///   Category — omitted (all categories)
        /// </summary>
        public List<TAProcessIdentity> GetProcesses(string sessionId)
        {
            string url = _sdkUrl + "/ProcessService.svc/json/GetProcessesSummary";

            var payload = new
            {
                sessionId = sessionId,
                processesSummaryFilter = new
                {
                    AccessType = 1,
                    UseProcessType = true,
                    ProcessType = 0
                }
            };

            string response = PostJson(url, JsonConvert.SerializeObject(payload));
            JObject result = JObject.Parse(response);
            JArray processes = result["d"] as JArray;

            var list = new List<TAProcessIdentity>();
            if (processes != null)
            {
                foreach (JObject p in processes)
                {
                    list.Add(new TAProcessIdentity
                    {
                        Id = p["Id"]?.ToString(),
                        Name = p["Name"]?.ToString()
                    });
                }
            }

            return list;
        }

        #endregion

        #region Step 3 — Get Process Initialization Variables

        /// <summary>
        /// Returns the initialization variables for the selected process.
        /// Called at config time when the user selects a process from the dropdown.
        ///
        /// processIdentity sends only Id — version is always 0.0 and Name is not required.
        /// Document variables (VariableType = 32760) are included in the returned list
        /// so the caller can filter them for display purposes, but they are never shown
        /// in the config UI and are never saved to registry.
        /// </summary>
        public List<TAInputVariable> GetProcessInitializationVariables(
            string sessionId, TAProcessIdentity processIdentity)
        {
            string url = _sdkUrl + "/ProcessService.svc/json/GetProcessInitialization2";

            var payload = new
            {
                sessionId = sessionId,
                processIdentity = new
                {
                    Id = processIdentity.Id
                    // Version omitted — TA defaults to latest (0.0)
                    // Name omitted — Id alone is sufficient
                },
                initializationOptions = new
                {
                    EvaluateXmlVariables = false,
                    EvaluateXmlExpressions = false
                }
            };

            string response = PostJson(url, JsonConvert.SerializeObject(payload));
            JObject result = JObject.Parse(response);
            JObject d = (JObject)result["d"];
            JArray inputVariables = d?["InputVariables"] as JArray;

            var list = new List<TAInputVariable>();
            if (inputVariables != null)
            {
                foreach (JObject v in inputVariables)
                {
                    list.Add(new TAInputVariable
                    {
                        Id = v["Id"]?.ToString(),
                        VariableType = v["VariableType"]?.Value<int>() ?? 0,
                        Value = v["Value"]?.ToString(),
                        DisplayName = v["DisplayName"]?.ToString()
                    });
                }
            }

            return list;
        }

        #endregion

        #region Step 4 — Runtime: Create Job With Documents

        /// <summary>
        /// Creates a job in TotalAgility on the selected process,
        /// attaching the PDF document inline as Base64.
        ///
        /// Endpoint: /Services/Sdk/JobService.svc/json/CreateJobWithDocuments
        ///
        /// The PDF is sent in RuntimeDocumentCollection with Base64Data and MimeType only.
        /// InputVariables contains the non-document init vars saved from config.
        /// Document variables are handled automatically by TA via RuntimeDocumentCollection
        /// and are never included in InputVariables.
        ///
        /// Returns TAJobResult with JobId, ProcessName, CreatedAt on success.
        /// </summary>
        public TAJobResult CreateJobWithDocuments(
            string sessionId,
            TAProcessIdentity processIdentity,
            byte[] pdfBytes,
            List<TAInputVariable> inputVariables)
        {
            string url = _sdkUrl + "/JobService.svc/json/CreateJobWithDocuments";

            // Build InputVariables list — Id and Value only as confirmed by API spec
            var variables = new List<object>();
            foreach (var v in inputVariables)
            {
                variables.Add(new
                {
                    Id = v.Id,
                    Value = (object)v.Value
                });
            }

            var payload = new
            {
                sessionId = sessionId,
                processIdentity = new
                {
                    Id = processIdentity.Id
                    // Version always 0.0 — omitted, TA defaults to latest
                },
                jobWithDocsInitialization = new
                {
                    RuntimeDocumentCollection = new[]
                    {
                        new
                        {
                            Base64Data = Convert.ToBase64String(pdfBytes),
                            MimeType   = "application/pdf"
                        }
                    },
                    InputVariables = variables,
                    FolderFields = new object[0]
                }
            };

            string response = PostJson(url, JsonConvert.SerializeObject(payload));
            JObject result = JObject.Parse(response);
            JObject d = (JObject)result["d"];

            if (d == null)
                throw new Exception("Invalid response from CreateJobWithDocuments.");

            string jobId = d["Id"]?.ToString();

            if (string.IsNullOrEmpty(jobId))
                throw new Exception("Job ID was not returned from CreateJobWithDocuments.");

            return new TAJobResult
            {
                JobId = jobId,
                ProcessName = processIdentity.Name,
                CreatedAt = DateTime.Now,
                Success = true
            };
        }

        #endregion

        #region HTTP Helper

        /// <summary>
        /// Core HTTP POST helper — sends a JSON body and returns the response string.
        /// On HTTP errors, reads the TA error body and includes it in the exception
        /// message for meaningful diagnostics.
        /// </summary>
        private string PostJson(string url, string json)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Proxy = null;
                request.Timeout = 300000; // 5 minutes
                request.ContentType = "application/json";
                request.Method = "POST";

                byte[] bodyBytes = Encoding.UTF8.GetBytes(json);
                request.ContentLength = bodyBytes.Length;

                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(bodyBytes, 0, bodyBytes.Length);
                    stream.Flush();
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(responseStream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse httpResponse)
                {
                    using (StreamReader reader =
                        new StreamReader(httpResponse.GetResponseStream()))
                    {
                        string errorBody = reader.ReadToEnd();
                        throw new Exception(
                            $"TotalAgility API Error " +
                            $"[{(int)httpResponse.StatusCode} {httpResponse.StatusDescription}]: " +
                            $"{errorBody}", ex);
                    }
                }
                throw new Exception(
                    $"Network error calling TotalAgility API: {ex.Message}", ex);
            }
        }

        #endregion
    }
}