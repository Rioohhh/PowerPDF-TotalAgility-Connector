namespace PPDF.TotalAgility.Connector
{
    partial class ProcessSelectionForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.labelSectionProfile = new System.Windows.Forms.Label();
            this.labelProfile = new System.Windows.Forms.Label();
            this.comboBoxProfile = new System.Windows.Forms.ComboBox();
            this.btnNewProfile = new System.Windows.Forms.Button();
            this.btnDeleteProfile = new System.Windows.Forms.Button();
            this.labelProfileName = new System.Windows.Forms.Label();
            this.textBoxProfileName = new System.Windows.Forms.TextBox();
            this.labelSectionProc = new System.Windows.Forms.Label();
            this.labelProcess = new System.Windows.Forms.Label();
            this.comboBoxProcess = new System.Windows.Forms.ComboBox();
            this.labelSectionVars = new System.Windows.Forms.Label();
            this.panelVariables = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancelForm = new System.Windows.Forms.Button();
            this.SuspendLayout();

            // labelSectionProfile
            this.labelSectionProfile.BackColor = System.Drawing.Color.SteelBlue;
            this.labelSectionProfile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.labelSectionProfile.ForeColor = System.Drawing.Color.White;
            this.labelSectionProfile.Location = new System.Drawing.Point(8, 8);
            this.labelSectionProfile.Name = "labelSectionProfile";
            this.labelSectionProfile.Size = new System.Drawing.Size(432, 20);
            this.labelSectionProfile.Text = "  Document Type (Profile)";
            this.labelSectionProfile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelSectionProfile.TabIndex = 0;

            // labelProfile
            this.labelProfile.AutoSize = true;
            this.labelProfile.Location = new System.Drawing.Point(10, 38);
            this.labelProfile.Name = "labelProfile";
            this.labelProfile.Text = "Profile:";
            this.labelProfile.TabIndex = 1;

            // comboBoxProfile
            this.comboBoxProfile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxProfile.Location = new System.Drawing.Point(100, 36);
            this.comboBoxProfile.Name = "comboBoxProfile";
            this.comboBoxProfile.Size = new System.Drawing.Size(230, 21);
            this.comboBoxProfile.TabIndex = 2;
            this.comboBoxProfile.SelectedIndexChanged += new System.EventHandler(this.comboBoxProfile_SelectedIndexChanged);

            // btnNewProfile
            this.btnNewProfile.Location = new System.Drawing.Point(336, 34);
            this.btnNewProfile.Name = "btnNewProfile";
            this.btnNewProfile.Size = new System.Drawing.Size(50, 24);
            this.btnNewProfile.TabIndex = 3;
            this.btnNewProfile.Text = "New";
            this.btnNewProfile.UseVisualStyleBackColor = true;
            this.btnNewProfile.Click += new System.EventHandler(this.btnNewProfile_Click);

            // btnDeleteProfile
            this.btnDeleteProfile.Location = new System.Drawing.Point(390, 34);
            this.btnDeleteProfile.Name = "btnDeleteProfile";
            this.btnDeleteProfile.Size = new System.Drawing.Size(50, 24);
            this.btnDeleteProfile.TabIndex = 4;
            this.btnDeleteProfile.Text = "Delete";
            this.btnDeleteProfile.UseVisualStyleBackColor = true;
            this.btnDeleteProfile.Click += new System.EventHandler(this.btnDeleteProfile_Click);

            // labelProfileName
            this.labelProfileName.AutoSize = true;
            this.labelProfileName.Location = new System.Drawing.Point(10, 68);
            this.labelProfileName.Name = "labelProfileName";
            this.labelProfileName.Text = "Name:";
            this.labelProfileName.TabIndex = 5;

            // textBoxProfileName
            this.textBoxProfileName.Location = new System.Drawing.Point(100, 66);
            this.textBoxProfileName.Name = "textBoxProfileName";
            this.textBoxProfileName.Size = new System.Drawing.Size(340, 20);
            this.textBoxProfileName.TabIndex = 6;

            // labelSectionProc
            this.labelSectionProc.BackColor = System.Drawing.Color.SteelBlue;
            this.labelSectionProc.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.labelSectionProc.ForeColor = System.Drawing.Color.White;
            this.labelSectionProc.Location = new System.Drawing.Point(8, 96);
            this.labelSectionProc.Name = "labelSectionProc";
            this.labelSectionProc.Size = new System.Drawing.Size(432, 20);
            this.labelSectionProc.Text = "  Process Configuration";
            this.labelSectionProc.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelSectionProc.TabIndex = 7;

            // labelProcess
            this.labelProcess.AutoSize = true;
            this.labelProcess.Location = new System.Drawing.Point(10, 126);
            this.labelProcess.Name = "labelProcess";
            this.labelProcess.Text = "Process:";
            this.labelProcess.TabIndex = 8;

            // comboBoxProcess
            this.comboBoxProcess.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.comboBoxProcess.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.comboBoxProcess.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.comboBoxProcess.Location = new System.Drawing.Point(100, 124);
            this.comboBoxProcess.Name = "comboBoxProcess";
            this.comboBoxProcess.Size = new System.Drawing.Size(340, 21);
            this.comboBoxProcess.TabIndex = 9;
            this.comboBoxProcess.SelectedIndexChanged += new System.EventHandler(this.comboBoxProcess_SelectedIndexChanged);
            this.comboBoxProcess.KeyDown += new System.Windows.Forms.KeyEventHandler(this.comboBoxProcess_KeyDown);
            this.comboBoxProcess.Leave += new System.EventHandler(this.comboBoxProcess_Leave);

            // labelSectionVars
            this.labelSectionVars.BackColor = System.Drawing.Color.SteelBlue;
            this.labelSectionVars.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.labelSectionVars.ForeColor = System.Drawing.Color.White;
            this.labelSectionVars.Location = new System.Drawing.Point(8, 154);
            this.labelSectionVars.Name = "labelSectionVars";
            this.labelSectionVars.Size = new System.Drawing.Size(432, 20);
            this.labelSectionVars.Text = "  Initialization Variables";
            this.labelSectionVars.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelSectionVars.TabIndex = 10;

            // panelVariables
            this.panelVariables.AutoScroll = true;
            this.panelVariables.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelVariables.Location = new System.Drawing.Point(8, 176);
            this.panelVariables.Name = "panelVariables";
            this.panelVariables.Size = new System.Drawing.Size(432, 200);
            this.panelVariables.TabIndex = 11;
            this.panelVariables.Enabled = false;

            // btnSave
            this.btnSave.BackColor = System.Drawing.Color.SteelBlue;
            this.btnSave.Enabled = false;
            this.btnSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(248, 388);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(90, 26);
            this.btnSave.TabIndex = 12;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

            // btnCancelForm
            this.btnCancelForm.Location = new System.Drawing.Point(350, 388);
            this.btnCancelForm.Name = "btnCancelForm";
            this.btnCancelForm.Size = new System.Drawing.Size(90, 26);
            this.btnCancelForm.TabIndex = 13;
            this.btnCancelForm.Text = "Cancel";
            this.btnCancelForm.UseVisualStyleBackColor = true;
            this.btnCancelForm.Click += new System.EventHandler(this.btnCancelForm_Click);

            // ProcessSelectionForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 426);
            this.Controls.Add(this.labelSectionProfile);
            this.Controls.Add(this.labelProfile);
            this.Controls.Add(this.comboBoxProfile);
            this.Controls.Add(this.btnNewProfile);
            this.Controls.Add(this.btnDeleteProfile);
            this.Controls.Add(this.labelProfileName);
            this.Controls.Add(this.textBoxProfileName);
            this.Controls.Add(this.labelSectionProc);
            this.Controls.Add(this.labelProcess);
            this.Controls.Add(this.comboBoxProcess);
            this.Controls.Add(this.labelSectionVars);
            this.Controls.Add(this.panelVariables);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCancelForm);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProcessSelectionForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Send to TotalAgility \u2014 Document Types";
            this.Load += new System.EventHandler(this.ProcessSelectionForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label labelSectionProfile;
        private System.Windows.Forms.Label labelProfile;
        private System.Windows.Forms.ComboBox comboBoxProfile;
        private System.Windows.Forms.Button btnNewProfile;
        private System.Windows.Forms.Button btnDeleteProfile;
        private System.Windows.Forms.Label labelProfileName;
        private System.Windows.Forms.TextBox textBoxProfileName;
        private System.Windows.Forms.Label labelSectionProc;
        private System.Windows.Forms.Label labelProcess;
        private System.Windows.Forms.ComboBox comboBoxProcess;
        private System.Windows.Forms.Label labelSectionVars;
        private System.Windows.Forms.Panel panelVariables;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancelForm;
    }
}