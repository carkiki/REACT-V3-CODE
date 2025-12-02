using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using ReactCRM.Database;
using ReactCRM.Models;
using ReactCRM.Services;

namespace ReactCRM.UI.Clients
{
    /// <summary>
    /// Form to view field change history for a client
    /// Shows all versions of each field with dropdown to select different versions
    /// </summary>
    public partial class FieldHistoryForm : Form
    {
        private int clientId;
        private Client currentClient;
        private DataGridView gridHistory;
        private ComboBox cboField;
        private Label lblFieldLabel;
        private Panel panelVersions;
        private Button btnClose;
        private Button btnRestoreVersion;
        private Label lblCurrentValue;
        private List<FieldHistoryEntry> allHistory;

        public FieldHistoryForm(int clientId, Client client)
        {
            this.clientId = clientId;
            this.currentClient = client;
            InitializeComponent();
            LoadFieldHistory();
        }

        private void InitializeComponent()
        {
            this.Text = $"Field History - {currentClient.Name}";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(700, 500);

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
                Text = $"Change History for: {currentClient.Name}",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 15),
                AutoSize = true
            };

            var lblSubtitle = new Label
            {
                Text = $"SSN: {currentClient.SSN} | View all field changes and restore previous versions",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(189, 195, 199),
                Location = new Point(15, 45),
                AutoSize = true
            };

            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(lblSubtitle);

