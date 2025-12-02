using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using ReactCRM.Models;
using ReactCRM.Services;

namespace ReactCRM.UI.Logs
{
    public partial class LogViewerForm : Form
    {
        private DataGridView gridLogs;
        private DateTimePicker dtpStartDate;
        private DateTimePicker dtpEndDate;
        private ComboBox cboActionType;
        private ComboBox cboUser;
        private TextBox txtSearch;
        private Button btnFilter;
        private Button btnClear;
        private Button btnExport;
        private Button btnRefresh;
        private Button btnClose;
        private Label lblTotal;
        private CheckBox chkToday;
        private CheckBox chkThisWeek;
        private CheckBox chkThisMonth;

        private List<AuditLog> allLogs;

        public LogViewerForm()
        {
            InitializeComponent();
            LoadLogs();
        }

        private void InitializeComponent()
        {
            this.Text = "Audit Log Viewer";
            this.Size = new Size(1400, 800);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(1000, 600);

            // Header Panel
            var panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(52, 73, 94),
                Padding = new Padding(15)
            };

            var lblTitle = new Label
            {
                Text = "Audit Log Viewer",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 10),
                AutoSize = true
            };

            var lblSubtitle = new Label
            {
                Text = "View and filter system activity logs",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(189, 195, 199),
                Location = new Point(15, 40),
                AutoSize = true
            };

            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(lblSubtitle);

