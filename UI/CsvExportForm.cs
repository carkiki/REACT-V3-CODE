using System;

using System.Collections.Generic;

using System.Drawing;

using System.IO;

using System.Linq;

using System.Text;

using System.Windows.Forms;

using ReactCRM.Database;

using ReactCRM.Models;

using ReactCRM.Services;



namespace ReactCRM.UI.Import

{

    public partial class CsvExportForm : Form

    {

        private Panel panelFields;

        private Panel panelFilters;

        private Panel panelBottom;

        private CheckedListBox chkStandardFields;

        private CheckedListBox chkCustomFields;

        private RadioButton rbAllClients;

        private RadioButton rbFilteredClients;

        private TextBox txtSearchFilter;

        private Button btnSelectAll;

        private Button btnDeselectAll;

        private Button btnExport;

        private Button btnCancel;

        private Label lblStatus;

        private ProgressBar progressBar;

        private ComboBox cboDelimiter;

        private CheckBox chkIncludeHeaders;



        private ClientRepository clientRepository;

        private CustomFieldRepository customFieldRepository;

        private List<CustomField> customFields;



        public CsvExportForm()

        {

            clientRepository = new ClientRepository();

            customFieldRepository = new CustomFieldRepository();

            customFields = customFieldRepository.GetAll();

            InitializeComponent();

            LoadFieldOptions();

        }



        private void InitializeComponent()

