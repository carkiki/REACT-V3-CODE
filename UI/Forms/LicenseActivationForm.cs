using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using ReactCRM.Services;
using ReactCRM.Utils;

namespace ReactCRM.UI.Forms
{
    /// <summary>
    /// Form for license activation
    /// </summary>
    public class LicenseActivationForm : Form
    {
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblLicenseKey;
        private TextBox txtLicenseKey;
        private Label lblFirebaseUrl;
        private TextBox txtFirebaseUrl;
        private Label lblHardwareId;
        private TextBox txtHardwareId;
        private Button btnActivate;
        private Button btnCancel;
        private ProgressBar progressBar;
        private Label lblStatus;
        private Panel panelHeader;

        public bool LicenseActivated { get; private set; }

        public LicenseActivationForm()
        {
            InitializeComponent();
            LoadHardwareId();
        }

        private void InitializeComponent()
        {
            this.Text = "License Activation - REACT CRM";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Header Panel
            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.FromArgb(52, 73, 94)
            };

            lblTitle = new Label
            {
                Text = "🔐 License Activation",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 20),
                AutoSize = true
            };

            lblSubtitle = new Label
            {
                Text = "Enter your license key to activate REACT CRM",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(189, 195, 199),
                Location = new Point(20, 60),
                AutoSize = true
            };

            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(lblSubtitle);

            // License Key Label
            lblLicenseKey = new Label
            {
                Text = "License Key:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(30, 130),
                Size = new Size(200, 25)
            };

            // License Key TextBox
            txtLicenseKey = new TextBox
            {
                Location = new Point(30, 160),
                Size = new Size(520, 30),
                Font = new Font("Segoe UI", 10),
                PlaceholderText = "Enter your license key"
            };

            // Firebase URL Label
            lblFirebaseUrl = new Label
            {
                Text = "Firebase URL (Optional):",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(30, 210),
                Size = new Size(200, 25)
            };

            // Firebase URL TextBox
            txtFirebaseUrl = new TextBox
            {
                Location = new Point(30, 240),
                Size = new Size(520, 30),
                Font = new Font("Segoe UI", 10),
                PlaceholderText = "https://your-project.firebaseio.com/"
            };

            // Hardware ID Label
            lblHardwareId = new Label
            {
                Text = "Hardware ID:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(30, 290),
                Size = new Size(200, 25)
            };

            // Hardware ID TextBox (Read-only)
            txtHardwareId = new TextBox
            {
                Location = new Point(30, 320),
                Size = new Size(520, 30),
                Font = new Font("Segoe UI", 10),
                ReadOnly = true,
                BackColor = Color.FromArgb(236, 240, 241)
            };

            // Progress Bar
            progressBar = new ProgressBar
            {
                Location = new Point(30, 370),
                Size = new Size(520, 10),
                Style = ProgressBarStyle.Marquee,
                Visible = false
            };

            // Status Label
            lblStatus = new Label
            {
                Location = new Point(30, 385),
                Size = new Size(520, 25),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(127, 140, 141),
                Text = ""
            };

            // Activate Button
            btnActivate = new Button
            {
                Text = "Activate License",
                Location = new Point(350, 420),
                Size = new Size(130, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnActivate.FlatAppearance.BorderSize = 0;
            btnActivate.Click += BtnActivate_Click;

            // Cancel Button
            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(490, 420),
                Size = new Size(80, 40),
                BackColor = Color.FromArgb(127, 140, 141),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            // Add controls to form
            this.Controls.Add(panelHeader);
            this.Controls.Add(lblLicenseKey);
            this.Controls.Add(txtLicenseKey);
            this.Controls.Add(lblFirebaseUrl);
            this.Controls.Add(txtFirebaseUrl);
            this.Controls.Add(lblHardwareId);
            this.Controls.Add(txtHardwareId);
            this.Controls.Add(progressBar);
            this.Controls.Add(lblStatus);
            this.Controls.Add(btnActivate);
            this.Controls.Add(btnCancel);
        }

        private void LoadHardwareId()
        {
            try
            {
                string hardwareId = HardwareInfo.GetHardwareId();
                txtHardwareId.Text = hardwareId;
            }
            catch (Exception ex)
            {
                txtHardwareId.Text = "Error retrieving hardware ID";
                lblStatus.Text = $"Warning: {ex.Message}";
                lblStatus.ForeColor = Color.Orange;
            }
        }

        private async void BtnActivate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLicenseKey.Text))
            {
                MessageBox.Show("Please enter a license key.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Disable controls
            btnActivate.Enabled = false;
            btnCancel.Enabled = false;
            txtLicenseKey.Enabled = false;
            txtFirebaseUrl.Enabled = false;
            progressBar.Visible = true;
            lblStatus.Text = "Activating license...";
            lblStatus.ForeColor = Color.FromArgb(52, 152, 219);

            try
            {
                string licenseKey = txtLicenseKey.Text.Trim();
                string firebaseUrl = string.IsNullOrWhiteSpace(txtFirebaseUrl.Text)
                    ? null
                    : txtFirebaseUrl.Text.Trim();

                var result = await LicenseValidationService.Instance.ActivateLicenseAsync(licenseKey, firebaseUrl);

                if (result.Success)
                {
                    lblStatus.Text = result.Message;
                    lblStatus.ForeColor = Color.Green;

                    MessageBox.Show(
                        $"{result.Message}\n\n" +
                        $"Company: {result.License.CompanyName}\n" +
                        $"Valid until: {result.License.ExpirationDate:yyyy-MM-dd}",
                        "Activation Successful",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    LicenseActivated = true;
                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    lblStatus.Text = result.Message;
                    lblStatus.ForeColor = Color.Red;

                    MessageBox.Show(result.Message, "Activation Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);

                    // Re-enable controls
                    btnActivate.Enabled = true;
                    btnCancel.Enabled = true;
                    txtLicenseKey.Enabled = true;
                    txtFirebaseUrl.Enabled = true;
                    progressBar.Visible = false;
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error: {ex.Message}";
                lblStatus.ForeColor = Color.Red;

                MessageBox.Show($"An error occurred during activation:\n{ex.Message}",
                    "Activation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Re-enable controls
                btnActivate.Enabled = true;
                btnCancel.Enabled = true;
                txtLicenseKey.Enabled = true;
                txtFirebaseUrl.Enabled = true;
                progressBar.Visible = false;
            }
        }
    }
}