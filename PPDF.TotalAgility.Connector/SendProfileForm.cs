using System;
using System.Drawing;
using System.Windows.Forms;

namespace PPDF.TotalAgility.Connector
{
    /// <summary>
    /// Shown when the user clicks "Send to TotalAgility". A coloured banner names the
    /// environment (connection) the PDF will go to, and the user picks the document type
    /// (profile) that decides which process receives it.
    ///
    ///   [ Sending to: Customer A  -  customera.tungstencloud.com ]
    ///   Send "[DocumentName]" as which document type?
    ///   [ Factuur            ]
    ///   [ Kantoorartikelen   ]
    ///   [ Send ]  [ Manage... ]  [ Cancel ]
    ///   ☐ Do not show this message again   (only offered when there is a single connection and profile)
    ///
    /// Built in code (no designer file) because the layout depends on the profile count.
    /// </summary>
    public class SendProfileForm : Form
    {
        public enum SendResult { Send, Cancel }

        // Dark enough for white text; every environment gets one colour, stable across sessions
        private static readonly Color[] BannerColors =
        {
            Color.FromArgb(0x1F, 0x6F, 0x8B), Color.FromArgb(0x8B, 0x3A, 0x62),
            Color.FromArgb(0x2E, 0x7D, 0x32), Color.FromArgb(0xB0, 0x5A, 0x00),
            Color.FromArgb(0x5B, 0x3F, 0x9E), Color.FromArgb(0xA6, 0x2B, 0x2B),
            Color.FromArgb(0x00, 0x69, 0x5C), Color.FromArgb(0x4A, 0x52, 0x5A)
        };

        private readonly string _documentName;
        private readonly bool _singleConnection;
        private TAConnection _conn;

        private Label lblBanner;
        private Label lblMessage;
        private ListBox listProfiles;
        private Label lblProcess;
        private CheckBox checkBoxDoNotShow;
        private Button btnSend;
        private Button btnManage;
        private Button btnCancel;

        public SendResult Result { get; private set; } = SendResult.Cancel;
        public TAProfile SelectedProfile { get; private set; }
        public bool DoNotShowAgain => checkBoxDoNotShow.Visible && checkBoxDoNotShow.Checked;

        public SendProfileForm(string documentName, TAConnection connection, bool singleConnection)
        {
            _documentName = documentName;
            _conn = connection;
            _singleConnection = singleConnection;

            BuildUi();
            FillList(_conn.LastProfile);
        }

        /// <summary>Same name always gives the same colour.</summary>
        internal static Color BannerColorFor(string name)
        {
            int hash = 17;
            foreach (char ch in (name ?? "").ToLowerInvariant())
                hash = unchecked(hash * 31 + ch);
            return BannerColors[(hash & 0x7FFFFFFF) % BannerColors.Length];
        }

        private void BuildUi()
        {
            lblBanner = new Label
            {
                AutoSize = false,
                BackColor = BannerColorFor(_conn.Name),
                ForeColor = Color.White,
                Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold),
                Location = new Point(0, 0),
                Size = new Size(440, 52),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0)
            };
            ShowBanner();

            lblMessage = new Label
            {
                AutoSize = false,
                Font = new Font("Microsoft Sans Serif", 10F),
                Location = new Point(20, 62),
                Size = new Size(400, 36),
                Text = $"Send \"{_documentName}\" as which document type?",
                TextAlign = ContentAlignment.MiddleLeft
            };

            listProfiles = new ListBox
            {
                Font = new Font("Microsoft Sans Serif", 10F),
                Location = new Point(22, 104),
                Size = new Size(396, 120),
                IntegralHeight = false
            };
            listProfiles.SelectedIndexChanged += (s, e) => UpdateProcessLabel();
            listProfiles.DoubleClick += (s, e) => btnSend.PerformClick();

            lblProcess = new Label
            {
                AutoSize = false,
                ForeColor = Color.DimGray,
                Location = new Point(22, 230),
                Size = new Size(396, 20),
                TextAlign = ContentAlignment.MiddleLeft
            };

            btnSend = new Button
            {
                BackColor = Color.SteelBlue,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 260),
                Size = new Size(80, 28),
                Text = "Send",
                UseVisualStyleBackColor = false
            };
            btnSend.Click += btnSend_Click;

            btnManage = new Button
            {
                Location = new Point(112, 260),
                Size = new Size(100, 28),
                Text = "Manage...",
                UseVisualStyleBackColor = true
            };
            btnManage.Click += btnManage_Click;

            btnCancel = new Button
            {
                Location = new Point(224, 260),
                Size = new Size(80, 28),
                Text = "Cancel",
                UseVisualStyleBackColor = true
            };
            btnCancel.Click += (s, e) => { Result = SendResult.Cancel; Close(); };

            checkBoxDoNotShow = new CheckBox
            {
                AutoSize = true,
                Font = new Font("Microsoft Sans Serif", 8.5F),
                ForeColor = Color.DimGray,
                Location = new Point(22, 300),
                Text = "Do not show this message again",
                Visible = _singleConnection && _conn.Profiles.Count == 1
            };

            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(440, 330);
            Controls.Add(lblBanner);
            Controls.Add(lblMessage);
            Controls.Add(listProfiles);
            Controls.Add(lblProcess);
            Controls.Add(btnSend);
            Controls.Add(btnManage);
            Controls.Add(btnCancel);
            Controls.Add(checkBoxDoNotShow);
            AcceptButton = btnSend;
            CancelButton = btnCancel;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Send to TotalAgility — " + _conn.Name;
        }

        private void ShowBanner()
        {
            string host = _conn.Host;
            lblBanner.Text = "Sending to:  " + _conn.Name +
                (string.IsNullOrEmpty(host) || host == _conn.Name ? "" : "\r\n" + host);
            lblBanner.BackColor = BannerColorFor(_conn.Name);
        }

        private void FillList(string preselectName)
        {
            listProfiles.Items.Clear();
            foreach (var p in _conn.Profiles)
                listProfiles.Items.Add(p);

            TAProfile pre = _conn.FindProfile(preselectName);
            if (pre != null)
                listProfiles.SelectedItem = pre;
            else if (listProfiles.Items.Count > 0)
                listProfiles.SelectedIndex = 0;

            checkBoxDoNotShow.Visible = _singleConnection && _conn.Profiles.Count == 1;
            UpdateProcessLabel();
        }

        private void UpdateProcessLabel()
        {
            var p = listProfiles.SelectedItem as TAProfile;
            lblProcess.Text = p == null ? "" : "Process: " + p.ProcessName;
            btnSend.Enabled = p != null;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            SelectedProfile = listProfiles.SelectedItem as TAProfile;
            if (SelectedProfile == null) return;
            Result = SendResult.Send;
            Close();
        }

        private void btnManage_Click(object sender, EventArgs e)
        {
            string current = (listProfiles.SelectedItem as TAProfile)?.Name;
            string id = _conn.Id;

            using (var psForm = new ProcessSelectionForm(id))
                psForm.ShowDialog(this);

            // Pick up the profiles that were just edited
            TAConnection fresh = ConnectionStore.Get(id);
            if (fresh != null)
            {
                _conn = fresh;
                ShowBanner();
            }
            FillList(string.IsNullOrEmpty(_conn.LastProfile) ? current : _conn.LastProfile);
        }
    }
}