        {

            this.Text = "Export to CSV";

            this.Size = new Size(900, 650);

            this.StartPosition = FormStartPosition.CenterParent;

            this.MinimumSize = new Size(800, 600);

            this.BackColor = Color.FromArgb(236, 240, 241);



            // Header Panel

            var panelHeader = new Panel

            {

                Dock = DockStyle.Top,

                Height = 80,

                BackColor = Color.FromArgb(52, 152, 219),

                Padding = new Padding(20)

            };



            var lblTitle = new Label

            {

                Text = "📤 Export Clients to CSV",

                Font = new Font("Segoe UI", 16, FontStyle.Bold),

                ForeColor = Color.White,

                Location = new Point(20, 10),

                AutoSize = true

            };



            var lblSubtitle = new Label

            {

                Text = "Select fields and filters to export client data",

                Font = new Font("Segoe UI", 10),

                ForeColor = Color.FromArgb(236, 240, 241),

                Location = new Point(20, 45),

                AutoSize = true

            };



            panelHeader.Controls.Add(lblTitle);

            panelHeader.Controls.Add(lblSubtitle);



            // Main container with split panels

            var panelMain = new Panel

            {

                Dock = DockStyle.Fill,

                Padding = new Padding(20)

            };



            // Left panel - Field Selection

            panelFields = new Panel

            {

                Location = new Point(20, 20),

                Size = new Size(400, 430),

                BackColor = Color.White,

                BorderStyle = BorderStyle.FixedSingle

            };



            var lblFieldsTitle = new Label

            {

                Text = "Select Fields to Export",

                Font = new Font("Segoe UI", 12, FontStyle.Bold),

                Location = new Point(15, 15),

                AutoSize = true

            };



            // Standard Fields

            var lblStandard = new Label

            {

                Text = "Standard Fields:",

                Font = new Font("Segoe UI", 10, FontStyle.Bold),

                Location = new Point(15, 50),

                AutoSize = true

            };



            chkStandardFields = new CheckedListBox

            {

                Location = new Point(15, 75),

                Size = new Size(360, 120),

                Font = new Font("Segoe UI", 9),

                CheckOnClick = true

            };



            // Custom Fields

            var lblCustom = new Label

            {

                Text = "Custom Fields:",

                Font = new Font("Segoe UI", 10, FontStyle.Bold),

                Location = new Point(15, 210),

                AutoSize = true

            };



            chkCustomFields = new CheckedListBox

            {

                Location = new Point(15, 235),

                Size = new Size(360, 140),

                Font = new Font("Segoe UI", 9),

                CheckOnClick = true

            };



            // Select/Deselect buttons

            btnSelectAll = new Button

            {

                Text = "Select All",

                Location = new Point(15, 385),

                Size = new Size(100, 30),

                BackColor = Color.FromArgb(52, 152, 219),

                ForeColor = Color.White,

                FlatStyle = FlatStyle.Flat,

                Cursor = Cursors.Hand

            };

            btnSelectAll.FlatAppearance.BorderSize = 0;

            btnSelectAll.Click += BtnSelectAll_Click;



            btnDeselectAll = new Button

            {

                Text = "Deselect All",

                Location = new Point(125, 385),

                Size = new Size(100, 30),

                BackColor = Color.FromArgb(127, 140, 141),

                ForeColor = Color.White,

                FlatStyle = FlatStyle.Flat,

                Cursor = Cursors.Hand

            };

            btnDeselectAll.FlatAppearance.BorderSize = 0;

            btnDeselectAll.Click += BtnDeselectAll_Click;



            panelFields.Controls.Add(lblFieldsTitle);

            panelFields.Controls.Add(lblStandard);

            panelFields.Controls.Add(chkStandardFields);

            panelFields.Controls.Add(lblCustom);

            panelFields.Controls.Add(chkCustomFields);

            panelFields.Controls.Add(btnSelectAll);

            panelFields.Controls.Add(btnDeselectAll);



            // Right panel - Filters and Options

            panelFilters = new Panel

            {

                Location = new Point(440, 20),

                Size = new Size(400, 430),

                BackColor = Color.White,

                BorderStyle = BorderStyle.FixedSingle

            };



            var lblFiltersTitle = new Label

            {

                Text = "Export Options",

                Font = new Font("Segoe UI", 12, FontStyle.Bold),

                Location = new Point(15, 15),

                AutoSize = true

            };



            // Client Filter Options

            var lblClientFilter = new Label

            {

                Text = "Clients to Export:",

                Font = new Font("Segoe UI", 10, FontStyle.Bold),

                Location = new Point(15, 50),

                AutoSize = true

            };



            rbAllClients = new RadioButton

            {

                Text = "Export all clients",

                Location = new Point(15, 80),

                AutoSize = true,

                Checked = true,

                Font = new Font("Segoe UI", 9)

            };

            rbAllClients.CheckedChanged += (s, e) =>

            {

                txtSearchFilter.Enabled = !rbAllClients.Checked;

            };



            rbFilteredClients = new RadioButton

            {

                Text = "Export filtered clients (search):",

                Location = new Point(15, 110),

                AutoSize = true,

                Font = new Font("Segoe UI", 9)

            };



            txtSearchFilter = new TextBox

            {

                Location = new Point(30, 140),

                Size = new Size(340, 25),

                Font = new Font("Segoe UI", 9),

                Enabled = false,

                PlaceholderText = "Enter search term (name, SSN, email, phone)"

            };



            var lblFilterHelp = new Label

            {

                Text = "Search will look in all standard and custom fields",

                Location = new Point(30, 170),

                Size = new Size(340, 30),

                Font = new Font("Segoe UI", 8, FontStyle.Italic),

                ForeColor = Color.Gray

            };



            // CSV Options

            var lblCsvOptions = new Label

            {

                Text = "CSV Format Options:",

                Font = new Font("Segoe UI", 10, FontStyle.Bold),

                Location = new Point(15, 215),

                AutoSize = true

            };



            var lblDelimiter = new Label

            {

                Text = "Delimiter:",

                Location = new Point(15, 245),

                AutoSize = true,

                Font = new Font("Segoe UI", 9)

            };



            cboDelimiter = new ComboBox

            {

                Location = new Point(100, 242),

                Width = 150,

                DropDownStyle = ComboBoxStyle.DropDownList,

                Font = new Font("Segoe UI", 9)

            };

            cboDelimiter.Items.AddRange(new object[] { "Comma (,)", "Semicolon (;)", "Tab", "Pipe (|)" });

            cboDelimiter.SelectedIndex = 0;



            chkIncludeHeaders = new CheckBox

            {

                Text = "Include column headers in first row",

                Location = new Point(15, 280),

                AutoSize = true,

                Checked = true,

                Font = new Font("Segoe UI", 9)

            };



            // Export Info

            var lblExportInfo = new Label

            {

                Text = "ℹ️ Export Information",

                Font = new Font("Segoe UI", 10, FontStyle.Bold),

                Location = new Point(15, 320),

                AutoSize = true

            };



            var lblInfo = new Label

            {

                Text = "• Dates will be exported in M/d/yyyy format\n" +

                       "• Custom field values will be cleaned\n" +

                       "• Empty fields will be exported as blank\n" +

                       "• File will be saved as UTF-8 encoding",

                Location = new Point(15, 350),

                Size = new Size(360, 65),

                Font = new Font("Segoe UI", 8),

                ForeColor = Color.FromArgb(52, 73, 94)

            };



            panelFilters.Controls.Add(lblFiltersTitle);

            panelFilters.Controls.Add(lblClientFilter);

            panelFilters.Controls.Add(rbAllClients);

            panelFilters.Controls.Add(rbFilteredClients);

            panelFilters.Controls.Add(txtSearchFilter);

            panelFilters.Controls.Add(lblFilterHelp);

            panelFilters.Controls.Add(lblCsvOptions);

            panelFilters.Controls.Add(lblDelimiter);

            panelFilters.Controls.Add(cboDelimiter);

            panelFilters.Controls.Add(chkIncludeHeaders);

            panelFilters.Controls.Add(lblExportInfo);

            panelFilters.Controls.Add(lblInfo);



            panelMain.Controls.Add(panelFields);

            panelMain.Controls.Add(panelFilters);



            // Bottom Panel - Status and Actions

            panelBottom = new Panel

            {

                Dock = DockStyle.Bottom,

                Height = 100,

                BackColor = Color.FromArgb(236, 240, 241),

                Padding = new Padding(20)

            };



            lblStatus = new Label

            {

                Text = "Ready to export",

                Location = new Point(20, 15),

                AutoSize = true,

                Font = new Font("Segoe UI", 9, FontStyle.Italic),

                ForeColor = Color.FromArgb(52, 73, 94)

            };



            progressBar = new ProgressBar

            {

                Location = new Point(20, 40),

                Width = 640,

                Height = 20,

                Visible = false

            };



            btnExport = new Button

            {

                Text = "Export to CSV",

                Location = new Point(680, 35),

                Size = new Size(120, 35),

                BackColor = Color.FromArgb(46, 204, 113),

                ForeColor = Color.White,

                FlatStyle = FlatStyle.Flat,

                Font = new Font("Segoe UI", 10, FontStyle.Bold),

                Cursor = Cursors.Hand

            };

            btnExport.FlatAppearance.BorderSize = 0;

            btnExport.Click += BtnExport_Click;



            btnCancel = new Button

            {

                Text = "Cancel",

                Location = new Point(810, 35),

                Size = new Size(80, 35),

                BackColor = Color.FromArgb(127, 140, 141),

                ForeColor = Color.White,

                FlatStyle = FlatStyle.Flat,

                Font = new Font("Segoe UI", 10),

                Cursor = Cursors.Hand

            };

            btnCancel.FlatAppearance.BorderSize = 0;

            btnCancel.Click += (s, e) => this.Close();



            panelBottom.Controls.Add(lblStatus);

            panelBottom.Controls.Add(progressBar);

            panelBottom.Controls.Add(btnExport);

            panelBottom.Controls.Add(btnCancel);



            // Add all panels to form

            this.Controls.Add(panelMain);

            this.Controls.Add(panelBottom);

            this.Controls.Add(panelHeader);

        }



