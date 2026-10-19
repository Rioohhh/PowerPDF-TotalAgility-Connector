using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PPDF.TotalAgility.Connector
{
    /// <summary>
    /// Configuration control hosted inside Power PDF's
    /// File -> Options -> Connectors -> TotalAgility panel.
    ///
    /// Layout: Environment (connection) dropdown + name, SDK URL, Auth Type, Credentials (context),
    /// Connect, Confirmation checkbox, Save.
    /// Each connection (e.g. a customer environment) has its own URL, credentials and document types.
    /// The selected connection is the one the Send button uses.
    /// Process and Variables are configured via the Configure ribbon button.
    /// </summary>
    public partial class ConfigurationControl : UserControl
    {
        #region Auth Type Constants
        private const int AUTH_STANDARD = 0;
        private const int AUTH_WINDOWS = 1;
        private const int AUTH_FEDERATED = 2;
        #endregion

        #region State
        private Connector m_conn;
        private TotalAgilityService _taService;
        private List<TAConnection> _connections = new List<TAConnection>();
        private TAConnection _current;
        private bool _loading;
        private ToolTip _toolTip = new ToolTip();
        #endregion

        #region Construction
        public ConfigurationControl(Connector conn)
        {
            InitializeComponent();
            StoreSheet();
            m_conn = conn;
        }

        public void CheckData() { }
        public void UpdateData() { }
        public void ShowHelp() { }
        #endregion

        #region Window Style
        [Flags] public enum WS_STYLE : int { WS_CHILD = 0x40000000 }
        [Flags] public enum DS_STYLE : int { DS_CONTROL = 0x0400 }
        [Flags] public enum WS_EX_STYLE : int { WS_EX_CONTROLPARENT = 0x00010000 }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams p = base.CreateParams;
                p.Style = (int)WS_STYLE.WS_CHILD | (int)DS_STYLE.DS_CONTROL;
                p.ExStyle = (int)WS_EX_STYLE.WS_EX_CONTROLPARENT;
                return p;
            }
        }
        #endregion

        #region Sheet Management
        protected override void OnHandleDestroyed(EventArgs e)
        {
            ClearSheet();
            base.OnHandleDestroyed(e);
        }

        protected static System.Collections.Generic.Dictionary<IntPtr, ConfigurationControl>
            sheetList = new System.Collections.Generic.Dictionary<IntPtr, ConfigurationControl>();

        protected void StoreSheet()
        {
            if (!sheetList.ContainsKey(Handle))
                sheetList.Add(Handle, this);
        }

        protected void ClearSheet()
        {
            if (sheetList.ContainsKey(Handle))
                sheetList.Remove(Handle);
        }

        public static ConfigurationControl GetSheet(IntPtr hwnd)
        {
            ConfigurationControl c;
            sheetList.TryGetValue(hwnd, out c);
            return c;
        }

        public static ConfigurationControl GetSheet(int hWnd) =>
            GetSheet((IntPtr)hWnd);
        #endregion

        #region Form Load
        private void ConfigurationControl_Load(object sender, EventArgs e)
        {
            try
            {
                _connections = ConnectionStore.Load();
                _current = ConnectionStore.GetActive(_connections);

                RefreshConnectionCombo(_current);
                ShowConnection(_current);

                checkBoxShowConfirmation.Checked = ConnectionStore.ShowConfirmation;

                AutoConnect(_current);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load settings: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Connections — dropdown, name, New, Delete
        private void RefreshConnectionCombo(TAConnection select)
        {
            _loading = true;
            try
            {
                comboBoxConnection.Items.Clear();
                foreach (var c in _connections)
                    comboBoxConnection.Items.Add(c);
                comboBoxConnection.SelectedItem = select;
                buttonDeleteConnection.Enabled = _connections.Count > 1;
            }
            finally
            {
                _loading = false;
            }
        }

        /// <summary>Puts a connection's settings into the fields.</summary>
        private void ShowConnection(TAConnection c)
        {
            _loading = true;
            try
            {
                textBoxConnName.Text = c.Name;
                textBoxSdkUrl.Text = c.SdkUrl;
                textBoxUserId.Text = c.UserId;
                textBoxPassword.Text = c.Password;

                if (c.AuthType == AUTH_WINDOWS) radioButtonWindowsAD.Checked = true;
                else if (c.AuthType == AUTH_FEDERATED) radioButtonFederated.Checked = true;
                else radioButtonStandard.Checked = true;

                ShowAuthFields(c.AuthType);

                ClearFieldError(textBoxSdkUrl);
                ClearFieldError(textBoxUserId);
                ClearFieldError(textBoxPassword);
            }
            finally
            {
                _loading = false;
            }
        }

        private int SelectedAuthType()
        {
            return radioButtonWindowsAD.Checked ? AUTH_WINDOWS
                 : radioButtonFederated.Checked ? AUTH_FEDERATED
                 : AUTH_STANDARD;
        }

        /// <summary>Copies the fields back into the connection that is being shown.</summary>
        private void CommitCurrent()
        {
            if (_current == null) return;

            _current.Name = textBoxConnName.Text.Trim();
            _current.SdkUrl = textBoxSdkUrl.Text.Trim();
            _current.AuthType = SelectedAuthType();
            _current.UserId = textBoxUserId.Text.Trim();
            _current.Password = textBoxPassword.Text.Trim();
            if (_current.AuthType == AUTH_WINDOWS)
                _current.WindowsUsername = textBoxWindowsUser.Text;
        }

        private string NewConnectionName()
        {
            const string baseName = "New connection";
            string name = baseName;
            int n = 2;
            while (FindByName(name, null) != null)
                name = baseName + " " + n++;
            return name;
        }

        private TAConnection FindByName(string name, TAConnection except)
        {
            foreach (var c in _connections)
                if (!ReferenceEquals(c, except) &&
                    string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase))
                    return c;
            return null;
        }

        private void comboBoxConnection_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loading) return;

            var target = comboBoxConnection.SelectedItem as TAConnection;
            if (target == null || ReferenceEquals(target, _current)) return;

            CommitCurrent();
            _current = target;
            ShowConnection(_current);

            // Switching to a connection that is already saved takes effect at once, so the
            // Send button uses it. A connection that was just created becomes active on Save.
            if (ConnectionStore.Get(_current.Id) != null)
                ConnectionStore.SetActive(_current.Id);
        }

        private void textBoxConnName_TextChanged(object sender, EventArgs e)
        {
            if (_loading || _current == null) return;

            _current.Name = textBoxConnName.Text.Trim();

            // Show the new name in the dropdown
            int idx = comboBoxConnection.Items.IndexOf(_current);
            if (idx < 0) return;
            _loading = true;
            try
            {
                comboBoxConnection.Items[idx] = _current;
                comboBoxConnection.SelectedIndex = idx;
            }
            finally
            {
                _loading = false;
            }
        }

        private void buttonNewConnection_Click(object sender, EventArgs e)
        {
            CommitCurrent();

            var c = new TAConnection { Name = NewConnectionName() };
            _connections.Add(c);
            _current = c;
            RefreshConnectionCombo(c);
            ShowConnection(c);
            textBoxConnName.Focus();
            textBoxConnName.SelectAll();
        }

        private void buttonDeleteConnection_Click(object sender, EventArgs e)
        {
            if (_current == null || _connections.Count <= 1) return;

            DialogResult confirm = MessageBox.Show(
                "Delete the connection \"" + _current.Name + "\" including its document types " +
                "and saved credentials?\n\nThis is applied when you click Save.",
                "TotalAgility", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            int idx = _connections.IndexOf(_current);
            _connections.Remove(_current);
            _current = _connections[Math.Min(idx, _connections.Count - 1)];
            RefreshConnectionCombo(_current);
            ShowConnection(_current);
        }
        #endregion

        #region Auth Type — Show/Hide Fields
        private void ShowAuthFields(int authType)
        {
            labelUserId.Visible = authType == AUTH_STANDARD;
            textBoxUserId.Visible = authType == AUTH_STANDARD;
            labelPassword.Visible = authType == AUTH_STANDARD;
            textBoxPassword.Visible = authType == AUTH_STANDARD;

            labelWindowsUser.Visible = authType == AUTH_WINDOWS;
            textBoxWindowsUser.Visible = authType == AUTH_WINDOWS;

            if (authType == AUTH_WINDOWS)
                textBoxWindowsUser.Text =
                    Environment.UserDomainName + "\\" + Environment.UserName;
        }

        private void radioButtonStandard_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonStandard.Checked) ShowAuthFields(AUTH_STANDARD);
        }

        private void radioButtonWindowsAD_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonWindowsAD.Checked) ShowAuthFields(AUTH_WINDOWS);
        }

        private void radioButtonFederated_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonFederated.Checked) ShowAuthFields(AUTH_FEDERATED);
        }
        #endregion

        #region Auto Connect
        private void AutoConnect(TAConnection c)
        {
            if (string.IsNullOrEmpty(c.SdkUrl)) return;

            try
            {
                Cursor = Cursors.WaitCursor;
                _taService = new TotalAgilityService(c.SdkUrl);

                if (c.AuthType == AUTH_STANDARD)
                {
                    if (string.IsNullOrEmpty(c.UserId) || string.IsNullOrEmpty(c.Password))
                        return;
                    TASession session = _taService.LogOn(c.UserId, c.Password);
                    c.SessionId = session.SessionId;
                    ConnectionStore.SaveSession(c);
                }
                else if (c.AuthType == AUTH_WINDOWS)
                {
                    TASession session = _taService.LogOnWithWindowsAD(Environment.UserName);
                    c.SessionId = session.SessionId;
                    ConnectionStore.SaveSession(c);
                }
                // Federated — do not auto-connect, requires browser interaction
            }
            catch { /* silent — user can click Connect manually */ }
            finally { Cursor = Cursors.Default; }
        }
        #endregion

        #region Connect Button — async to keep UI responsive
        private async void buttonConnect_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxSdkUrl.Text))
            {
                ShowFieldError(textBoxSdkUrl, "Please enter the TA SDK URL.");
                return;
            }

            int authType = SelectedAuthType();

            if (authType == AUTH_STANDARD)
            {
                if (string.IsNullOrWhiteSpace(textBoxUserId.Text))
                { ShowFieldError(textBoxUserId, "Please enter the User ID."); return; }
                if (string.IsNullOrWhiteSpace(textBoxPassword.Text))
                { ShowFieldError(textBoxPassword, "Please enter the Password."); return; }
            }

            // The connection this click was made for, even if the dropdown changes meanwhile
            TAConnection target = _current;

            try
            {
                Cursor = Cursors.WaitCursor;
                buttonConnect.Enabled = false;
                ClearFieldError(textBoxSdkUrl);

                string sdkUrl = textBoxSdkUrl.Text.Trim();
                _taService = new TotalAgilityService(sdkUrl);
                TotalAgilityService service = _taService;
                TASession session = null;

                if (authType == AUTH_STANDARD)
                {
                    string userId = textBoxUserId.Text.Trim();
                    string password = textBoxPassword.Text.Trim();
                    ClearFieldError(textBoxUserId);
                    ClearFieldError(textBoxPassword);

                    // Run on background thread — keeps UI responsive
                    session = await Task.Run(() => service.LogOn(userId, password));
                }
                else if (authType == AUTH_WINDOWS)
                {
                    string winUser = Environment.UserName;
                    session = await Task.Run(() => service.LogOnWithWindowsAD(winUser));
                }
                else if (authType == AUTH_FEDERATED)
                {
                    string taBaseUrl = service.GetTaBaseUrl(sdkUrl);
                    IntPtr handle = this.Handle;

                    // Use TaskCompletionSource so STA thread runs WebView2 browser
                    // while UI thread stays responsive via await
                    var tcs = new TaskCompletionSource<TASession>();

                    Thread fedThread = new Thread(() =>
                    {
                        try
                        {
                            TASession result = service.LogOnWithFederated(
                                taBaseUrl, new WindowWrapper(handle));
                            tcs.SetResult(result);
                        }
                        catch (Exception ex)
                        {
                            tcs.SetException(ex);
                        }
                    });
                    fedThread.SetApartmentState(ApartmentState.STA);
                    fedThread.Start();

                    // Await without blocking UI thread
                    session = await tcs.Task;
                }

                if (session == null || !session.IsValid)
                    throw new Exception("Authentication failed.");

                target.SessionId = session.SessionId;
                ConnectionStore.SaveSession(target);

                MessageBox.Show(
                    $"Connected successfully to \"{target.Name}\"!\nWelcome, {session.DisplayName}.",
                    "Connected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection failed:\n\n" + ex.Message,
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                buttonConnect.Enabled = true;
            }
        }
        #endregion

        #region Save Button
        private void buttonSave_Click(object sender, EventArgs e)
        {
            CommitCurrent();

            // Check every connection, not just the visible one
            foreach (var c in _connections)
            {
                string problem = null;
                if (string.IsNullOrWhiteSpace(c.Name))
                    problem = "Every connection needs a name.";
                else if (FindByName(c.Name, c) != null)
                    problem = "The name \"" + c.Name + "\" is used more than once.";
                else if (string.IsNullOrWhiteSpace(c.SdkUrl))
                    problem = "The SDK URL of \"" + c.Name + "\" is required.";

                if (problem == null) continue;

                if (!ReferenceEquals(c, _current))
                {
                    _current = c;
                    RefreshConnectionCombo(c);
                    ShowConnection(c);
                }
                MessageBox.Show(problem, "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                ConnectionStore.SaveAll(_connections);
                ConnectionStore.SetActive(_current.Id);
                ConnectionStore.ShowConfirmation = checkBoxShowConfirmation.Checked;

                MessageBox.Show(
                    "Settings saved successfully.\n\nActive connection: " + _current.Name,
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save settings:\n\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Field Error Helpers
        private void ShowFieldError(TextBox txt, string message)
        {
            txt.BackColor = Color.FromArgb(255, 220, 220);
            MessageBox.Show(message, "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txt.Focus();
        }

        private void ClearFieldError(Control ctrl)
        {
            ctrl.BackColor = SystemColors.Window;
            _toolTip.SetToolTip(ctrl, "");
        }
        #endregion
    }
}