            // Field Selection Panel
            var panelFieldSelect = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(236, 240, 241),
                Padding = new Padding(15)
            };

            lblFieldLabel = new Label
            {
                Text = "Select Field:",
                Location = new Point(15, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 10)
            };

            cboField = new ComboBox
            {
                Location = new Point(120, 17),
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cboField.SelectedIndexChanged += CboField_SelectedIndexChanged;

            lblCurrentValue = new Label
            {
                Text = "",
                Location = new Point(400, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(39, 174, 96)
            };

            panelFieldSelect.Controls.Add(lblFieldLabel);
            panelFieldSelect.Controls.Add(cboField);
            panelFieldSelect.Controls.Add(lblCurrentValue);

            // History Grid
            gridHistory = new DataGridView
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
                Padding = new Padding(15)
            };

            btnRestoreVersion = new Button
            {
                Text = "Restore Selected Version",
                Location = new Point(15, 12),
                Size = new Size(180, 35),
                BackColor = Color.FromArgb(241, 196, 15),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnRestoreVersion.Click += BtnRestoreVersion_Click;

            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(205, 12),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(127, 140, 141),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnClose.Click += (s, e) => this.Close();

            panelBottom.Controls.Add(btnRestoreVersion);
            panelBottom.Controls.Add(btnClose);

            // Add controls to form
            this.Controls.Add(gridHistory);
            this.Controls.Add(panelFieldSelect);
            this.Controls.Add(panelHeader);
            this.Controls.Add(panelBottom);
        }

        private void LoadFieldHistory()
        {
            allHistory = new List<FieldHistoryEntry>();

            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                SELECT fh.FieldName, fh.OldValue, fh.NewValue, fh.ChangedAt,
                       fh.ChangeSource, w.Username
                FROM FieldHistory fh
                LEFT JOIN Workers w ON fh.ChangedBy = w.Id
                WHERE fh.ClientId = @clientId
                ORDER BY fh.FieldName, fh.ChangedAt DESC";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@clientId", clientId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                allHistory.Add(new FieldHistoryEntry
                {
                    FieldName = reader["FieldName"].ToString(),
                    OldValue = reader["OldValue"]?.ToString(),
                    NewValue = reader["NewValue"]?.ToString(),
                    ChangedAt = Convert.ToDateTime(reader["ChangedAt"]),
                    ChangeSource = reader["ChangeSource"]?.ToString(),
                    ChangedBy = reader["Username"]?.ToString()
                });
            }

            // Populate field dropdown with ALL fields (basic + custom)
            var allFields = new List<string>();

            // Add standard fields
            allFields.Add("Name");
            allFields.Add("Phone");
            allFields.Add("Email");
            allFields.Add("DOB");

            // Add custom fields
            var customFieldRepo = new CustomFieldRepository();
            var customFields = customFieldRepo.GetAll();
            foreach (var field in customFields)
            {
                allFields.Add(field.FieldName);
            }

            // Add any fields from history that aren't in the above lists (legacy fields)
            var historyFields = allHistory.Select(h => h.FieldName).Distinct();
            foreach (var field in historyFields)
            {
                if (!allFields.Contains(field))
                {
                    allFields.Add(field);
                }
            }

            allFields = allFields.OrderBy(f => f).ToList();

            if (allFields.Count == 0)
            {
                cboField.Items.Add("(No fields available)");
                cboField.SelectedIndex = 0;
                cboField.Enabled = false;
                btnRestoreVersion.Enabled = false;
            }
            else
            {
                foreach (var field in allFields)
                {
                    cboField.Items.Add(field);
                }
                cboField.SelectedIndex = 0;
            }
        }

        private void CboField_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboField.SelectedItem == null || cboField.SelectedItem.ToString() == "(No fields available)")
                return;

            string selectedField = cboField.SelectedItem.ToString();
            var fieldHistory = allHistory.Where(h => h.FieldName == selectedField).OrderByDescending(h => h.ChangedAt).ToList();

            // Show current value
            string currentValue = GetCurrentFieldValue(selectedField);
            lblCurrentValue.Text = $"Current Value: {currentValue}";

            // Setup grid columns
            gridHistory.DataSource = null;
            gridHistory.Columns.Clear();

            gridHistory.Columns.Add("Version", "Version");
            gridHistory.Columns.Add("OldValue", "Old Value");
            gridHistory.Columns.Add("NewValue", "New Value");
            gridHistory.Columns.Add("ChangedAt", "Changed At");
            gridHistory.Columns.Add("ChangedBy", "Changed By");
            gridHistory.Columns.Add("Source", "Source");

            if (fieldHistory.Count == 0)
            {
                // No history for this field yet, show message
                gridHistory.Rows.Add(
                    "-",
                    "(no history)",
                    currentValue,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    "N/A",
                    "Current"
                );
                btnRestoreVersion.Enabled = false;
            }
            else
            {
                // Add rows
                int versionNumber = fieldHistory.Count;
                foreach (var entry in fieldHistory)
                {
                    gridHistory.Rows.Add(
                        $"v{versionNumber}",
                        entry.OldValue ?? "(empty)",
                        entry.NewValue ?? "(empty)",
                        entry.ChangedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        entry.ChangedBy ?? "System",
                        entry.ChangeSource ?? "Manual"
                    );
                    versionNumber--;
                }
                btnRestoreVersion.Enabled = true;
            }

            // Format columns
            gridHistory.Columns["Version"].Width = 70;
            gridHistory.Columns["OldValue"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            gridHistory.Columns["NewValue"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            gridHistory.Columns["ChangedAt"].Width = 150;
            gridHistory.Columns["ChangedBy"].Width = 120;
            gridHistory.Columns["Source"].Width = 100;
        }

        private string GetCurrentFieldValue(string fieldName)
        {
            // Check standard fields
            switch (fieldName)
            {
                case "Name": return currentClient.Name;
                case "Phone": return currentClient.Phone ?? "(empty)";
                case "Email": return currentClient.Email ?? "(empty)";
                case "DOB": return currentClient.DOB?.ToString("yyyy-MM-dd") ?? "(empty)";
                default:
                    // Check custom fields in ExtraData
                    return currentClient.GetExtraDataValue(fieldName) ?? "(empty)";
            }
        }

        private void BtnRestoreVersion_Click(object sender, EventArgs e)
        {
            if (gridHistory.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a version to restore.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string selectedField = cboField.SelectedItem.ToString();
            string oldValue = gridHistory.SelectedRows[0].Cells["OldValue"].Value.ToString();
            if (oldValue == "(empty)") oldValue = "";

            var result = MessageBox.Show(
                $"Restore field '{selectedField}' to value:\n\n{oldValue}\n\nThis will create a new entry in the change history.",
                "Confirm Restore",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    RestoreFieldValue(selectedField, oldValue);
                    MessageBox.Show("Field value restored successfully.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Reload history
                    allHistory.Clear();
                    cboField.Items.Clear();
                    LoadFieldHistory();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error restoring value: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void RestoreFieldValue(string fieldName, string value)
        {
            var repository = new ClientRepository();
            int userId = AuthService.Instance.GetCurrentUserId();

            // Update the field
            if (IsStandardField(fieldName))
            {
                // Update standard field
                switch (fieldName)
                {
                    case "Name": currentClient.Name = value; break;
                    case "Phone": currentClient.Phone = value; break;
                    case "Email": currentClient.Email = value; break;
                    case "DOB":
                        currentClient.DOB = DateTime.TryParse(value, out var dob) ? dob : (DateTime?)null;
                        break;
                }
                repository.UpdateClient(currentClient, userId, true);
            }
            else
            {
                // Update custom field
                repository.UpdateExtraData(clientId, fieldName, value, userId);
            }
        }

        private bool IsStandardField(string fieldName)
        {
            return fieldName == "Name" || fieldName == "Phone" ||
                   fieldName == "Email" || fieldName == "DOB";
        }

        private class FieldHistoryEntry
        {
            public string FieldName { get; set; }
            public string OldValue { get; set; }
            public string NewValue { get; set; }
            public DateTime ChangedAt { get; set; }
            public string ChangeSource { get; set; }
            public string ChangedBy { get; set; }
        }
    }
}
