using System;
using System.Drawing;
using System.Windows.Forms;
using ReactCRM.Database;
using ReactCRM.Models;
using ReactCRM.Services;

namespace ReactCRM.UI.Workers
{
    public partial class WorkerEditForm : Form
    {
        private Worker worker;
        private bool isNewWorker;
        private WorkerRepository repository;

        private TextBox txtUsername;
        private ComboBox cboRole;
        private TextBox txtPassword;
        private TextBox txtConfirmPassword;
        private CheckBox chkIsActive;
        private Button btnSave;
        private Button btnCancel;
        private Label lblPasswordNote;

        public WorkerEditForm(Worker existingWorker = null)
        {
            repository = new WorkerRepository();
            worker = existingWorker ?? new Worker { IsActive = true };
            isNewWorker = existingWorker == null;

            InitializeComponent();
            LoadWorkerData();
        }

        private void InitializeComponent()
        {
            this.Text = isNewWorker ? "Add New Worker" : "Edit Worker";
            this.Size = new Size(450, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int yPos = 20;
            int labelWidth = 120;
            int fieldX = 140;
            int fieldWidth = 250;
            int spacing = 50;

            // Username
            var lblUsername = new Label
            {
                Text = "Username:",
                Location = new Point(20, yPos + 3),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10)
            };

            txtUsername = new TextBox
            {
                Location = new Point(fieldX, yPos),
                Width = fieldWidth,
                Font = new Font("Segoe UI", 10)
            };
            yPos += spacing;

            // Role
            var lblRole = new Label
            {
                Text = "Role:",
                Location = new Point(20, yPos + 3),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10)
            };

            cboRole = new ComboBox
            {
                Location = new Point(fieldX, yPos),
                Width = fieldWidth,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cboRole.Items.AddRange(new object[] { "Admin", "Employee", "ViewOnly" });
            cboRole.SelectedIndex = 1; // Default to Employee
            yPos += spacing;

            // Password (only for new workers)
            if (isNewWorker)
            {
                var lblPassword = new Label
                {
                    Text = "Password:",
                    Location = new Point(20, yPos + 3),
                    Width = labelWidth,
                    Font = new Font("Segoe UI", 10)
                };

                txtPassword = new TextBox
                {
                    Location = new Point(fieldX, yPos),
                    Width = fieldWidth,
                    PasswordChar = '●',
                    Font = new Font("Segoe UI", 10)
                };
                yPos += spacing;

                var lblConfirmPassword = new Label
                {
                    Text = "Confirm Password:",
                    Location = new Point(20, yPos + 3),
                    Width = labelWidth,
                    Font = new Font("Segoe UI", 10)
                };

                txtConfirmPassword = new TextBox
                {
                    Location = new Point(fieldX, yPos),
                    Width = fieldWidth,
                    PasswordChar = '●',
                    Font = new Font("Segoe UI", 10)
                };
                yPos += spacing;

                this.Controls.Add(lblPassword);
                this.Controls.Add(txtPassword);
                this.Controls.Add(lblConfirmPassword);
                this.Controls.Add(txtConfirmPassword);
            }
            else
            {
                lblPasswordNote = new Label
                {
                    Text = "Use 'Change Password' to update password",
                    Location = new Point(fieldX, yPos),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9, FontStyle.Italic),
                    ForeColor = Color.Gray
                };
                yPos += 30;

                this.Controls.Add(lblPasswordNote);
            }

            // IsActive
            var lblIsActive = new Label
            {
                Text = "Status:",
                Location = new Point(20, yPos + 3),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10)
            };

            chkIsActive = new CheckBox
            {
                Text = "Active",
                Location = new Point(fieldX, yPos),
                AutoSize = true,
                Checked = true,
                Font = new Font("Segoe UI", 10)
            };
            yPos += spacing;

            // Buttons
            btnSave = new Button
            {
                Text = "Save",
                Location = new Point(fieldX, yPos + 10),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(fieldX + 110, yPos + 10),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(127, 140, 141),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnCancel.Click += (s, e) => this.Close();

            // Add controls
            this.Controls.Add(lblUsername);
            this.Controls.Add(txtUsername);
            this.Controls.Add(lblRole);
            this.Controls.Add(cboRole);
            this.Controls.Add(lblIsActive);
            this.Controls.Add(chkIsActive);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
        }

