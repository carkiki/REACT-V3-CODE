using System;
using System.Drawing;
using System.Windows.Forms;
using ReactCRM.Services;
using ReactCRM.UI.Dashboard;

namespace ReactCRM.UI.Login
{
    public partial class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblUsername;
        private Label lblPassword;
        private Label lblError;
        private Panel panelLeft;
        private Panel panelRight;
        private Label lblDefaultCredentials;
        private Label lblWelcome;

        public LoginForm()
        {
            InitializeComponent();
            // Apply application logo
            LogoService.ApplyLogoToForm(this);
        }

        private void InitializeComponent()
        {
            this.Text = "REACT CRM - Login";
            this.Size = new Size(1000, 600);

            this.StartPosition = FormStartPosition.CenterScreen;

            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            this.MaximizeBox = false;

            this.MinimizeBox = false;

            this.BackColor = UITheme.Colors.BackgroundPrimary;

            this.Font = UITheme.Fonts.BodyRegular;



            // ===== LEFT PANEL - Branding Section =====

            panelLeft = new Panel

            {

                Dock = DockStyle.Left,

                Width = 500,

                BackColor = UITheme.Colors.HeaderPrimary,

                Padding = new Padding(50)

            };



            // Logo/Title (Centered)

            lblTitle = new Label

            {

                Text = "REACT CRM",

                Font = new Font(UITheme.Fonts.PrimaryFont, 42, FontStyle.Bold),

                ForeColor = UITheme.Colors.TextInverse,

                AutoSize = true,

                Location = new Point(100, 180)

            };



            // Subtitle (Centered)

            lblSubtitle = new Label

            {

                Text = "Client Relationship\nManagement System",

                Font = new Font(UITheme.Fonts.PrimaryFont, 14, FontStyle.Regular),

                ForeColor = Color.FromArgb(220, 220, 220),

                AutoSize = true,

                Location = new Point(120, 250),

                TextAlign = ContentAlignment.MiddleCenter

            };



            // Decorative element

            Label lblVersion = new Label

            {

                Text = "v1.0",

                Font = UITheme.Fonts.BodySmall,

                ForeColor = Color.FromArgb(180, 180, 180),

                AutoSize = true,

                Location = new Point(220, 500)

            };



            panelLeft.Controls.Add(lblTitle);

            panelLeft.Controls.Add(lblSubtitle);

            panelLeft.Controls.Add(lblVersion);



            // ===== RIGHT PANEL - Login Form Section =====

            panelRight = new Panel

            {

                Dock = DockStyle.Fill,

                BackColor = UITheme.Colors.BackgroundPrimary,

                Padding = new Padding(60, 100, 60, 50)

            };



            // Welcome Label

            lblWelcome = new Label

            {

                Text = "Welcome Back",

                Font = new Font(UITheme.Fonts.PrimaryFont, 24, FontStyle.Bold),

                ForeColor = UITheme.Colors.TextPrimary,

                AutoSize = true,

                Location = new Point(80, 100)

            };



            // Username Label

            lblUsername = new Label

            {

                Text = "Username",

                Font = new Font(UITheme.Fonts.PrimaryFont, 10, FontStyle.Bold),

                ForeColor = UITheme.Colors.TextPrimary,

                Location = new Point(80, 170),

                AutoSize = true

            };



            // Username TextBox

            txtUsername = new TextBox

            {

                Location = new Point(80, 195),

                Size = new Size(320, 35),

                Font = new Font(UITheme.Fonts.PrimaryFont, 11),

                BorderStyle = BorderStyle.FixedSingle,

                Text = "admin"

            };



            // Password Label

            lblPassword = new Label

            {

                Text = "Password",

                Font = new Font(UITheme.Fonts.PrimaryFont, 10, FontStyle.Bold),

                ForeColor = UITheme.Colors.TextPrimary,

                Location = new Point(80, 250),

                AutoSize = true

            };



            // Password TextBox

            txtPassword = new TextBox

            {

                Location = new Point(80, 275),

                Size = new Size(320, 35),

                Font = new Font(UITheme.Fonts.PrimaryFont, 11),

                BorderStyle = BorderStyle.FixedSingle,

                PasswordChar = '●',

                Text = "admin123"

            };



            // Login Button

            btnLogin = new Button

            {

                Text = "Sign In",

                Location = new Point(80, 340),

                Size = new Size(320, 45),

                Font = new Font(UITheme.Fonts.PrimaryFont, 12, FontStyle.Bold),

                Cursor = Cursors.Hand

            };

            UITheme.ApplyButtonStyle(btnLogin, ButtonType.Primary);

            btnLogin.Click += BtnLogin_Click;



            // Error Label

            lblError = new Label

            {

                Text = "",

                Location = new Point(80, 395),

                Size = new Size(320, 30),

                ForeColor = UITheme.Colors.StatusError,

                TextAlign = ContentAlignment.MiddleCenter,

                Font = UITheme.Fonts.BodySmall,

                AutoSize = false

            };



            // Default Credentials Info

            lblDefaultCredentials = new Label

            {

                Text = "For any question send mail to: borjac240@gmail.com",

                Location = new Point(80, 450),

                Size = new Size(320, 50),

                ForeColor = UITheme.Colors.TextSecondary,

                Font = new Font(UITheme.Fonts.PrimaryFont, 8),

                TextAlign = ContentAlignment.MiddleCenter,

                AutoSize = false

            };



            panelRight.Controls.Add(lblWelcome);

            panelRight.Controls.Add(lblUsername);

            panelRight.Controls.Add(txtUsername);

            panelRight.Controls.Add(lblPassword);

            panelRight.Controls.Add(txtPassword);

            panelRight.Controls.Add(btnLogin);

            panelRight.Controls.Add(lblError);

            panelRight.Controls.Add(lblDefaultCredentials);



            // Add panels to form

            this.Controls.Add(panelRight);

            this.Controls.Add(panelLeft);

            // Allow Enter key to login
            this.AcceptButton = btnLogin;

            // Set default focus
            txtUsername.Focus();
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                ShowError("Please enter username");
                txtUsername.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                ShowError("Please enter password");
                txtPassword.Focus();
                return;
            }
            // Disable login button to prevent multiple clicks
            btnLogin.Enabled = false;
            // Create and show loading form
            LoadingForm loadingForm = new LoadingForm();
            loadingForm.Show(this);
            try
            {
                // Perform login operation on background thread
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Text;
                bool loginSuccess = await System.Threading.Tasks.Task.Run(() =>
                {
                    // Simulate minimum loading time for better UX

                    System.Threading.Thread.Sleep(500);

                    return AuthService.Instance.Login(username, password);
                });
                // Close loading form
                loadingForm.Close();
                if (loginSuccess)
                {
                    // Clear password after successful login
                    txtPassword.Clear();
                    // Hide login form
                    this.Hide();
                    // Show dashboard
                    var dashboard = new DashboardForm();

                    dashboard.FormClosed += (s, args) => this.Close();

                    dashboard.Show();
                }
                else
                {
                    ShowError("Invalid username or password");

                    txtPassword.Clear();

                    txtPassword.Focus();

                    btnLogin.Enabled = true;
                }
            }
            catch (Exception ex)
            {

                loadingForm.Close();

                ShowError($"Login error: {ex.Message}");

                btnLogin.Enabled = true;
            }
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            lblError.ForeColor = UITheme.Colors.StatusError;
        }
    }
}
