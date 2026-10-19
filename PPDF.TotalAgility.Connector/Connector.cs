using DMSConnector;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PPDF.TotalAgility.Connector
{
    public abstract class ERRORS
    {
        public const int E_CANCELLED = -2147221492;
    }

    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    [Guid("A3F7C241-88B2-4D1E-9C3A-7E2B5F0D4C8A")]
    [ProgId("TotalAgility")]
    public class Connector : IDMSConnector, IPropertyPageHandler
    {
        [ComRegisterFunction]
        public static void RegisterAsConnector(Type t)
        {
            using (RegistryKey hklm = RegistryKey.OpenBaseKey(
                RegistryHive.LocalMachine, RegistryView.Registry32))
            {
                RegistryKey rkConnectors = OpenOrCreateSubkey(
                    OpenOrCreateSubkey(
                        OpenOrCreateSubkey(hklm, "Software"),
                        "ScanSoft"),
                    "Connectors");
                OpenOrCreateSubkey(rkConnectors, GetProgId(t));
            }
        }

        [ComUnregisterFunction]
        public static void UnregisterAsConnector(Type t)
        {
            using (RegistryKey hklm = RegistryKey.OpenBaseKey(
                RegistryHive.LocalMachine, RegistryView.Registry32))
            {
                hklm.DeleteSubKey(
                    @"Software\ScanSoft\Connectors\" + GetProgId(t), false);
            }
        }

        // ── Auth Type Constants ───────────────────────────────────────────
        private const int AUTH_STANDARD = 0;
        private const int AUTH_WINDOWS = 1;
        private const int AUTH_FEDERATED = 2;

        // ── State ─────────────────────────────────────────────────────────
        protected string _connectorName = "TotalAgility";
        protected string _languageCode;
        protected bool _initialized = false;
        protected IntPtr _parentWindow = IntPtr.Zero;
        protected MenuItemList _menuItems;
        protected Dictionary<string, Document> _documents =
            new Dictionary<string, Document>();

        public Connector() { }

        // ── IDMSConnector: Initialization ─────────────────────────────────
        void IDMSConnector.Init(object application, string LangCode)
        {
            if (!_initialized)
            {
                _languageCode = LangCode;
                try
                {
                    CultureInfo cultureInfo = Langs.Iso639_3ToCulture(_languageCode);
                    if (!cultureInfo.IsNeutralCulture)
                        Thread.CurrentThread.CurrentCulture = cultureInfo;
                    Thread.CurrentThread.CurrentUICulture = cultureInfo;
                }
                catch { }

                _menuItems = MenuItemList.Create();
                _initialized = true;
            }
        }

        void IDMSConnector.Shutdown()
        {
            if (_initialized)
            {
                foreach (Document doc in _documents.Values)
                    doc.Close();
                _documents.Clear();
                _initialized = false;
            }
        }

        // ── IDMSConnector: Properties ─────────────────────────────────────
        string IDMSConnector.ConnectorName { get { return _connectorName; } }
        int IDMSConnector.ParentWindow { set { _parentWindow = (IntPtr)value; } }

        // ── IDMSConnector: Menu ───────────────────────────────────────────
        void IDMSConnector.MenuGetNumberOfItems(out int num, out string title)
        {
            num = _menuItems.Count;
            title = _connectorName;
        }

        void IDMSConnector.MenuGetMenuItem(int num,
            out int menuItemId, out string text, out string tooltip,
            out bool isPartOfToolbar, out CallbackType cbType,
            out int hIconBig, out int hIconSmall, out bool enabledWithoutDoc)
        {
            if (num < 0 || num >= _menuItems.Count)
            {
                throw new ArgumentOutOfRangeException("num");
            }

            MenuItem item = _menuItems[num];

            menuItemId = item.menuItemId;
            text = item.text;
            tooltip = item.tooltip;
            isPartOfToolbar = item.isPartOfToolbar;
            cbType = item.cbType;
            hIconBig = item.hIconBig.ToInt32();
            hIconSmall = item.hIconSmall.ToInt32();
            enabledWithoutDoc = item.enabledWithoutDoc;

            System.IO.File.AppendAllText(@"C:\Temp\TAConnectorDebug.txt", DateTime.Now + " | MenuGetMenuItem: " + "menuItemId=" + menuItemId +  " text=" + text + " hIconBig=" + hIconBig.ToString("X") + " hIconSmall=" + hIconSmall.ToString("X") +  "\r\n");
        }

        bool IDMSConnector.MenuGetItemState(int menuItemId, string docId) => true;

        void IDMSConnector.MenuAction(int menuItemId, string docId)
        {
            if (menuItemId == (int)ItemId.Configure)
            {
                IntPtr owner = _parentWindow;
                System.Threading.Thread thread = new System.Threading.Thread(() =>
                {
                    var psForm = new ProcessSelectionForm();
                    psForm.ShowDialog(new WindowWrapper(owner));
                    if (owner != IntPtr.Zero)
                        SetForegroundWindow(owner);
                });
                thread.SetApartmentState(System.Threading.ApartmentState.STA);
                thread.Start();
                // No Join() — do not block Power PDF UI thread
            }
        }


        // ── IDMSConnector: DocAddNew — Main Action ────────────────────────
        string IDMSConnector.DocAddNew(string sourceFile, string title, string[] docProperties)
        {
            // ── Step 1: Load configuration ────────────────────────────
            List<TAConnection> connections = ConnectionStore.Load();
            TAConnection conn = ConnectionStore.GetActive(connections);

            string sdkUrl = conn.SdkUrl;
            int authType = conn.AuthType;
            string userId = conn.UserId;
            string password = conn.Password;

            // ── Step 2: Resolve document name ─────────────────────────
            string documentName = string.IsNullOrEmpty(title)
                ? Path.GetFileName(sourceFile)
                : title;

            // ── Step 3: Read show confirmation flag (default ON) ───────
            bool showConfirmation = ConnectionStore.ShowConfirmation;

            // ── Step 4: Choose the profile (document type → process) ───
            List<TAProfile> profiles = conn.Profiles;

            if (string.IsNullOrEmpty(sdkUrl))
            {
                MessageBox.Show(
                    "No TotalAgility connection has been configured.\n\n" +
                    "Please enter the SDK URL and credentials under " +
                    "File \u2192 Options \u2192 Connectors \u2192 TotalAgility.",
                    "Connection Not Configured",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return "";
            }

            if (profiles.Count == 0)
            {
                MessageBox.Show(
                    "No document type has been configured for \"" + conn.Name + "\".\n\n" +
                    "Please click the Configure button in the TotalAgility ribbon " +
                    "to add a document type and select its process before sending.",
                    "Process Not Configured",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return "";
            }

            TAProfile profile;

            // Only a single connection with a single profile and the confirmation switched off
            // sends straight away. With several connections the dialog always shows which
            // environment the PDF goes to; with several profiles the user picks the document type.
            bool singleConnection = connections.Count == 1;
            if (singleConnection && profiles.Count == 1 && !showConfirmation)
            {
                profile = profiles[0];
            }
            else
            {
                using (var chooser = new SendProfileForm(documentName, conn, singleConnection))
                {
                    chooser.ShowDialog(new WindowWrapper(_parentWindow));

                    if (chooser.Result != SendProfileForm.SendResult.Send)
                        return "";

                    profile = chooser.SelectedProfile;

                    if (singleConnection && profiles.Count == 1)
                        ConnectionStore.ShowConfirmation = !chooser.DoNotShowAgain;
                }
            }

            ConnectionStore.SetLastProfile(conn.Id, profile.Name);

            string processId = profile.ProcessId;
            string processName = profile.ProcessName;
            string processLabel = processName + " (" + conn.Name + ")";
            List<TAInputVariable> inputVariables =
                profile.Variables ?? new List<TAInputVariable>();

            // ── Step 6: Show LoadingForm ───────────────────────────────
            LoadingForm loadingForm = null;
            Thread loadingThread = new Thread(() =>
            {
                loadingForm = new LoadingForm();
                Application.Run(loadingForm);
            });
            loadingThread.SetApartmentState(ApartmentState.STA);
            loadingThread.Start();

            // ── Step 7: Authenticate and call TotalAgility API ────────
            // TEST DELAY — 5 seconds to verify LoadingForm animation
            // Remove this Thread.Sleep before production release
            Thread.Sleep(5000);

            TAJobResult jobResult = null;
            try
            {
                var taService = new TotalAgilityService(sdkUrl);

                // Try saved SessionId first — skip LogOn if still valid
                TASession session = null;
                string savedSessionId = conn.SessionId;

                if (!string.IsNullOrEmpty(savedSessionId))
                {
                    try
                    {
                        // Validate by calling a lightweight API
                        taService.GetProcesses(savedSessionId);
                        session = new TASession
                        {
                            SessionId = savedSessionId,
                            IsValid = true
                        };
                    }
                    catch
                    {
                        // SessionId expired — re-authenticate below
                        session = null;
                    }
                }

                if (session == null)
                {
                    if (authType == AUTH_STANDARD)
                    {
                        session = taService.LogOn(userId, password);
                    }
                    else if (authType == AUTH_WINDOWS)
                    {
                        string winUser = conn.WindowsUsername;
                        if (string.IsNullOrEmpty(winUser))
                            winUser = Environment.UserName;
                        session = taService.LogOnWithWindowsAD(winUser);
                    }
                    else if (authType == AUTH_FEDERATED)
                    {
                        string taBaseUrl = taService.GetTaBaseUrl(sdkUrl);
                        session = taService.LogOnWithFederated(
                            taBaseUrl, new WindowWrapper(_parentWindow));
                    }
                    conn.SessionId = session.SessionId;
                    ConnectionStore.SaveSession(conn);
                }

                byte[] pdfBytes = File.ReadAllBytes(sourceFile);

                TAProcessIdentity processIdentity = new TAProcessIdentity
                {
                    Id = processId,
                    Name = processName
                };

                jobResult = taService.CreateJobWithDocuments(
                    session.SessionId,
                    processIdentity,
                    pdfBytes,
                    inputVariables);

                jobResult.DocumentName = documentName;
                jobResult.ProcessName = processLabel;
            }
            catch (Exception ex)
            {
                jobResult = new TAJobResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ProcessName = processLabel,
                    DocumentName = documentName
                };
            }

            // ── Step 8: Close LoadingForm, show Result ─────────────────
            TAJobResult resultToShow = jobResult;
            IntPtr ownerHwnd = _parentWindow;
            Thread resultThread = new Thread(() =>
            {
                using (var resultForm = new TAResultForm(resultToShow))
                {
                    if (ownerHwnd != IntPtr.Zero)
                        resultForm.ShowDialog(new WindowWrapper(ownerHwnd));
                    else
                        resultForm.ShowDialog();
                }
            });
            resultThread.SetApartmentState(ApartmentState.STA);

            try
            {
                resultThread.Start();
                if (loadingForm != null && loadingForm.InvokeRequired)
                    loadingForm.Invoke(new Action(() => loadingForm.Close()));
                else if (loadingForm != null)
                    loadingForm.Close();
            }
            catch { }

            loadingThread.Join();
            resultThread.Join();

            // ── Step 9: Restore focus to Power PDF ────────────────────
            if (_parentWindow != IntPtr.Zero)
                SetForegroundWindow(_parentWindow);

            return "";
        }

        // ── IDMSConnector: Document Lifecycle ─────────────────────────────
        void IDMSConnector.DocOpen(string docId, OpenMode mode)
        {
            if (_documents.TryGetValue(docId, out Document doc))
                doc.Open(mode);
        }

        void IDMSConnector.DocClose(string docId, DMSConnector.CloseReason reason)
        {
            if (_documents.TryGetValue(docId, out Document doc))
            {
                doc.Close();
                _documents.Remove(docId);
            }
        }

        void IDMSConnector.DocModified(string docId) { }

        string IDMSConnector.DocGetLocalFile(string docId)
        {
            return _documents.TryGetValue(docId, out Document doc)
                ? doc.LocalFileName : null;
        }

        OpenMode IDMSConnector.DocGetOpenMode(string docId)
        {
            return _documents.TryGetValue(docId, out Document doc)
                ? doc.OpenMode : OpenMode.OPEN_NONE;
        }

        string IDMSConnector.DocGetTitle(string docId)
        {
            return _documents.TryGetValue(docId, out Document doc)
                ? doc.Title : string.Empty;
        }

        void IDMSConnector.DocPrepareSave(string docId, int menuItemId,
            string[] docProperties, out string targetFileName)
        {
            targetFileName = Path.GetTempFileName();
        }

        void IDMSConnector.DocSave(string docId, string targetFileName,
            out string newDocId)
        {
            newDocId = null;
        }

        void IDMSConnector.DocSelectFiles(SelectType type,
            int MenuIndex, out string[] docIds)
        {
            docIds = new string[0];
        }

        string IDMSConnector.FileIsFromDms(string localFile)
        {
            throw new NotImplementedException();
        }

        // ── IDMSConnector: Property Page ──────────────────────────────────
        IPropertyPageHandler IDMSConnector.PropertyPageHandler
        {
            get { return this; }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        int IPropertyPageHandler.Create(int parenthWnd)
        {
            ConfigurationControl p = new ConfigurationControl(this);
            SetParent(p.Handle, (IntPtr)parenthWnd);
            return p.Handle.ToInt32();
        }

        void IPropertyPageHandler.ShowHelp(int hWnd)
        {
            ConfigurationControl.GetSheet(hWnd)?.ShowHelp();
        }

        void IPropertyPageHandler.CheckData(int hWnd)
        {
            ConfigurationControl.GetSheet(hWnd)?.CheckData();
        }

        void IPropertyPageHandler.UpdateData(int hWnd)
        {
            ConfigurationControl.GetSheet(hWnd)?.UpdateData();
        }

        // ── COM Registration Helpers ──────────────────────────────────────
        private static RegistryKey OpenOrCreateSubkey(RegistryKey parent, string name)
        {
            RegistryKey rk = parent.OpenSubKey(name, true)
                          ?? parent.CreateSubKey(name);
            if (rk == null)
                throw new COMException(
                    "Cannot register connector — check admin permissions.");
            return rk;
        }

        private static string GetProgId(Type t)
        {
            var attrs = t.GetCustomAttributes(typeof(ProgIdAttribute), true);
            if (attrs.Length == 0)
                throw new InvalidOperationException("ProgId attribute missing.");
            return ((ProgIdAttribute)attrs[0]).Value;
        }
    }
}