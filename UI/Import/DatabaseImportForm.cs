using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using ReactCRM.Database;
using ReactCRM.Models;
using ReactCRM.Services;

namespace ReactCRM.UI.Import
{
    public partial class DatabaseImportForm : Form
    {
        private ComboBox cboDbType;
        private TextBox txtConnectionString;
        private TextBox txtQuery;
        private Button btnTestConnection;
        private Button btnPreview;
        private Button btnImport;
        private Button btnCancel;
        private DataGridView gridPreview;
        private Label lblStatus;
        private ProgressBar progressBar;
        private DataTable previewData;
        private ClientRepository clientRepository;

        public DatabaseImportForm()
        {
            clientRepository = new ClientRepository();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Database Import";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(800, 600);

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
                Text = "Import from External Database",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 15),
                AutoSize = true
            };

            var lblSubtitle = new Label
            {
                Text = "Connect to external database and import client data",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(189, 195, 199),
                Location = new Point(15, 45),
                AutoSize = true
            };

            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(lblSubtitle);

            // Settings Panel
            var panelSettings = new Panel
            {
                Dock = DockStyle.Top,
                Height = 280,
                BackColor = Color.FromArgb(236, 240, 241),
                Padding = new Padding(15)
            };

            int yPos = 15;

            // Database Type
            var lblDbType = new Label
            {
                Text = "Database Type:",
                Location = new Point(15, yPos),
                Width = 150,
                Font = new Font("Segoe UI", 10)
            };