        private void LoadFieldOptions()

        {

            // Load standard fields

            chkStandardFields.Items.Clear();

            chkStandardFields.Items.Add("SSN", true);

            chkStandardFields.Items.Add("Name", true);

            chkStandardFields.Items.Add("DOB (Date of Birth)", true);

            chkStandardFields.Items.Add("Phone", true);

            chkStandardFields.Items.Add("Email", true);

            chkStandardFields.Items.Add("Notes", false);



            // Load custom fields

            chkCustomFields.Items.Clear();

            foreach (var field in customFields)

            {

                chkCustomFields.Items.Add(field.Label, true); // Check all by default

            }



            UpdateStatus();

        }



        private void BtnSelectAll_Click(object sender, EventArgs e)

        {

            for (int i = 0; i < chkStandardFields.Items.Count; i++)

                chkStandardFields.SetItemChecked(i, true);



            for (int i = 0; i < chkCustomFields.Items.Count; i++)

                chkCustomFields.SetItemChecked(i, true);



            UpdateStatus();

        }



        private void BtnDeselectAll_Click(object sender, EventArgs e)

        {

            for (int i = 0; i < chkStandardFields.Items.Count; i++)

                chkStandardFields.SetItemChecked(i, false);



            for (int i = 0; i < chkCustomFields.Items.Count; i++)

                chkCustomFields.SetItemChecked(i, false);



            UpdateStatus();

        }



        private void UpdateStatus()

        {

            int selectedCount = chkStandardFields.CheckedItems.Count + chkCustomFields.CheckedItems.Count;

            lblStatus.Text = $"Ready to export - {selectedCount} field(s) selected";

        }



        private char GetDelimiter()

        {

            switch (cboDelimiter.SelectedIndex)

            {

                case 1: return ';';

                case 2: return '\t';

                case 3: return '|';

                default: return ',';

            }

        }



        private void BtnExport_Click(object sender, EventArgs e)

