using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PPDF.TotalAgility.Connector
{
    /// <summary>
    /// Shown when the user clicks "Send to TotalAgility". Lets the user pick the
    /// document type (profile) that decides which TotalAgility process receives the PDF.
    ///
    ///   Send "[DocumentName]" as:
    ///   [ Factuur            ]
    ///   [ Kantoorartikelen   ]
    ///   [ Send ]  [ Manage... ]  [ Cancel ]
    ///   ☐ Do not show this message again   (only offered when there is a single profile)
    ///
    /// Built in code (no designer file) because the layout depends on the profile count.
    /// </summary>
    public class SendProfileForm : Form
    {
        public enum SendResult { Send, Cancel }

        private readonly string _documentName;
        private List<TAProfile> _profiles;

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

        public SendProfileForm(string documentName, List<TAProfile> profiles,
            string preselectName)
        {
            _documentName = documentName;
            _profiles = profiles;

            BuildUi();
            FillList(preselectName);
        }

        private void BuildUi()
        {
            lblMessage = new Label
            {
                AutoSize = false,
                Font = new Font("Microsoft Sans Serif", 10F),
                Location = new Point(20, 16),
                Size = new Size(400, 36),
                Text = $"Send \"{_documentName}\" as which document type?",
                TextAlign = ContentAlignment.MiddleLeft
            };

            listProfiles = new ListBox
            {
                Font = new Font("Microsoft Sans Serif", 10F),
                Location = new Point(22, 58),
                Size = new Size(396, 120),
                IntegralHeight = false
            };
            listProfiles.SelectedIndexChanged += (s, e) => UpdateProcessLabel();
            listProfiles.DoubleClick += (s, e) => btnSend.PerformClick();

            lblProcess = new Label
            {
                AutoSize = false,
                ForeColor = Color.DimGray,
                Location = new Point(22, 184),
                Size = new Size(396, 20),
                TextAlign = ContentAlignment.MiddleLeft
            };

            btnSend = new Button
            {
                BackColor = Color.SteelBlue,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 214),
                Size = new Size(80, 28),
                Text = "Send",
                UseVisualStyleBackColor = false
            };
            btnSend.Click += btnSend_Click;

            btnManage = new Button
            {
                Location = new Point(112, 214),
                Size = new Size(100, 28),
                Text = "Manage...",
                UseVisualStyleBackColor = true
            };
            btnManage.Click += btnManage_Click;

            btnCancel = new Button
            {
                Location = new Point(224, 214),
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
                Location = new Point(22, 254),
                Text = "Do not show this message again",
                Visible = _profiles.Count == 1
            };

            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(440, 284);
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
            Text = "Send to TotalAgility";
        }

        private void FillList(string preselectName)
        {
            listProfiles.Items.Clear();
            foreach (var p in _profiles)
                listProfiles.Items.Add(p);

            TAProfile pre = ProfileStore.Find(_profiles, preselectName);
            if (pre != null)
                listProfiles.SelectedItem = pre;
            else if (listProfiles.Items.Count > 0)
                listProfiles.SelectedIndex = 0;

            checkBoxDoNotShow.Visible = _profiles.Count == 1;
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

            using (var psForm = new ProcessSelectionForm())
                psForm.ShowDialog(this);

            _profiles = ProfileStore.Load();
            string last = ProfileStore.GetLastUsedName();
            FillList(string.IsNullOrEmpty(last) ? current : last);
        }
    }
}