            cboDbType = new ComboBox
            {
                Location = new Point(170, yPos - 3),
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cboDbType.Items.AddRange(new object[] {
                "SQL Server",
                "MySQL",
                "PostgreSQL",
                "SQLite",
                "Oracle"
            });
            cboDbType.SelectedIndex = 0;
            cboDbType.SelectedIndexChanged += CboDbType_SelectedIndexChanged;
            yPos += 35;

            // Connection String
            var lblConnection = new Label
            {
                Text = "Connection String:",
                Location = new Point(15, yPos),
                Width = 150,
                Font = new Font("Segoe UI", 10)
            };

            txtConnectionString = new TextBox
            {
                Location = new Point(170, yPos - 3),
                Width = 650,
                Height = 60,
                Multiline = true,
                Font = new Font("Consolas", 9),
                ScrollBars = ScrollBars.Vertical
            };
            yPos += 70;

            // Connection String Help
            var lblHelp = new Label
            {
                Text = "Example: Server=localhost;Database=mydb;User Id=user;Password=pass;",
                Location = new Point(170, yPos),
                Width = 650,
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Color.Gray
            };
            yPos += 25;

            btnTestConnection = new Button
            {
                Text = "Test Connection",
                Location = new Point(170, yPos),
                Size = new Size(140, 30),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnTestConnection.Click += BtnTestConnection_Click;
            yPos += 40;

            // Query
            var lblQuery = new Label
            {
                Text = "SQL Query:",
                Location = new Point(15, yPos),
                Width = 150,
                Font = new Font("Segoe UI", 10)
            };

            var lblQueryNote = new Label
            {
                Text = "Must return columns: SSN, Name, DOB, Phone, Email",
                Location = new Point(170, yPos + 2),
                Width = 400,
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Color.Gray
            };
            yPos += 20;

            txtQuery = new TextBox
            {
                Location = new Point(170, yPos),
                Width = 650,
                Height = 60,
                Multiline = true,
                Font = new Font("Consolas", 9),
                ScrollBars = ScrollBars.Vertical,
                Text = "SELECT SSN, Name, DOB, Phone, Email FROM Clients"
            };

            panelSettings.Controls.Add(lblDbType);
            panelSettings.Controls.Add(cboDbType);
            panelSettings.Controls.Add(lblConnection);
            panelSettings.Controls.Add(txtConnectionString);
            panelSettings.Controls.Add(lblHelp);
            panelSettings.Controls.Add(btnTestConnection);
            panelSettings.Controls.Add(lblQuery);
            panelSettings.Controls.Add(lblQueryNote);
            panelSettings.Controls.Add(txtQuery);

            // Preview Grid
            gridPreview = new DataGridView
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
                Height = 80,
                BackColor = Color.FromArgb(236, 240, 241),
                Padding = new Padding(15)
            };

            lblStatus = new Label
            {
                Text = "Ready to import",
                Location = new Point(15, 15),
                Width = 600,
                Font = new Font("Segoe UI", 9)
            };

            progressBar = new ProgressBar
            {
                Location = new Point(15, 40),
                Width = 600,
                Height = 25,
                Visible = false
            };

            btnPreview = new Button
            {
                Text = "Preview Data",
                Location = new Point(650, 10),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnPreview.Click += BtnPreview_Click;

            btnImport = new Button
            {
                Text = "Import",
                Location = new Point(780, 10),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnImport.Click += BtnImport_Click;

            btnCancel = new Button
            {
                Text = "Close",
                Location = new Point(890, 10),
                Size = new Size(80, 35),
                BackColor = Color.FromArgb(127, 140, 141),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnCancel.Click += (s, e) => this.Close();

            panelBottom.Controls.Add(lblStatus);
            panelBottom.Controls.Add(progressBar);
            panelBottom.Controls.Add(btnPreview);
            panelBottom.Controls.Add(btnImport);
            panelBottom.Controls.Add(btnCancel);

            // Add controls to form
            this.Controls.Add(gridPreview);
            this.Controls.Add(panelSettings);
            this.Controls.Add(panelHeader);
            this.Controls.Add(panelBottom);

            // Load default connection string template
            UpdateConnectionStringTemplate();
        }

        private void CboDbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateConnectionStringTemplate();
        }

        private void UpdateConnectionStringTemplate()
        {
            string template = cboDbType.SelectedItem?.ToString() switch
            {
                "SQL Server" => "Server=localhost;Database=MyDatabase;User Id=username;Password=password;TrustServerCertificate=True;",
                "MySQL" => "Server=localhost;Database=MyDatabase;Uid=username;Pwd=password;",
                "PostgreSQL" => "Host=localhost;Database=MyDatabase;Username=username;Password=password;",
                "SQLite" => "Data Source=path/to/database.db;",
                "Oracle" => "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=ORCL)));User Id=username;Password=password;",
                _ => ""
            };

            if (string.IsNullOrEmpty(txtConnectionString.Text))
            {
                txtConnectionString.Text = template;
            }
        }

        private void BtnTestConnection_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtConnectionString.Text))
            {
                MessageBox.Show("Please enter a connection string.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                lblStatus.Text = "Testing connection...";
                lblStatus.ForeColor = Color.Blue;
                Application.DoEvents();

                // Note: This is a placeholder. In production, you'd need to reference
                // appropriate database provider packages (System.Data.SqlClient, MySql.Data, Npgsql, etc.)

                MessageBox.Show(
                    "Connection test not fully implemented.\n\n" +
                    "To enable this feature, add references to:\n" +
                    "- System.Data.SqlClient (SQL Server)\n" +
                    "- MySql.Data (MySQL)\n" +
                    "- Npgsql (PostgreSQL)\n\n" +
                    "For now, use 'Preview Data' to test if the connection works.",
                    "Info",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                lblStatus.Text = "Ready - Use 'Preview Data' to test query";
                lblStatus.ForeColor = Color.Black;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection test failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = $"Connection failed: {ex.Message}";
                lblStatus.ForeColor = Color.Red;
            }
        }

        private void BtnPreview_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtConnectionString.Text))
            {
                MessageBox.Show("Please enter a connection string.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtQuery.Text))
            {
                MessageBox.Show("Please enter a SQL query.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                lblStatus.Text = "Fetching preview data...";
                lblStatus.ForeColor = Color.Blue;
                Application.DoEvents();

                // For SQLite demonstration (since we have Microsoft.Data.Sqlite)
                if (cboDbType.SelectedItem.ToString() == "SQLite")
                {
                    PreviewSQLiteData();
                }
                else
                {
                    MessageBox.Show(
                        $"{cboDbType.SelectedItem} import not fully implemented.\n\n" +
                        "To enable full database import support, add NuGet packages:\n" +
                        "- System.Data.SqlClient (SQL Server)\n" +
                        "- MySql.Data (MySQL)\n" +
                        "- Npgsql (PostgreSQL)\n\n" +
                        "SQLite import is available for demonstration.",
                        "Info",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    lblStatus.Text = "Only SQLite import is currently enabled";
                    lblStatus.ForeColor = Color.Orange;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = $"Error: {ex.Message}";
                lblStatus.ForeColor = Color.Red;
            }
        }

        private void PreviewSQLiteData()
        {
            using var connection = new SqliteConnection(txtConnectionString.Text);
            connection.Open();

            using (var cmd = new SqliteCommand(txtQuery.Text, connection))
            using (var reader = cmd.ExecuteReader())
            {
                previewData = new DataTable();

                // Create columns from reader schema
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    previewData.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
                }

                // Read all rows
                while (reader.Read())
                {
                    var row = previewData.NewRow();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[i] = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i);
                    }
                    previewData.Rows.Add(row);
                }

                gridPreview.DataSource = previewData;

                lblStatus.Text = $"Preview loaded: {previewData.Rows.Count} rows";
                lblStatus.ForeColor = Color.Green;
                btnImport.Enabled = previewData.Rows.Count > 0;
            }
        }

        private void BtnImport_Click(object sender, EventArgs e)
        {
            if (previewData == null || previewData.Rows.Count == 0)
            {
                MessageBox.Show("Please preview data first.", "No Data",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Import {previewData.Rows.Count} records?\n\n" +
                "Existing clients with matching SSNs will be updated.\n" +
                "New clients will be created.",
                "Confirm Import",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    progressBar.Visible = true;
                    progressBar.Maximum = previewData.Rows.Count;
                    progressBar.Value = 0;

                    int created = 0;
                    int updated = 0;
                    int errors = 0;
                    int userId = AuthService.Instance.GetCurrentUserId();

                    string importSource = cboDbType.SelectedItem?.ToString() ?? "External Database";

                    // Get current custom fields definitions
                    var customFieldRepo = new CustomFieldRepository();
                    var customFields = customFieldRepo.GetAll();

                    foreach (DataRow row in previewData.Rows)
                    {
                        try
                        {
                            var client = new Client
                            {
                                SSN = row["SSN"]?.ToString()?.Trim(),
                                Name = row["Name"]?.ToString()?.Trim(),
                                Phone = row["Phone"]?.ToString()?.Trim(),
                                Email = row["Email"]?.ToString()?.Trim()
                            };

                            if (row["DOB"] != DBNull.Value && DateTime.TryParse(row["DOB"].ToString(), out DateTime dob))
                            {
                                client.DOB = dob;
                            }

                            if (string.IsNullOrEmpty(client.SSN) || string.IsNullOrEmpty(client.Name))
                            {
                                errors++;
                                continue;
                            }

                            // Import custom fields from source data if columns exist
                            foreach (var field in customFields)
                            {
                                // Check if the source data has a column matching this custom field
                                if (previewData.Columns.Contains(field.FieldName))
                                {
                                    var value = row[field.FieldName]?.ToString()?.Trim() ?? "";
                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        client.SetExtraDataValue(field.FieldName, value);
                                    }
                                }
                            }

                            // Check if exists
                            var existing = clientRepository.GetClientBySSN(client.SSN);
                            if (existing != null)
                            {
                                // Update basic fields
                                existing.Name = client.Name;
                                existing.Phone = client.Phone;
                                existing.Email = client.Email;
                                existing.DOB = client.DOB;

                                // Update custom fields: only update if import has value, preserve if empty
                                if (existing.ExtraData == null)
                                {
                                    existing.ExtraData = new Dictionary<string, object>();
                                }

                                foreach (var field in customFields)
                                {
                                    if (previewData.Columns.Contains(field.FieldName))
                                    {
                                        var importValue = row[field.FieldName]?.ToString()?.Trim() ?? "";

                                        // Only update if import has a value, otherwise preserve existing
                                        if (!string.IsNullOrEmpty(importValue))
                                        {
                                            existing.ExtraData[field.FieldName] = importValue;
                                        }
                                        // If import value is empty, keep existing value (don't overwrite)
                                    }
                                }

                                clientRepository.UpdateClient(existing, userId, true);
                                updated++;

                                // Log import update action
                                AuditService.LogAction("IMPORT_UPDATE", "Client", existing.Id,
                                    $"Client '{existing.Name}' updated via import from {importSource}");
                            }
                            else
                            {
                                int newClientId = clientRepository.CreateClient(client, userId);
                                created++;

                                // Log import create action
                                AuditService.LogAction("IMPORT_CREATE", "Client", newClientId,
                                    $"Client '{client.Name}' imported from {importSource}");
                            }
                        }
                        catch
                        {
                            errors++;
                        }

                        progressBar.Value++;
                        lblStatus.Text = $"Importing... ({progressBar.Value}/{progressBar.Maximum})";
                        Application.DoEvents();
                    }

                    progressBar.Visible = false;
                    lblStatus.Text = $"Import complete: {created} created, {updated} updated, {errors} errors";
                    lblStatus.ForeColor = errors > 0 ? Color.Orange : Color.Green;

                    MessageBox.Show(
                        $"Import completed successfully!\n\n" +
                        $"Created: {created}\n" +
                        $"Updated: {updated}\n" +
                        $"Errors: {errors}",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    AuditService.LogAction("Import", "Database", null,
                        $"Imported {created + updated} clients from external database");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error during import: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    lblStatus.Text = $"Import failed: {ex.Message}";
                    lblStatus.ForeColor = Color.Red;
                    progressBar.Visible = false;
                }
            }
        }
    }
}