        {

            // Validate field selection

            if (chkStandardFields.CheckedItems.Count == 0 && chkCustomFields.CheckedItems.Count == 0)

            {

                MessageBox.Show("Please select at least one field to export.", "No Fields Selected",

                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }



            // Show save file dialog

            using (var saveFileDialog = new SaveFileDialog())

            {

                saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";

                saveFileDialog.Title = "Save CSV Export";

                saveFileDialog.FileName = $"clients_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";



                if (saveFileDialog.ShowDialog() == DialogResult.OK)

                {

                    PerformExport(saveFileDialog.FileName);

                }

            }

        }



        private void PerformExport(string filePath)

        {

            progressBar.Visible = true;

            btnExport.Enabled = false;

            lblStatus.Text = "Preparing export...";

            Application.DoEvents();



            try

            {

                // Get clients to export

                List<Client> clients;

                if (rbAllClients.Checked)

                {

                    clients = clientRepository.GetAllClients();

                    lblStatus.Text = $"Loading {clients.Count} clients...";

                }

                else

                {

                    string searchTerm = txtSearchFilter.Text.Trim();

                    if (string.IsNullOrEmpty(searchTerm))

                    {

                        MessageBox.Show("Please enter a search term for filtered export.", "Search Required",

                            MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        return;

                    }

                    clients = clientRepository.SearchClients(searchTerm);

                    lblStatus.Text = $"Found {clients.Count} matching clients...";

                }



                Application.DoEvents();



                if (clients.Count == 0)

                {

                    MessageBox.Show("No clients found to export.", "No Data",

                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    return;

                }



                // Prepare selected fields

                var selectedStandardFields = new List<string>();

                foreach (var item in chkStandardFields.CheckedItems)

                {

                    selectedStandardFields.Add(item.ToString());

                }



                var selectedCustomFields = new List<string>();

                foreach (var item in chkCustomFields.CheckedItems)

                {

                    selectedCustomFields.Add(item.ToString());

                }



                // Generate CSV

                char delimiter = GetDelimiter();

                progressBar.Maximum = clients.Count;

                progressBar.Value = 0;



                using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))

                {

                    // Write headers if requested

                    if (chkIncludeHeaders.Checked)

                    {

                        var headers = new List<string>();



                        foreach (var field in selectedStandardFields)

                        {

                            headers.Add(EscapeCsvValue(field, delimiter));

                        }



                        foreach (var field in selectedCustomFields)

                        {

                            headers.Add(EscapeCsvValue(field, delimiter));

                        }



                        writer.WriteLine(string.Join(delimiter.ToString(), headers));

                    }



                    // Write data rows

                    int rowCount = 0;

                    foreach (var client in clients)

                    {

                        var values = new List<string>();



                        // Add standard field values

                        foreach (var field in selectedStandardFields)

                        {

                            string value = GetStandardFieldValue(client, field);

                            values.Add(EscapeCsvValue(value, delimiter));

                        }



                        // Add custom field values

                        foreach (var fieldLabel in selectedCustomFields)

                        {

                            // Find the custom field by label

                            var customField = customFields.FirstOrDefault(f => f.Label == fieldLabel);

                            string value = "";



                            if (customField != null)

                            {

                                value = client.GetExtraDataValue(customField.FieldName) ?? "";

                            }



                            values.Add(EscapeCsvValue(value, delimiter));

                        }



                        writer.WriteLine(string.Join(delimiter.ToString(), values));



                        rowCount++;

                        progressBar.Value = rowCount;



                        // Update status every 100 rows

                        if (rowCount % 100 == 0)

                        {

                            lblStatus.Text = $"Exporting... {rowCount}/{clients.Count}";

                            Application.DoEvents();

                        }

                    }

                }



                // Log export

                AuditService.LogExport(Path.GetFileName(filePath), clients.Count);



                lblStatus.Text = $"Export completed: {clients.Count} clients exported";



                MessageBox.Show($"Successfully exported {clients.Count} clients to:\n{filePath}",

                    "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);



                // Ask if user wants to open the file

                var result = MessageBox.Show("Would you like to open the exported file?", "Open File",

                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);



                if (result == DialogResult.Yes)

                {

                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo

                    {

                        FileName = filePath,

                        UseShellExecute = true

                    });

                }



                this.Close();

            }

            catch (Exception ex)

            {

                MessageBox.Show($"Error exporting CSV: {ex.Message}", "Export Error",

                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                lblStatus.Text = "Export failed";

            }

            finally

            {

                progressBar.Visible = false;

                btnExport.Enabled = true;

            }

        }



        private string GetStandardFieldValue(Client client, string fieldName)

        {

            switch (fieldName)

            {

                case "SSN":

                    return client.SSN ?? "";

                case "Name":

                    return client.Name ?? "";

                case "DOB (Date of Birth)":

                    return client.DOB.HasValue ? client.DOB.Value.ToString("M/d/yyyy") : "";

                case "Phone":

                    return client.Phone ?? "";

                case "Email":

                    return client.Email ?? "";

                case "Notes":

                    return client.Notes ?? "";

                default:

                    return "";

            }

        }



        private string EscapeCsvValue(string value, char delimiter)

        {

            if (string.IsNullOrEmpty(value))

                return "";



            // If value contains delimiter, newline, or quote, wrap in quotes

            if (value.Contains(delimiter) || value.Contains('\n') || value.Contains('\r') || value.Contains('"'))

            {

                // Escape quotes by doubling them

                value = value.Replace("\"", "\"\"");

                return $"\"{value}\"";

            }



            return value;

        }

    }

}