using System;
using System.Drawing;
using System.Windows.Forms;

namespace PPDF.TotalAgility.Connector
{
    /// <summary>
    /// Displays the outcome of a "Send to TotalAgility" operation.
    ///
    /// Success — green header, tick icon, 4 detail rows:
    ///   Job ID, Process Name, Document Name, Created At
    ///
    /// Error — red header, warning icon, scrollable error text box
    ///   so the user can read and copy the message for support.
    ///
    /// Both states flow through this single form — no plain MessageBox.
    /// </summary>
    public partial class TAResultForm : Form
    {
        public TAResultForm(TAJobResult result)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;

            if (result == null)
            {
                ShowError("An unknown error occurred. No result was returned.");
                return;
            }

            if (result.Success)
                ShowSuccess(result);
            else
                ShowError(result.ErrorMessage ?? "An unknown error occurred.", result.ProcessName);
        }

        // ── Success ───────────────────────────────────────────────────────

        private void ShowSuccess(TAJobResult result)
        {
            panelHeader.BackColor = Color.FromArgb(0, 130, 80);
            lblHeader.Text = "Job Created Successfully";
            lblHeader.ForeColor = Color.White;
            pictureBoxStatus.Image = Resources.Resources.Success_icon;

            lblJobIdValue.Text = result.JobId ?? "-";
            lblProcessValue.Text = result.ProcessName ?? "-";
            lblDocumentValue.Text = result.DocumentName ?? "-";
            lblCreatedAtValue.Text = result.CreatedAt == default(DateTime)
                                     ? "-"
                                     : result.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");

            panelDetail.Visible = true;
            panelError.Visible = false;
            ClientSize = new System.Drawing.Size(560, 295);
        }

        // ── Error ─────────────────────────────────────────────────────────

        private void ShowError(string message, string processLabel = null)
        {
            panelHeader.BackColor = Color.FromArgb(180, 30, 30);
            lblHeader.Text = "Submission Failed";
            lblHeader.ForeColor = Color.White;
            pictureBoxStatus.Image = Resources.Resources.Alert_triangle;

            // Name the process and environment, so it is clear where the PDF was meant to go
            txtErrorMessage.Text = string.IsNullOrEmpty(processLabel)
                ? message
                : "Process: " + processLabel + "\r\n\r\n" + message;

            panelDetail.Visible = false;
            panelError.Visible = true;
            ClientSize = new System.Drawing.Size(560, 325);
        }

        // ── Events ────────────────────────────────────────────────────────

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void TAResultForm_Load(object sender, EventArgs e) { }
    }
}