            // Filter Panel
            var panelFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 130,
                BackColor = Color.FromArgb(236, 240, 241),
                Padding = new Padding(10)
            };

            int yPos = 10;
            int labelX = 10;
            int controlX = 100;

            // Date Range Row 1
            var lblDateRange = new Label
            {
                Text = "Date Range:",
                Location = new Point(labelX, yPos + 3),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };

            dtpStartDate = new DateTimePicker
            {
                Location = new Point(controlX, yPos),
                Width = 150,
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 9)
            };
            dtpStartDate.Value = DateTime.Today.AddDays(-7);

            var lblTo = new Label
            {
                Text = "to",
                Location = new Point(controlX + 160, yPos + 3),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };

            dtpEndDate = new DateTimePicker
            {
                Location = new Point(controlX + 185, yPos),
                Width = 150,
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 9)
            };
            dtpEndDate.Value = DateTime.Today.AddDays(1);

            // Quick Date Filters
            chkToday = new CheckBox
            {
                Text = "Today",
                Location = new Point(controlX + 350, yPos + 2),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };
            chkToday.CheckedChanged += (s, e) => SetDateRange(0);

            chkThisWeek = new CheckBox
            {
                Text = "This Week",
                Location = new Point(controlX + 430, yPos + 2),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };
            chkThisWeek.CheckedChanged += (s, e) => SetDateRange(7);

            chkThisMonth = new CheckBox
            {
                Text = "This Month",
                Location = new Point(controlX + 540, yPos + 2),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };
            chkThisMonth.CheckedChanged += (s, e) => SetDateRange(30);

            yPos += 35;

            // Action Type and User Row 2
            var lblActionType = new Label
            {
                Text = "Action Type:",
                Location = new Point(labelX, yPos + 3),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };

            cboActionType = new ComboBox
            {
                Location = new Point(controlX, yPos),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cboActionType.Items.Add("(All)");
            cboActionType.SelectedIndex = 0;

            var lblUser = new Label
            {
                Text = "User:",
                Location = new Point(controlX + 220, yPos + 3),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };

            cboUser = new ComboBox
            {
                Location = new Point(controlX + 260, yPos),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cboUser.Items.Add("(All)");
            cboUser.SelectedIndex = 0;

            yPos += 35;

            // Search Row 3
            var lblSearch = new Label
            {
                Text = "Search:",
                Location = new Point(labelX, yPos + 3),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };

            txtSearch = new TextBox
            {
                Location = new Point(controlX, yPos),
                Width = 400,
                Font = new Font("Segoe UI", 9)
            };

            btnFilter = new Button
            {
                Text = "Filter",
                Location = new Point(controlX + 410, yPos - 2),
                Size = new Size(80, 28),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnFilter.Click += BtnFilter_Click;

            btnClear = new Button
            {
                Text = "Clear",
                Location = new Point(controlX + 500, yPos - 2),
                Size = new Size(80, 28),
                BackColor = Color.FromArgb(127, 140, 141),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnClear.Click += BtnClear_Click;

            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(controlX + 590, yPos - 2),
                Size = new Size(80, 28),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnRefresh.Click += (s, e) => LoadLogs();

            panelFilter.Controls.Add(lblDateRange);
            panelFilter.Controls.Add(dtpStartDate);
            panelFilter.Controls.Add(lblTo);
            panelFilter.Controls.Add(dtpEndDate);
            panelFilter.Controls.Add(chkToday);
            panelFilter.Controls.Add(chkThisWeek);
            panelFilter.Controls.Add(chkThisMonth);
            panelFilter.Controls.Add(lblActionType);
            panelFilter.Controls.Add(cboActionType);
            panelFilter.Controls.Add(lblUser);
            panelFilter.Controls.Add(cboUser);
            panelFilter.Controls.Add(lblSearch);
            panelFilter.Controls.Add(txtSearch);
            panelFilter.Controls.Add(btnFilter);
            panelFilter.Controls.Add(btnClear);
            panelFilter.Controls.Add(btnRefresh);

            // DataGridView
            gridLogs = new DataGridView
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

            // Bottom Panel
            var panelBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(236, 240, 241),
                Padding = new Padding(10)
            };

            lblTotal = new Label
            {
                Text = "Total: 0",
                Location = new Point(10, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            btnExport = new Button
            {
                Text = "Export to CSV",
                Location = new Point(1000, 12),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(241, 196, 15),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnExport.Click += BtnExport_Click;

            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(1130, 12),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(127, 140, 141),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnClose.Click += (s, e) => this.Close();

            panelBottom.Controls.Add(lblTotal);
            panelBottom.Controls.Add(btnExport);
            panelBottom.Controls.Add(btnClose);

            // Add controls to form
            this.Controls.Add(gridLogs);
            this.Controls.Add(panelFilter);
            this.Controls.Add(panelHeader);
            this.Controls.Add(panelBottom);
        }

        private void SetDateRange(int days)
        {
            // Uncheck other checkboxes
            if (days == 0)
            {
                chkToday.Checked = true;
                chkThisWeek.Checked = false;
                chkThisMonth.Checked = false;
                dtpStartDate.Value = DateTime.Today;
                dtpEndDate.Value = DateTime.Today.AddDays(1);
            }
            else if (days == 7)
            {
                chkToday.Checked = false;
                chkThisWeek.Checked = true;
                chkThisMonth.Checked = false;
                dtpStartDate.Value = DateTime.Today.AddDays(-7);
                dtpEndDate.Value = DateTime.Today.AddDays(1);
            }
            else if (days == 30)
            {
                chkToday.Checked = false;
                chkThisWeek.Checked = false;
                chkThisMonth.Checked = true;
                dtpStartDate.Value = DateTime.Today.AddDays(-30);
                dtpEndDate.Value = DateTime.Today.AddDays(1);
            }
            ApplyFilters();
        }

        private void LoadLogs()
        {
            try
            {
                allLogs = AuditService.GetAuditLogs(dtpStartDate.Value, dtpEndDate.Value);

                // Populate action type filter
                var actionTypes = allLogs.Select(l => l.ActionType).Distinct().OrderBy(a => a).ToList();
                cboActionType.Items.Clear();
                cboActionType.Items.Add("(All)");
                cboActionType.Items.AddRange(actionTypes.ToArray());
                cboActionType.SelectedIndex = 0;

                // Populate user filter
                var users = allLogs.Select(l => l.User).Distinct().OrderBy(u => u).ToList();
                cboUser.Items.Clear();
                cboUser.Items.Add("(All)");
                cboUser.Items.AddRange(users.ToArray());
                cboUser.SelectedIndex = 0;

                DisplayLogs(allLogs);

                AuditService.LogAction("View", "AuditLogs", null,
                    $"Viewed audit logs ({allLogs.Count} entries)");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading logs: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayLogs(List<AuditLog> logs)
        {
            gridLogs.DataSource = null;
            gridLogs.Columns.Clear();

            var displayData = logs.Select(l => new
            {
                l.Id,
                Timestamp = l.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                l.User,
                l.ActionType,
                l.Entity,
                l.EntityId,
                l.Description
            }).ToList();

            gridLogs.DataSource = displayData;

            // Format columns - with null checks and error handling
            try
            {
                if (gridLogs?.Columns != null)
                {
                    if (gridLogs.Columns.Contains("Id"))
                    {
                        try { gridLogs.Columns["Id"].Width = 50; } catch { }
                    }

                    if (gridLogs.Columns.Contains("Timestamp"))
                    {
                        try { gridLogs.Columns["Timestamp"].Width = 150; } catch { }
                    }

                    if (gridLogs.Columns.Contains("User"))
                    {
                        try { gridLogs.Columns["User"].Width = 120; } catch { }
                    }

                    if (gridLogs.Columns.Contains("ActionType"))
                    {
                        try
                        {
                            gridLogs.Columns["ActionType"].HeaderText = "Action";
                            gridLogs.Columns["ActionType"].Width = 120;
                        }
                        catch { }
                    }

                    if (gridLogs.Columns.Contains("Entity"))
                    {
                        try { gridLogs.Columns["Entity"].Width = 100; } catch { }
                    }

                    if (gridLogs.Columns.Contains("EntityId"))
                    {
                        try
                        {
                            gridLogs.Columns["EntityId"].HeaderText = "Entity ID";
                            gridLogs.Columns["EntityId"].Width = 80;
                        }
                        catch { }
                    }

                    if (gridLogs.Columns.Contains("Description"))
                    {
                        try
                        {
                            gridLogs.Columns["Description"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Column formatting error: {ex.Message}");
            }

            lblTotal.Text = $"Total: {logs.Count} log entries";
        }

        private void BtnFilter_Click(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (allLogs == null)
            {
                LoadLogs();
                return;
            }

            var filtered = allLogs.AsEnumerable();

            // Filter by action type
            if (cboActionType.SelectedIndex > 0)
            {
                string actionType = cboActionType.SelectedItem.ToString();
                filtered = filtered.Where(l => l.ActionType == actionType);
            }

            // Filter by user
            if (cboUser.SelectedIndex > 0)
            {
                string user = cboUser.SelectedItem.ToString();
                filtered = filtered.Where(l => l.User == user);
            }

            // Filter by search text
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                string search = txtSearch.Text.ToLower();
                filtered = filtered.Where(l =>
                    l.Description.ToLower().Contains(search) ||
                    l.ActionType.ToLower().Contains(search) ||
                    l.Entity?.ToLower().Contains(search) == true ||
                    l.User.ToLower().Contains(search));
            }

            DisplayLogs(filtered.ToList());
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            dtpStartDate.Value = DateTime.Today.AddDays(-7);
            dtpEndDate.Value = DateTime.Today.AddDays(1);
            cboActionType.SelectedIndex = 0;
            cboUser.SelectedIndex = 0;
            txtSearch.Clear();
            chkToday.Checked = false;
            chkThisWeek.Checked = false;
            chkThisMonth.Checked = false;

            LoadLogs();
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                using (var saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                    saveFileDialog.Title = "Export Audit Logs";
                    saveFileDialog.FileName = $"audit_logs_{DateTime.Now:yyyy-MM-dd_HHmmss}.csv";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        ExportToCsv(saveFileDialog.FileName);
                        MessageBox.Show($"Logs exported successfully to:\n{saveFileDialog.FileName}",
                            "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        AuditService.LogAction("Export", "AuditLogs", null,
                            $"Exported audit logs to {Path.GetFileName(saveFileDialog.FileName)}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting logs: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToCsv(string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                // Write header
                writer.WriteLine("Id,Timestamp,User,ActionType,Entity,EntityId,Description");

                // Get current displayed logs
                var displayedLogs = allLogs;
                if (cboActionType.SelectedIndex > 0 || cboUser.SelectedIndex > 0 || !string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    ApplyFilters();
                }

                // Write data
                foreach (var log in displayedLogs)
                {
                    string line = $"{log.Id}," +
                                 $"{log.Timestamp:yyyy-MM-dd HH:mm:ss}," +
                                 $"\"{EscapeCsv(log.User)}\"," +
                                 $"\"{EscapeCsv(log.ActionType)}\"," +
                                 $"\"{EscapeCsv(log.Entity)}\"," +
                                 $"{log.EntityId}," +
                                 $"\"{EscapeCsv(log.Description)}\"";

                    writer.WriteLine(line);
                }
            }
        }

        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            return value.Replace("\"", "\"\"");
        }
    }
}