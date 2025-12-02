using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ReactCRM.Database;
using ReactCRM.Models;
using ReactCRM.Services;

namespace ReactCRM.UI.Workers
{
    public partial class WorkerManagementForm : Form
    {
        private DataGridView gridWorkers;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnChangePassword;
        private Button btnDeactivate;
        private Button btnActivate;
        private Button btnDelete;
        private Button btnClose;
        private Label lblTotal;
        private WorkerRepository repository;

        public WorkerManagementForm()
        {
            repository = new WorkerRepository();
            InitializeComponent();
            LoadWorkers();
        }

        private void InitializeComponent()
        {
            this.Text = "Worker Management";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(800, 500);

            // Header Panel
            var panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(52, 73, 94),
                Padding = new Padding(15)
            };

            var lblTitle = new Label
            {
                Text = "Worker Management",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 15),
                AutoSize = true
            };

            var lblSubtitle = new Label
            {
                Text = "Manage system users and their permissions",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(189, 195, 199),
                Location = new Point(15, 45),
                AutoSize = true
            };

            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(lblSubtitle);

            // Top Panel
            var panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(236, 240, 241),
                Padding = new Padding(10)
            };

            btnAdd = new Button
            {
                Text = "Add Worker",
                Location = new Point(10, 12),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnAdd.Click += BtnAdd_Click;

            lblTotal = new Label
            {
                Text = "Total: 0",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(150, 20)
            };

            panelTop.Controls.Add(btnAdd);
            panelTop.Controls.Add(lblTotal);

            // DataGridView
            gridWorkers = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9)
            };
            gridWorkers.CellFormatting += GridWorkers_CellFormatting;

            // Bottom Panel
            var panelBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(236, 240, 241),
                Padding = new Padding(10)
            };

            btnEdit = new Button
            {
                Text = "Edit",
                Location = new Point(10, 12),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(241, 196, 15),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnEdit.Click += BtnEdit_Click;

            btnChangePassword = new Button
            {
                Text = "Change Password",
                Location = new Point(120, 12),
                Size = new Size(140, 35),
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnChangePassword.Click += BtnChangePassword_Click;

            btnDeactivate = new Button
            {
                Text = "Deactivate",
                Location = new Point(270, 12),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnDeactivate.Click += BtnDeactivate_Click;

            btnActivate = new Button
            {
                Text = "Activate",
                Location = new Point(380, 12),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnActivate.Click += BtnActivate_Click;

            btnDelete = new Button

            {

                Text = "Delete",

                Location = new Point(490, 12),

                Size = new Size(100, 35),

                BackColor = Color.FromArgb(192, 57, 43),

                ForeColor = Color.White,

                FlatStyle = FlatStyle.Flat,

                Font = new Font("Segoe UI", 9),

                Cursor = Cursors.Hand

            };

            btnDelete.Click += BtnDelete_Click;



            btnClose = new Button

            {

                Text = "Close",

                Location = new Point(600, 12),

                Size = new Size(100, 35),

                BackColor = Color.FromArgb(127, 140, 141),

                ForeColor = Color.White,

                FlatStyle = FlatStyle.Flat,

                Font = new Font("Segoe UI", 9),

                Cursor = Cursors.Hand

            };

            btnClose.Click += (s, e) => this.Close();



            panelBottom.Controls.Add(btnEdit);

            panelBottom.Controls.Add(btnChangePassword);

            panelBottom.Controls.Add(btnDeactivate);

            panelBottom.Controls.Add(btnActivate);

            panelBottom.Controls.Add(btnDelete);
            panelBottom.Controls.Add(btnClose);

            // Add controls to form
            this.Controls.Add(gridWorkers);
            this.Controls.Add(panelTop);
            this.Controls.Add(panelHeader);
            this.Controls.Add(panelBottom);

            // Check permissions
            if (!AuthService.Instance.CanManageWorkers())
            {
                btnAdd.Enabled = false;
                btnEdit.Enabled = false;
                btnChangePassword.Enabled = false;
                btnDeactivate.Enabled = false;
                btnActivate.Enabled = false;
                btnDelete.Enabled = false;
            }
        }

        private void LoadWorkers()
        {
            try
            {
                var workers = repository.GetAllWorkers();
                gridWorkers.DataSource = null;
                gridWorkers.Columns.Clear();

                // Manually add columns
                gridWorkers.Columns.Add("Id", "ID");
                gridWorkers.Columns.Add("Username", "Username");
                gridWorkers.Columns.Add("Role", "Role");
                gridWorkers.Columns.Add("IsActive", "Status");
                gridWorkers.Columns.Add("LastLogin", "Last Login");
                gridWorkers.Columns.Add("CreatedAt", "Created At");

                // Add rows
                foreach (var worker in workers)
                {
                    gridWorkers.Rows.Add(
                        worker.Id,
                        worker.Username,
                        worker.Role.ToString(),
                        worker.IsActive ? "Active" : "Inactive",
                        worker.LastLogin?.ToString("yyyy-MM-dd HH:mm") ?? "Never",
                        worker.CreatedAt.ToString("yyyy-MM-dd HH:mm")
                    );
                }

                // Format columns
                gridWorkers.Columns["Id"].Width = 50;
                gridWorkers.Columns["Username"].Width = 200;
                gridWorkers.Columns["Role"].Width = 150;
                gridWorkers.Columns["IsActive"].Width = 100;
                gridWorkers.Columns["LastLogin"].Width = 150;
                gridWorkers.Columns["CreatedAt"].Width = 150;

                lblTotal.Text = $"Total: {workers.Count} ({workers.Count(w => w.IsActive)} active)";

                AuditService.LogAction("View", "WorkerList", null,
                    $"Viewed worker list ({workers.Count} workers)");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading workers: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void BtnDelete_Click(object sender, EventArgs e)

        {

            if (!AuthService.Instance.CanManageWorkers())

            {

                MessageBox.Show("You don't have permission to delete workers.", "Access Denied",

                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }



            if (gridWorkers.SelectedRows.Count == 0)

            {

                MessageBox.Show("Please select a worker to delete.", "No Selection",

                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                return;

            }



            int workerId = (int)gridWorkers.SelectedRows[0].Cells["Id"].Value;

            string username = gridWorkers.SelectedRows[0].Cells["Username"].Value.ToString();



            // Prevent deleting self

            if (username == AuthService.Instance.GetCurrentUsername())

            {

                MessageBox.Show("You cannot delete your own account.", "Cannot Delete",

                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }



            var result = MessageBox.Show(

                $"Are you sure you want to PERMANENTLY DELETE user '{username}'?\n\n" +

                "This action cannot be undone. All audit logs associated with this user will remain, " +

                "but the user account will be permanently removed from the database.\n\n" +

                "Consider deactivating the user instead if you want to preserve the account.",

                "Confirm Permanent Deletion",

                MessageBoxButtons.YesNo,

                MessageBoxIcon.Warning);



            if (result == DialogResult.Yes)

            {

                // Double confirmation for permanent deletion

                var confirmResult = MessageBox.Show(

                    $"Final confirmation: Delete user '{username}' permanently?",

                    "Final Confirmation",

                    MessageBoxButtons.YesNo,

                    MessageBoxIcon.Exclamation);



                if (confirmResult == DialogResult.Yes)

                {

                    try

                    {

                        int userId = AuthService.Instance.GetCurrentUserId();

                        if (repository.DeleteWorker(workerId, userId))

                        {

                            MessageBox.Show("Worker deleted permanently.", "Success",

                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                            LoadWorkers();

                        }

                    }

                    catch (Exception ex)

                    {

                        MessageBox.Show($"Error deleting worker: {ex.Message}", "Error",

                            MessageBoxButtons.OK, MessageBoxIcon.Error);

                    }

                }

            }

        }

        private void GridWorkers_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (gridWorkers.Columns[e.ColumnIndex].Name == "IsActive" && e.Value != null)
            {
                if (e.Value.ToString() == "Active")
                {
                    e.CellStyle.ForeColor = Color.FromArgb(39, 174, 96);
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }
                else
                {
                    e.CellStyle.ForeColor = Color.FromArgb(231, 76, 60);
                }
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!AuthService.Instance.CanManageWorkers())
            {
                MessageBox.Show("You don't have permission to add workers.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var form = new WorkerEditForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadWorkers();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (!AuthService.Instance.CanManageWorkers())
            {
                MessageBox.Show("You don't have permission to edit workers.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (gridWorkers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a worker to edit.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int workerId = (int)gridWorkers.SelectedRows[0].Cells["Id"].Value;
            var worker = repository.GetWorkerById(workerId);

            if (worker != null)
            {
                var form = new WorkerEditForm(worker);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadWorkers();
                }
            }
        }

        private void BtnChangePassword_Click(object sender, EventArgs e)
        {
            if (!AuthService.Instance.CanManageWorkers())
            {
                MessageBox.Show("You don't have permission to change passwords.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (gridWorkers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a worker.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int workerId = (int)gridWorkers.SelectedRows[0].Cells["Id"].Value;
            string username = gridWorkers.SelectedRows[0].Cells["Username"].Value.ToString();

            var form = new ChangePasswordForm(workerId, username);
            if (form.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("Password changed successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnDeactivate_Click(object sender, EventArgs e)
        {
            if (!AuthService.Instance.CanManageWorkers())
            {
                MessageBox.Show("You don't have permission to deactivate workers.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (gridWorkers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a worker.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int workerId = (int)gridWorkers.SelectedRows[0].Cells["Id"].Value;
            string username = gridWorkers.SelectedRows[0].Cells["Username"].Value.ToString();

            // Prevent deactivating self
            if (username == AuthService.Instance.GetCurrentUsername())
            {
                MessageBox.Show("You cannot deactivate your own account.", "Cannot Deactivate",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to deactivate user '{username}'?\n\nThey will no longer be able to log in.",
                "Confirm Deactivate",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    int userId = AuthService.Instance.GetCurrentUserId();
                    if (repository.DeactivateWorker(workerId, userId))
                    {
                        MessageBox.Show("Worker deactivated successfully.", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadWorkers();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deactivating worker: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnActivate_Click(object sender, EventArgs e)
        {
            if (!AuthService.Instance.CanManageWorkers())
            {
                MessageBox.Show("You don't have permission to activate workers.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (gridWorkers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a worker.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int workerId = (int)gridWorkers.SelectedRows[0].Cells["Id"].Value;
            string status = gridWorkers.SelectedRows[0].Cells["IsActive"].Value.ToString();

            if (status == "Active")
            {
                MessageBox.Show("This worker is already active.", "Already Active",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var worker = repository.GetWorkerById(workerId);
                worker.IsActive = true;
                int userId = AuthService.Instance.GetCurrentUserId();
                if (repository.UpdateWorker(worker, userId))
                {
                    MessageBox.Show("Worker activated successfully.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadWorkers();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error activating worker: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
