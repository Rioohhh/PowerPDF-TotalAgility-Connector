namespace PPDF.TotalAgility.Connector
{
    partial class ConfigurationControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.labelSectionCredentials = new System.Windows.Forms.Label();
            this.labelEnvironment = new System.Windows.Forms.Label();
            this.comboBoxConnection = new System.Windows.Forms.ComboBox();
            this.buttonNewConnection = new System.Windows.Forms.Button();
            this.buttonDeleteConnection = new System.Windows.Forms.Button();
            this.labelConnName = new System.Windows.Forms.Label();
            this.textBoxConnName = new System.Windows.Forms.TextBox();
            this.labelSdkUrl = new System.Windows.Forms.Label();
            this.textBoxSdkUrl = new System.Windows.Forms.TextBox();
            this.groupBoxAuth = new System.Windows.Forms.GroupBox();
            this.radioButtonStandard = new System.Windows.Forms.RadioButton();
            this.radioButtonWindowsAD = new System.Windows.Forms.RadioButton();
            this.radioButtonFederated = new System.Windows.Forms.RadioButton();
            this.labelUserId = new System.Windows.Forms.Label();
            this.textBoxUserId = new System.Windows.Forms.TextBox();
            this.labelPassword = new System.Windows.Forms.Label();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.labelWindowsUser = new System.Windows.Forms.Label();
            this.textBoxWindowsUser = new System.Windows.Forms.TextBox();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.checkBoxShowConfirmation = new System.Windows.Forms.CheckBox();
            this.buttonSave = new System.Windows.Forms.Button();
            this.groupBoxAuth.SuspendLayout();
            this.SuspendLayout();
            //
            // labelSectionCredentials
            //
            this.labelSectionCredentials.BackColor = System.Drawing.Color.SteelBlue;
            this.labelSectionCredentials.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.labelSectionCredentials.ForeColor = System.Drawing.Color.White;
            this.labelSectionCredentials.Location = new System.Drawing.Point(11, 10);
            this.labelSectionCredentials.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelSectionCredentials.Name = "labelSectionCredentials";
            this.labelSectionCredentials.Size = new System.Drawing.Size(547, 25);
            this.labelSectionCredentials.TabIndex = 0;
            this.labelSectionCredentials.Text = "  TotalAgility Credentials";
            this.labelSectionCredentials.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // labelEnvironment
            //
            this.labelEnvironment.AutoSize = true;
            this.labelEnvironment.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.labelEnvironment.Location = new System.Drawing.Point(20, 46);
            this.labelEnvironment.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelEnvironment.Name = "labelEnvironment";
            this.labelEnvironment.Size = new System.Drawing.Size(87, 16);
            this.labelEnvironment.TabIndex = 1;
            this.labelEnvironment.Text = "Environment:";
            //
            // comboBoxConnection
            //
            this.comboBoxConnection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxConnection.FormattingEnabled = true;
            this.comboBoxConnection.Location = new System.Drawing.Point(155, 43);
            this.comboBoxConnection.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBoxConnection.Name = "comboBoxConnection";
            this.comboBoxConnection.Size = new System.Drawing.Size(231, 24);
            this.comboBoxConnection.TabIndex = 2;
            this.comboBoxConnection.SelectedIndexChanged += new System.EventHandler(this.comboBoxConnection_SelectedIndexChanged);
            //
            // buttonNewConnection
            //
            this.buttonNewConnection.Location = new System.Drawing.Point(394, 42);
            this.buttonNewConnection.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonNewConnection.Name = "buttonNewConnection";
            this.buttonNewConnection.Size = new System.Drawing.Size(76, 26);
            this.buttonNewConnection.TabIndex = 3;
            this.buttonNewConnection.Text = "New";
            this.buttonNewConnection.UseVisualStyleBackColor = true;
            this.buttonNewConnection.Click += new System.EventHandler(this.buttonNewConnection_Click);
            //
            // buttonDeleteConnection
            //
            this.buttonDeleteConnection.Location = new System.Drawing.Point(478, 42);
            this.buttonDeleteConnection.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonDeleteConnection.Name = "buttonDeleteConnection";
            this.buttonDeleteConnection.Size = new System.Drawing.Size(76, 26);
            this.buttonDeleteConnection.TabIndex = 4;
            this.buttonDeleteConnection.Text = "Delete";
            this.buttonDeleteConnection.UseVisualStyleBackColor = true;
            this.buttonDeleteConnection.Click += new System.EventHandler(this.buttonDeleteConnection_Click);
            //
            // labelConnName
            //
            this.labelConnName.AutoSize = true;
            this.labelConnName.Location = new System.Drawing.Point(20, 79);
            this.labelConnName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelConnName.Name = "labelConnName";
            this.labelConnName.Size = new System.Drawing.Size(45, 16);
            this.labelConnName.TabIndex = 5;
            this.labelConnName.Text = "Name:";
            //
            // textBoxConnName
            //
            this.textBoxConnName.Location = new System.Drawing.Point(155, 76);
            this.textBoxConnName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxConnName.Name = "textBoxConnName";
            this.textBoxConnName.Size = new System.Drawing.Size(399, 22);
            this.textBoxConnName.TabIndex = 6;
            this.textBoxConnName.TextChanged += new System.EventHandler(this.textBoxConnName_TextChanged);
            //
            // labelSdkUrl
            // 
            this.labelSdkUrl.AutoSize = true;
            this.labelSdkUrl.Location = new System.Drawing.Point(20, 110);
            this.labelSdkUrl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelSdkUrl.Name = "labelSdkUrl";
            this.labelSdkUrl.Size = new System.Drawing.Size(88, 16);
            this.labelSdkUrl.TabIndex = 7;
            this.labelSdkUrl.Text = "TA SDK URL:";
            //
            // textBoxSdkUrl
            //
            this.textBoxSdkUrl.Location = new System.Drawing.Point(155, 107);
            this.textBoxSdkUrl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxSdkUrl.Name = "textBoxSdkUrl";
            this.textBoxSdkUrl.Size = new System.Drawing.Size(399, 22);
            this.textBoxSdkUrl.TabIndex = 8;
            //
            // groupBoxAuth
            //
            this.groupBoxAuth.Controls.Add(this.radioButtonStandard);
            this.groupBoxAuth.Controls.Add(this.radioButtonWindowsAD);
            this.groupBoxAuth.Controls.Add(this.radioButtonFederated);
            this.groupBoxAuth.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Bold);
            this.groupBoxAuth.ForeColor = System.Drawing.Color.SteelBlue;
            this.groupBoxAuth.Location = new System.Drawing.Point(11, 140);
            this.groupBoxAuth.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBoxAuth.Name = "groupBoxAuth";
            this.groupBoxAuth.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBoxAuth.Size = new System.Drawing.Size(547, 108);
            this.groupBoxAuth.TabIndex = 9;
            this.groupBoxAuth.TabStop = false;
            this.groupBoxAuth.Text = "Authentication Type";
            //
            // radioButtonStandard
            //
            this.radioButtonStandard.AutoSize = true;
            this.radioButtonStandard.Checked = true;
            this.radioButtonStandard.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F);
            this.radioButtonStandard.ForeColor = System.Drawing.Color.Black;
            this.radioButtonStandard.Location = new System.Drawing.Point(13, 25);
            this.radioButtonStandard.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.radioButtonStandard.Name = "radioButtonStandard";
            this.radioButtonStandard.Size = new System.Drawing.Size(110, 28);
            this.radioButtonStandard.TabIndex = 0;
            this.radioButtonStandard.TabStop = true;
            this.radioButtonStandard.Text = "Standard";
            this.radioButtonStandard.UseVisualStyleBackColor = true;
            this.radioButtonStandard.CheckedChanged += new System.EventHandler(this.radioButtonStandard_CheckedChanged);
            //
            // radioButtonWindowsAD
            //
            this.radioButtonWindowsAD.AutoSize = true;
            this.radioButtonWindowsAD.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F);
            this.radioButtonWindowsAD.ForeColor = System.Drawing.Color.Black;
            this.radioButtonWindowsAD.Location = new System.Drawing.Point(13, 54);
            this.radioButtonWindowsAD.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.radioButtonWindowsAD.Name = "radioButtonWindowsAD";
            this.radioButtonWindowsAD.Size = new System.Drawing.Size(115, 22);
            this.radioButtonWindowsAD.TabIndex = 1;
            this.radioButtonWindowsAD.Text = "Windows AD";
            this.radioButtonWindowsAD.UseVisualStyleBackColor = true;
            this.radioButtonWindowsAD.CheckedChanged += new System.EventHandler(this.radioButtonWindowsAD_CheckedChanged);
            //
            // radioButtonFederated
            //
            this.radioButtonFederated.AutoSize = true;
            this.radioButtonFederated.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F);
            this.radioButtonFederated.ForeColor = System.Drawing.Color.Black;
            this.radioButtonFederated.Location = new System.Drawing.Point(13, 81);
            this.radioButtonFederated.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.radioButtonFederated.Name = "radioButtonFederated";
            this.radioButtonFederated.Size = new System.Drawing.Size(152, 22);
            this.radioButtonFederated.TabIndex = 2;
            this.radioButtonFederated.Text = "Federated Security";
            this.radioButtonFederated.UseVisualStyleBackColor = true;
            this.radioButtonFederated.CheckedChanged += new System.EventHandler(this.radioButtonFederated_CheckedChanged);
            //
            // labelUserId
            //
            this.labelUserId.AutoSize = true;
            this.labelUserId.Location = new System.Drawing.Point(20, 263);
            this.labelUserId.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelUserId.Name = "labelUserId";
            this.labelUserId.Size = new System.Drawing.Size(55, 16);
            this.labelUserId.TabIndex = 10;
            this.labelUserId.Text = "User ID:";
            //
            // textBoxUserId
            //
            this.textBoxUserId.Location = new System.Drawing.Point(155, 261);
            this.textBoxUserId.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxUserId.Name = "textBoxUserId";
            this.textBoxUserId.Size = new System.Drawing.Size(399, 22);
            this.textBoxUserId.TabIndex = 11;
            //
            // labelPassword
            //
            this.labelPassword.AutoSize = true;
            this.labelPassword.Location = new System.Drawing.Point(20, 297);
            this.labelPassword.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(70, 16);
            this.labelPassword.TabIndex = 12;
            this.labelPassword.Text = "Password:";
            //
            // textBoxPassword
            //
            this.textBoxPassword.Location = new System.Drawing.Point(155, 294);
            this.textBoxPassword.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.Size = new System.Drawing.Size(399, 22);
            this.textBoxPassword.TabIndex = 13;
            this.textBoxPassword.UseSystemPasswordChar = true;
            //
            // labelWindowsUser
            //
            this.labelWindowsUser.AutoSize = true;
            this.labelWindowsUser.Location = new System.Drawing.Point(20, 263);
            this.labelWindowsUser.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelWindowsUser.Name = "labelWindowsUser";
            this.labelWindowsUser.Size = new System.Drawing.Size(97, 16);
            this.labelWindowsUser.TabIndex = 14;
            this.labelWindowsUser.Text = "Windows User:";
            this.labelWindowsUser.Visible = false;
            //
            // textBoxWindowsUser
            //
            this.textBoxWindowsUser.BackColor = System.Drawing.SystemColors.Control;
            this.textBoxWindowsUser.Location = new System.Drawing.Point(155, 261);
            this.textBoxWindowsUser.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxWindowsUser.Name = "textBoxWindowsUser";
            this.textBoxWindowsUser.ReadOnly = true;
            this.textBoxWindowsUser.Size = new System.Drawing.Size(399, 22);
            this.textBoxWindowsUser.TabIndex = 15;
            this.textBoxWindowsUser.Visible = false;
            //
            // buttonConnect
            //
            this.buttonConnect.Location = new System.Drawing.Point(390, 336);
            this.buttonConnect.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(164, 28);
            this.buttonConnect.TabIndex = 16;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            //
            // checkBoxShowConfirmation
            //
            this.checkBoxShowConfirmation.AutoSize = true;
            this.checkBoxShowConfirmation.Checked = true;
            this.checkBoxShowConfirmation.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxShowConfirmation.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F);
            this.checkBoxShowConfirmation.Location = new System.Drawing.Point(24, 336);
            this.checkBoxShowConfirmation.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkBoxShowConfirmation.Name = "checkBoxShowConfirmation";
            this.checkBoxShowConfirmation.Size = new System.Drawing.Size(255, 22);
            this.checkBoxShowConfirmation.TabIndex = 17;
            this.checkBoxShowConfirmation.Text = "Show confirmation before sending";
            //
            // buttonSave
            //
            this.buttonSave.BackColor = System.Drawing.Color.SteelBlue;
            this.buttonSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.buttonSave.ForeColor = System.Drawing.Color.White;
            this.buttonSave.Location = new System.Drawing.Point(357, 406);
            this.buttonSave.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(200, 28);
            this.buttonSave.TabIndex = 18;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = false;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            //
            // ConfigurationControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.labelSectionCredentials);
            this.Controls.Add(this.labelEnvironment);
            this.Controls.Add(this.comboBoxConnection);
            this.Controls.Add(this.buttonNewConnection);
            this.Controls.Add(this.buttonDeleteConnection);
            this.Controls.Add(this.labelConnName);
            this.Controls.Add(this.textBoxConnName);
            this.Controls.Add(this.labelSdkUrl);
            this.Controls.Add(this.textBoxSdkUrl);
            this.Controls.Add(this.groupBoxAuth);
            this.Controls.Add(this.labelUserId);
            this.Controls.Add(this.textBoxUserId);
            this.Controls.Add(this.labelPassword);
            this.Controls.Add(this.textBoxPassword);
            this.Controls.Add(this.labelWindowsUser);
            this.Controls.Add(this.textBoxWindowsUser);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.checkBoxShowConfirmation);
            this.Controls.Add(this.buttonSave);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "ConfigurationControl";
            this.Size = new System.Drawing.Size(600, 458);
            this.Load += new System.EventHandler(this.ConfigurationControl_Load);
            this.groupBoxAuth.ResumeLayout(false);
            this.groupBoxAuth.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label labelSectionCredentials;
        private System.Windows.Forms.Label labelEnvironment;
        private System.Windows.Forms.ComboBox comboBoxConnection;
        private System.Windows.Forms.Button buttonNewConnection;
        private System.Windows.Forms.Button buttonDeleteConnection;
        private System.Windows.Forms.Label labelConnName;
        private System.Windows.Forms.TextBox textBoxConnName;
        private System.Windows.Forms.Label labelSdkUrl;
        private System.Windows.Forms.TextBox textBoxSdkUrl;
        private System.Windows.Forms.GroupBox groupBoxAuth;
        private System.Windows.Forms.RadioButton radioButtonStandard;
        private System.Windows.Forms.RadioButton radioButtonWindowsAD;
        private System.Windows.Forms.RadioButton radioButtonFederated;
        private System.Windows.Forms.Label labelUserId;
        private System.Windows.Forms.TextBox textBoxUserId;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Label labelWindowsUser;
        private System.Windows.Forms.TextBox textBoxWindowsUser;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.CheckBox checkBoxShowConfirmation;
        private System.Windows.Forms.Button buttonSave;
    }
}