        private void LoadWorkerData()
        {
            if (!isNewWorker)
            {
                txtUsername.Text = worker.Username;
                cboRole.SelectedItem = worker.Role.ToString();
                chkIsActive.Checked = worker.IsActive;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Username is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (cboRole.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a role.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboRole.Focus();
                return;
            }

            if (isNewWorker)
            {
                if (string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show("Password is required.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return;
                }

                if (txtPassword.Text.Length < 6)
                {
                    MessageBox.Show("Password must be at least 6 characters.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return;
                }

                if (txtPassword.Text != txtConfirmPassword.Text)
                {
                    MessageBox.Show("Passwords do not match.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtConfirmPassword.Focus();
                    return;
                }

                // Check for duplicate username
                var existing = repository.GetWorkerByUsername(txtUsername.Text.Trim());
                if (existing != null)
                {
                    MessageBox.Show("A worker with this username already exists.", "Duplicate Username",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            try
            {
                // Update worker object
                worker.Username = txtUsername.Text.Trim();
                worker.Role = Enum.Parse<UserRole>(cboRole.SelectedItem.ToString());
                worker.IsActive = chkIsActive.Checked;

                int userId = AuthService.Instance.GetCurrentUserId();

                if (isNewWorker)
                {
                    int newId = repository.CreateWorker(worker, txtPassword.Text, userId);
                    MessageBox.Show($"Worker created successfully (ID: {newId})", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    repository.UpdateWorker(worker, userId);
                    MessageBox.Show("Worker updated successfully.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving worker: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public partial class ChangePasswordForm : Form
    {
        private int workerId;
        private string username;
        private WorkerRepository repository;

        private TextBox txtNewPassword;
        private TextBox txtConfirmPassword;
        private Button btnSave;
        private Button btnCancel;

        public ChangePasswordForm(int workerId, string username)
        {
            this.workerId = workerId;
            this.username = username;
            repository = new WorkerRepository();

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = $"Change Password - {username}";
            this.Size = new Size(400, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int yPos = 20;
            int labelWidth = 130;
            int fieldX = 150;
            int fieldWidth = 200;
            int spacing = 50;

            // Info label
            var lblInfo = new Label
            {
                Text = $"Set new password for: {username}",
                Location = new Point(20, yPos),
                Width = 350,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            yPos += 40;

            // New Password
            var lblNewPassword = new Label
            {
                Text = "New Password:",
                Location = new Point(20, yPos + 3),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10)
            };

            txtNewPassword = new TextBox
            {
                Location = new Point(fieldX, yPos),
                Width = fieldWidth,
                PasswordChar = '●',
                Font = new Font("Segoe UI", 10)
            };
            yPos += spacing;

            // Confirm Password
            var lblConfirmPassword = new Label
            {
                Text = "Confirm Password:",
                Location = new Point(20, yPos + 3),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10)
            };

            txtConfirmPassword = new TextBox
            {
                Location = new Point(fieldX, yPos),
                Width = fieldWidth,
                PasswordChar = '●',
                Font = new Font("Segoe UI", 10)
            };
            yPos += spacing + 10;

            // Buttons
            btnSave = new Button
            {
                Text = "Change Password",
                Location = new Point(fieldX, yPos),
                Size = new Size(140, 35),
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(fieldX + 150, yPos),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(127, 140, 141),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnCancel.Click += (s, e) => this.Close();

            // Add controls
            this.Controls.Add(lblInfo);
            this.Controls.Add(lblNewPassword);
            this.Controls.Add(txtNewPassword);
            this.Controls.Add(lblConfirmPassword);
            this.Controls.Add(txtConfirmPassword);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNewPassword.Text))
            {
                MessageBox.Show("Password is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNewPassword.Focus();
                return;
            }

            if (txtNewPassword.Text.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNewPassword.Focus();
                return;
            }

            if (txtNewPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("Passwords do not match.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtConfirmPassword.Focus();
                return;
            }

            try
            {
                int userId = AuthService.Instance.GetCurrentUserId();
                repository.ChangePassword(workerId, txtNewPassword.Text, userId);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error changing password: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
