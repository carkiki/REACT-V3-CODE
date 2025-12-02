using Microsoft.Data.Sqlite;

using Newtonsoft.Json;

using ReactCRM.Database;

using ReactCRM.Models;

using ReactCRM.Services;

using System;

using System.Collections.Generic;

using System.Drawing;

using System.Linq;

using System.Text;

using System.Windows.Forms;

using File = System.IO.File;

using StreamReader = System.IO.StreamReader;



namespace ReactCRM.UI.Import

{

    public partial class CsvMapperForm : Form

    {

        private TextBox txtFilePath;

        private Button btnBrowse;

        private DataGridView gridPreview;

        private DataGridView gridMapping;

        private Button btnImport;

        private Button btnCancel;

        private CheckBox chkHasHeaders;

        private Label lblStatus;

        private ProgressBar progressBar;

        private ComboBox cboDelimiter;



        private string[] csvHeaders;

        private List<string[]> csvData;

        private ClientRepository clientRepository;

        private CustomFieldRepository customFieldRepository;

        private List<CustomField> customFields;



        public CsvMapperForm()

        {

            clientRepository = new ClientRepository();

            customFieldRepository = new CustomFieldRepository();

            InitializeComponent();

        }



        private void InitializeComponent()

        {

            this.Text = "CSV Import Wizard";

            this.Size = new Size(1000, 700);

            this.StartPosition = FormStartPosition.CenterParent;

            this.MinimumSize = new Size(800, 600);



            // File Selection Panel

            var panelFile = new Panel

            {

                Dock = DockStyle.Top,

                Height = 100,

                BackColor = Color.FromArgb(236, 240, 241),

                Padding = new Padding(10)

            };



            var lblFile = new Label

            {

                Text = "CSV File:",

                Location = new Point(10, 15),

                AutoSize = true,

                Font = new Font("Segoe UI", 10)

            };



            txtFilePath = new TextBox

            {

                Location = new Point(80, 12),

                Width = 600,

                Font = new Font("Segoe UI", 10),

                ReadOnly = true

            };



            btnBrowse = new Button

            {

                Text = "Browse...",

                Location = new Point(690, 10),

                Size = new Size(100, 28),

                Font = new Font("Segoe UI", 9),

                Cursor = Cursors.Hand

            };

            btnBrowse.Click += BtnBrowse_Click;



            chkHasHeaders = new CheckBox

            {

                Text = "First row contains headers",

                Location = new Point(10, 50),

                AutoSize = true,

                Checked = true,

                Font = new Font("Segoe UI", 9)

            };

            chkHasHeaders.CheckedChanged += (s, e) => LoadCsvPreview();



            var lblDelimiter = new Label

            {

                Text = "Delimiter:",

                Location = new Point(250, 52),

                AutoSize = true,

                Font = new Font("Segoe UI", 9)

            };



            cboDelimiter = new ComboBox

            {

                Location = new Point(320, 48),

                Width = 100,

                DropDownStyle = ComboBoxStyle.DropDownList,

                Font = new Font("Segoe UI", 9)

            };

            cboDelimiter.Items.AddRange(new object[] { "Comma (,)", "Semicolon (;)", "Tab", "Pipe (|)" });

            cboDelimiter.SelectedIndex = 0;

            cboDelimiter.SelectedIndexChanged += (s, e) => LoadCsvPreview();



            panelFile.Controls.Add(lblFile);

            panelFile.Controls.Add(txtFilePath);

            panelFile.Controls.Add(btnBrowse);

            panelFile.Controls.Add(chkHasHeaders);

            panelFile.Controls.Add(lblDelimiter);

            panelFile.Controls.Add(cboDelimiter);



            // Preview Panel

            var panelPreview = new Panel

            {

                Dock = DockStyle.Top,

                Height = 200,

                Padding = new Padding(10)

            };



            var lblPreview = new Label

            {

                Text = "CSV Preview (first 10 rows):",

                Dock = DockStyle.Top,

                Font = new Font("Segoe UI", 10, FontStyle.Bold),

                Height = 25

            };



            gridPreview = new DataGridView

            {

                Dock = DockStyle.Fill,

                ReadOnly = true,

                AllowUserToAddRows = false,

                AllowUserToDeleteRows = false,

                BackgroundColor = Color.White,

                BorderStyle = BorderStyle.Fixed3D,

                Font = new Font("Segoe UI", 9)

            };



            panelPreview.Controls.Add(gridPreview);

            panelPreview.Controls.Add(lblPreview);



            // Mapping Panel

            var panelMapping = new Panel

            {

                Dock = DockStyle.Fill,

                Padding = new Padding(10)

            };



            var lblMapping = new Label

            {

                Text = "Column Mapping:",

                Dock = DockStyle.Top,

                Font = new Font("Segoe UI", 10, FontStyle.Bold),

                Height = 25

            };



            gridMapping = new DataGridView

            {

                Dock = DockStyle.Fill,

                AllowUserToAddRows = false,

                AllowUserToDeleteRows = false,

                BackgroundColor = Color.White,

                BorderStyle = BorderStyle.Fixed3D,

                Font = new Font("Segoe UI", 9),

                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,

                ScrollBars = ScrollBars.Both,

                RowHeadersVisible = true,

                AllowUserToResizeRows = true

            };



            SetupMappingGrid();



            panelMapping.Controls.Add(gridMapping);

            panelMapping.Controls.Add(lblMapping);



            // Bottom Panel

            var panelBottom = new Panel

            {

                Dock = DockStyle.Bottom,

                Height = 100,

                BackColor = Color.FromArgb(236, 240, 241),

                Padding = new Padding(10)

            };



            lblStatus = new Label

            {

                Text = "Select a CSV file to begin",

                Location = new Point(10, 15),

                AutoSize = true,

                Font = new Font("Segoe UI", 9, FontStyle.Italic)

            };



            progressBar = new ProgressBar

            {

                Location = new Point(10, 40),

                Width = 760,

                Height = 20,

                Visible = false

            };



            btnImport = new Button

            {

                Text = "Import",

                Location = new Point(800, 35),

                Size = new Size(100, 35),

                BackColor = Color.FromArgb(46, 204, 113),

                ForeColor = Color.White,

                FlatStyle = FlatStyle.Flat,

                Font = new Font("Segoe UI", 10),

                Cursor = Cursors.Hand,

                Enabled = false

            };

            btnImport.Click += BtnImport_Click;



            btnCancel = new Button

            {

                Text = "Cancel",

                Location = new Point(910, 35),

                Size = new Size(100, 35),

                BackColor = Color.FromArgb(127, 140, 141),

                ForeColor = Color.White,

                FlatStyle = FlatStyle.Flat,

                Font = new Font("Segoe UI", 10),

                Cursor = Cursors.Hand

            };

            btnCancel.Click += (s, e) => this.Close();



            panelBottom.Controls.Add(lblStatus);

            panelBottom.Controls.Add(progressBar);

            panelBottom.Controls.Add(btnImport);

            panelBottom.Controls.Add(btnCancel);



            // Add all panels to form

            this.Controls.Add(panelMapping);

            this.Controls.Add(panelPreview);

            this.Controls.Add(panelFile);

            this.Controls.Add(panelBottom);

        }



        private void SetupMappingGrid()

        {

            gridMapping.Columns.Clear();



            var colCsvColumn = new DataGridViewTextBoxColumn

            {

                Name = "CsvColumn",

                HeaderText = "CSV Column",

                ReadOnly = true,

                Width = 200

            };



            var colTargetField = new DataGridViewComboBoxColumn

            {

                Name = "TargetField",

                HeaderText = "Map To Field",

                Width = 250

            };



            // Add standard fields and custom fields

            var targetFields = new List<string> { "(Skip)", "SSN", "Name", "DOB", "Phone", "Email" };



            // Add custom fields

            customFields = customFieldRepository.GetAll();

            foreach (var field in customFields)

            {

                targetFields.Add($"Custom: {field.Label}");

            }



            colTargetField.Items.AddRange(targetFields.ToArray());



            var colSample = new DataGridViewTextBoxColumn

            {

                Name = "SampleData",

                HeaderText = "Sample Data",

                ReadOnly = true

            };



            gridMapping.Columns.Add(colCsvColumn);

            gridMapping.Columns.Add(colTargetField);

            gridMapping.Columns.Add(colSample);

        }



        private void BtnBrowse_Click(object sender, EventArgs e)

        {

            using (var openFileDialog = new OpenFileDialog())

            {

                openFileDialog.Filter = "CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt|All files (*.*)|*.*";

                openFileDialog.Title = "Select CSV File";



                if (openFileDialog.ShowDialog() == DialogResult.OK)

                {

                    txtFilePath.Text = openFileDialog.FileName;

                    LoadCsvPreview();

                }

            }

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



        private void LoadCsvPreview()

        {

            if (string.IsNullOrEmpty(txtFilePath.Text) || !File.Exists(txtFilePath.Text))

                return;



            try

            {

                csvData = new List<string[]>();

                char delimiter = GetDelimiter();



                using (var reader = new StreamReader(txtFilePath.Text, Encoding.UTF8))

                {

                    // Read ALL rows

                    while (!reader.EndOfStream)

                    {

                        string line = reader.ReadLine();

                        if (!string.IsNullOrWhiteSpace(line))

                        {

                            var values = ParseCsvLine(line, delimiter);

                            csvData.Add(values);

                        }

                    }

                }



                if (csvData.Count == 0)

                {

                    MessageBox.Show("The CSV file is empty.", "Empty File",

                        MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    return;

                }



                // Set headers

                if (chkHasHeaders.Checked)

                {

                    csvHeaders = csvData[0];

                    csvData.RemoveAt(0);

                }

                else

                {

                    csvHeaders = Enumerable.Range(1, csvData[0].Length)

                        .Select(i => $"Column {i}")

                        .ToArray();

                }



                // Show preview

                DisplayPreview();

                DisplayMapping();



                btnImport.Enabled = true;

                lblStatus.Text = $"Loaded {csvData.Count} rows from CSV (ready to import all rows)";

            }

            catch (Exception ex)

            {

                MessageBox.Show($"Error loading CSV file: {ex.Message}", "Error",

                    MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }



        private string[] ParseCsvLine(string line, char delimiter)

        {

            var values = new List<string>();

            var currentValue = new StringBuilder();

            bool inQuotes = false;



            for (int i = 0; i < line.Length; i++)

            {

                char c = line[i];



                if (c == '"')

                {

                    inQuotes = !inQuotes;

                }

                else if (c == delimiter && !inQuotes)

                {

                    values.Add(currentValue.ToString().Trim());

                    currentValue.Clear();

                }

                else

                {

                    currentValue.Append(c);

                }

            }



            values.Add(currentValue.ToString().Trim());

            return values.ToArray();

        }



        private void DisplayPreview()

        {

            gridPreview.Columns.Clear();

            gridPreview.Rows.Clear();



            foreach (var header in csvHeaders)

            {

                gridPreview.Columns.Add(header, header);

            }



            // Only show first 10 rows in preview (but all data is loaded for import)

            int rowsToShow = Math.Min(10, csvData.Count);

            for (int i = 0; i < rowsToShow; i++)

            {

                gridPreview.Rows.Add(csvData[i]);

            }

        }



        private void DisplayMapping()

        {

            gridMapping.Rows.Clear();



            // Auto-create custom fields for non-standard columns

            var customFieldsToCreate = new List<string>();



            for (int i = 0; i < csvHeaders.Length; i++)

            {

                string sampleData = csvData.Count > 0 && i < csvData[0].Length

                    ? csvData[0][i]

                    : "";



                int rowIndex = gridMapping.Rows.Add(csvHeaders[i], null, sampleData);



                // Auto-map common fields

                string headerLower = csvHeaders[i].ToLower().Trim();

                string headerOriginal = csvHeaders[i].Trim();

                var cell = gridMapping.Rows[rowIndex].Cells["TargetField"] as DataGridViewComboBoxCell;



                // CORREGIDO: Solo mapear a campos standard si NO contiene "sp" o "spouse"

                bool isSpouseField = headerLower.Contains("sp ") || headerLower.Contains("spouse");



                // Smart mapping based on header name (solo para campos del taxpayer principal)

                if (!isSpouseField && (headerLower.Contains("ssn") || headerLower == "ssn"))

                    cell.Value = "SSN";

                else if (!isSpouseField && (headerLower == "nombre" || (headerLower.Contains("name") && !headerLower.Contains("last"))))

                    cell.Value = "Name";

                else if (!isSpouseField && (headerLower.Contains("dob") || headerLower.Contains("birth")))

                    cell.Value = "DOB";

                else if (!isSpouseField && (headerLower.Contains("teléfono") || headerLower.Contains("telefono") || headerLower.Contains("phone")))

                    cell.Value = "Phone";

                else if (!isSpouseField && (headerLower.Contains("correo") || headerLower.Contains("email") || headerLower.Contains("mail")))

                    cell.Value = "Email";

                else

                {

                    // Map to custom field (incluye campos de spouse)

                    string customFieldName = $"Custom: {headerOriginal}";



                    // Check if this custom field exists in the dropdown

                    if (cell.Items.Contains(customFieldName))

                    {

                        cell.Value = customFieldName;

                    }

                    else

                    {

                        // Mark for auto-creation

                        customFieldsToCreate.Add(headerOriginal);

                        cell.Value = "(Skip)";

                    }

                }

            }



            // Auto-create custom fields if needed

            if (customFieldsToCreate.Count > 0)

            {

                AutoCreateMissingCustomFields(customFieldsToCreate);



                // Refresh the mapping grid with new custom fields

                SetupMappingGrid();



                // Re-run mapping with new fields available

                for (int i = 0; i < csvHeaders.Length; i++)

                {

                    string headerLower = csvHeaders[i].ToLower().Trim();

                    string headerOriginal = csvHeaders[i].Trim();



                    // CORREGIDO: Solo skip campos standard del taxpayer principal (sin sp/spouse)

                    bool isSpouseField = headerLower.Contains("sp ") || headerLower.Contains("spouse");



                    // Skip ONLY standard fields del taxpayer principal

                    if (!isSpouseField &&

                        (headerLower.Contains("ssn") ||

                         headerLower == "nombre" ||

                         headerLower.Contains("dob") ||

                         headerLower.Contains("birth") ||

                         headerLower.Contains("teléfono") ||

                         headerLower.Contains("telefono") ||

                         headerLower.Contains("phone") ||

                         headerLower.Contains("correo") ||

                         headerLower.Contains("email") ||

                         headerLower.Contains("mail")))

                        continue;



                    // Map to custom field (incluye TODOS los campos de spouse)

                    var cell = gridMapping.Rows[i].Cells["TargetField"] as DataGridViewComboBoxCell;

                    string customFieldName = $"Custom: {headerOriginal}";



                    if (cell.Items.Contains(customFieldName))

                    {

                        cell.Value = customFieldName;

                    }

                }

            }

        }



        /// <summary>

        /// Smart data cleaning - removes common patterns like year prefixes

        /// </summary>

        private string SmartCleanValue(string value)

        {

            if (string.IsNullOrWhiteSpace(value))

                return value;



            value = value.Trim();



            // Remove year prefixes like "2024 - ", "2023 - ", etc.

            var yearPrefixPattern = new System.Text.RegularExpressions.Regex(@"^\d{4}\s*-\s*");

            value = yearPrefixPattern.Replace(value, "");



            // Convert checkmarks to X

            if (value.Contains("✔") || value.Contains("✓"))

            {

                value = "X";

            }



            return value.Trim();

        }



        /// <summary>

        /// Smart phone cleaning - normalizes phone numbers

        /// </summary>

        private string SmartCleanPhone(string value)

        {

            value = SmartCleanValue(value);

            if (string.IsNullOrWhiteSpace(value))

                return value;



            // Remove common separators for normalization (optional)

            // Keep as-is for now, just clean prefixes

            return value;

        }



        /// <summary>

        /// Smart email cleaning

        /// </summary>

        private string SmartCleanEmail(string value)

        {

            value = SmartCleanValue(value);

            if (string.IsNullOrWhiteSpace(value))

                return value;



            return value.ToLower().Trim();

        }



        /// <summary>

        /// Smart SSN cleaning - removes extra characters but keeps format

        /// </summary>

        private string SmartCleanSSN(string value)

        {

            if (string.IsNullOrWhiteSpace(value))

                return value;



            value = value.Trim();



            // Remove any extra dashes or spaces beyond normal SSN format

            // Keep XXX-XX-XXXX format if present

            return value;

        }



        /// <summary>

        /// Smart date parsing - tries multiple formats

        /// </summary>

        private DateTime? SmartParseDate(string value)

        {

            if (string.IsNullOrWhiteSpace(value))

                return null;



            value = SmartCleanValue(value);



            // Try parsing with multiple formats

            string[] formats = {

                "M/d/yyyy",      // 5/24/2007

                "MM/dd/yyyy",    // 05/24/2007

                "M-d-yyyy",      // 5-24-2007

                "MM-dd-yyyy",    // 05-24-2007

                "yyyy-MM-dd",    // 2007-05-24

                "yyyy/MM/dd",    // 2007/05/24

                "M/d/yy",        // 5/24/07

                "MM/dd/yy"       // 05/24/07

            };



            foreach (var format in formats)

            {

                if (DateTime.TryParseExact(value, format,

                    System.Globalization.CultureInfo.InvariantCulture,

                    System.Globalization.DateTimeStyles.None,

                    out DateTime result))

                {

                    return result;

                }

            }



            // Fallback to default parsing

            if (DateTime.TryParse(value, out DateTime fallbackResult))

            {

                return fallbackResult;

            }



            return null;

        }



        /// <summary>

        /// Auto-creates custom fields if they don't exist

        /// </summary>

        private void AutoCreateMissingCustomFields(List<string> fieldNames)

        {

            var existingFields = customFieldRepository.GetAll();

            var existingFieldNames = existingFields.Select(f => f.FieldName).ToHashSet();



            foreach (var fieldName in fieldNames)

            {

                if (!existingFieldNames.Contains(fieldName))

                {

                    // Create new custom field

                    var newField = new CustomField

                    {

                        FieldName = fieldName,

                        Label = fieldName,

                        FieldType = "text", // Default to text

                        IsRequired = false

                    };



                    try

                    {

                        customFieldRepository.Create(newField);

                        System.Diagnostics.Debug.WriteLine($"Auto-created custom field: {fieldName}");

                    }

                    catch (Exception ex)

                    {

                        System.Diagnostics.Debug.WriteLine($"Could not auto-create field {fieldName}: {ex.Message}");

                    }

                }

            }



            // Reload custom fields after creation

            customFields = customFieldRepository.GetAll();

        }



        private void BtnImport_Click(object sender, EventArgs e)

        {

            // Validate mapping

            var mappings = new Dictionary<string, int>();

            for (int i = 0; i < gridMapping.Rows.Count; i++)

            {

                var targetField = gridMapping.Rows[i].Cells["TargetField"].Value?.ToString();

                if (!string.IsNullOrEmpty(targetField) && targetField != "(Skip)")

                {

                    if (mappings.ContainsKey(targetField))

                    {

                        MessageBox.Show($"Duplicate mapping detected for '{targetField}'. Each field can only be mapped once.",

                            "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        return;

                    }

                    mappings[targetField] = i;

                }

            }



            // Check for required SSN

            if (!mappings.ContainsKey("SSN"))

            {

                var result = MessageBox.Show("SSN is not mapped. Records without SSN cannot be imported.\n\nContinue anyway?",

                    "Missing SSN Mapping", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.No)

                    return;

            }



            // Confirm import with actual row count

            var confirmResult = MessageBox.Show(

                $"Import {csvData.Count} records from CSV?\n\nThis operation may take several minutes for large files.",

                "Confirm Import",

                MessageBoxButtons.YesNo,

                MessageBoxIcon.Question);



            if (confirmResult == DialogResult.Yes)

            {

                PerformImport(mappings);

            }

        }



        private void PerformImport(Dictionary<string, int> mappings)
        {
            progressBar.Visible = true;
            progressBar.Maximum = csvData.Count;
            progressBar.Value = 0;
            btnImport.Enabled = false;
            btnBrowse.Enabled = false;

            int created = 0;
            int updated = 0;
            int skipped = 0;
            int errors = 0;
            var errorMessages = new List<string>();

            // Track start time for performance info
            var startTime = DateTime.Now;

            // Log import start
            DebugConsole.Info($"Starting OPTIMIZED CSV import: {csvData.Count} rows to process");
            DebugConsole.Info($"File: {System.IO.Path.GetFileName(txtFilePath.Text)}");

            try
            {
                // OPTIMIZATION 1: Load ALL existing clients by SSN in ONE query
                DebugConsole.Info("Loading existing clients into memory...");
                var existingClients = clientRepository.GetAllClients();
                var existingClientsBySSN = existingClients.ToDictionary(c => c.SSN ?? "", c => c);
                DebugConsole.Success($"Loaded {existingClients.Count} existing clients");

                int userId = AuthService.Instance.GetCurrentUserId();

                // OPTIMIZATION 2: Process in batches to avoid blocking the database
                const int BATCH_SIZE = 100; // Commit every 100 records
                using (var connection = Database.DbConnection.GetConnection())
                {
                    connection.Open();

                    SqliteTransaction transaction = null;

                    for (int i = 0; i < csvData.Count; i++)
                    {
                        // Start new transaction at batch boundaries
                        if (i % BATCH_SIZE == 0)
                        {
                            transaction = connection.BeginTransaction();
                            DebugConsole.Info($"Starting batch {(i / BATCH_SIZE) + 1} (records {i + 1}-{Math.Min(i + BATCH_SIZE, csvData.Count)})");
                        }

                        try
                        {
                            var row = csvData[i];

                            // ===== PASO 1: Extraer SSN del CSV PRIMERO =====
                            string ssnFromCsv = null;
                            if (mappings.TryGetValue("SSN", out int ssnColumnIndex))
                            {
                                if (ssnColumnIndex < row.Length)
                                {
                                    ssnFromCsv = SmartCleanSSN(row[ssnColumnIndex]);
                                }
                            }

                            // Validar SSN
                            if (string.IsNullOrWhiteSpace(ssnFromCsv))
                            {
                                skipped++;
                                continue;
                            }

                            // ===== PASO 2: Determinar si cliente existe y preparar objeto =====
                            Client client;
                            bool isUpdate = false;

                            if (existingClientsBySSN.TryGetValue(ssnFromCsv, out var existing))
                            {
                                // Cliente EXISTE - Copiar ExtraData existente para preservarlo
                                client = new Client();
                                client.Id = existing.Id;
                                client.SSN = existing.SSN;
                                client.Name = existing.Name;
                                client.DOB = existing.DOB;
                                client.Phone = existing.Phone;
                                client.Email = existing.Email;
                                client.Notes = existing.Notes;

                                // CRÍTICO: Copiar ExtraData existente (clonar el diccionario)
                                if (existing.ExtraData != null)
                                {
                                    client.ExtraData = new Dictionary<string, object>(existing.ExtraData);
                                }
                                else
                                {
                                    client.ExtraData = new Dictionary<string, object>();
                                }

                                isUpdate = true;
                            }
                            else
                            {
                                // Cliente NUEVO
                                client = new Client();
                                client.SSN = ssnFromCsv;
                                client.ExtraData = new Dictionary<string, object>();
                            }

                            // ===== PASO 3: Mapear campos del CSV (sobrescribe solo los mapeados) =====
                            foreach (var mapping in mappings)
                            {
                                if (mapping.Value >= row.Length)
                                    continue;

                                string value = row[mapping.Value];
                                if (string.IsNullOrWhiteSpace(value))
                                    continue;

                                switch (mapping.Key)
                                {
                                    case "SSN":
                                        // Ya se asignó arriba
                                        break;
                                    case "Name":
                                        client.Name = SmartCleanValue(value);
                                        break;
                                    case "Phone":
                                        client.Phone = SmartCleanPhone(value);
                                        break;
                                    case "Email":
                                        client.Email = SmartCleanEmail(value);
                                        break;
                                    case "DOB":
                                        var parsedDate = SmartParseDate(value);
                                        if (parsedDate.HasValue)
                                            client.DOB = parsedDate.Value;
                                        break;
                                    default:
                                        if (mapping.Key.StartsWith("Custom: "))
                                        {
                                            string fieldName = mapping.Key.Substring(8);
                                            // Apply special mapping for specific fields
                                            string cleanedValue = SmartCleanValue(value);
                                            cleanedValue = ApplySpecialMapping(fieldName, cleanedValue);

                                            // Actualiza o agrega el campo en ExtraData
                                            client.SetExtraDataValue(fieldName, cleanedValue);
                                        }
                                        break;
                                }
                            }

                            // Validar nombre
                            if (string.IsNullOrWhiteSpace(client.Name))
                            {
                                client.Name = "Unknown";
                            }

                            // Serialize ExtraData once for both INSERT and UPDATE
                            string extraDataJson = JsonConvert.SerializeObject(
                                client.ExtraData ?? new Dictionary<string, object>(),
                                new JsonSerializerSettings
                                {
                                    NullValueHandling = NullValueHandling.Include,
                                    DefaultValueHandling = DefaultValueHandling.Include
                                });

                            // ===== PASO 4: Guardar en base de datos =====
                            if (isUpdate)
                            {
                                // UPDATE existing client - Direct SQL
                                string updateSql = @"
                            UPDATE Clients
                            SET Name = @name,
                                DOB = @dob,
                                Phone = @phone,
                                Email = @email,
                                Notes = @notes,
                                ExtraData = @extraData,
                                UpdatedBy = @userId,
                                LastUpdated = CURRENT_TIMESTAMP
                            WHERE Id = @id";

                                using var updateCmd = new SqliteCommand(updateSql, connection, transaction);
                                updateCmd.Parameters.AddWithValue("@name", client.Name);
                                updateCmd.Parameters.AddWithValue("@dob", client.DOB?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
                                updateCmd.Parameters.AddWithValue("@phone", client.Phone ?? (object)DBNull.Value);
                                updateCmd.Parameters.AddWithValue("@email", client.Email ?? (object)DBNull.Value);
                                updateCmd.Parameters.AddWithValue("@notes", client.Notes ?? (object)DBNull.Value);
                                updateCmd.Parameters.AddWithValue("@extraData", extraDataJson);
                                updateCmd.Parameters.AddWithValue("@userId", userId);
                                updateCmd.Parameters.AddWithValue("@id", client.Id);
                                updateCmd.ExecuteNonQuery();
                                updated++;
                            }
                            else
                            {
                                // CREATE new client - Direct SQL
                                string insertSql = @"
                            INSERT INTO Clients (SSN, Name, DOB, Phone, Email, Notes, ExtraData, CreatedBy, UpdatedBy)
                            VALUES (@ssn, @name, @dob, @phone, @email, @notes, @extraData, @userId, @userId);
                            SELECT last_insert_rowid();";

                                using var insertCmd = new SqliteCommand(insertSql, connection, transaction);
                                insertCmd.Parameters.AddWithValue("@ssn", client.SSN);
                                insertCmd.Parameters.AddWithValue("@name", client.Name);
                                insertCmd.Parameters.AddWithValue("@dob", client.DOB?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
                                insertCmd.Parameters.AddWithValue("@phone", client.Phone ?? (object)DBNull.Value);
                                insertCmd.Parameters.AddWithValue("@email", client.Email ?? (object)DBNull.Value);
                                insertCmd.Parameters.AddWithValue("@notes", client.Notes ?? (object)DBNull.Value);
                                insertCmd.Parameters.AddWithValue("@extraData", extraDataJson);
                                insertCmd.Parameters.AddWithValue("@userId", userId);

                                int clientId = Convert.ToInt32(insertCmd.ExecuteScalar());
                                created++;
                            }
                        }
                        catch (Exception ex)
                        {
                            errors++;
                            if (errorMessages.Count < 10)
                            {
                                errorMessages.Add($"Row {i + 1}: {ex.Message}");
                                DebugConsole.Error($"Import error at row {i + 1}: {ex.Message}");
                            }
                        }

                        progressBar.Value = i + 1;

                        // Commit transaction at batch boundaries
                        if ((i + 1) % BATCH_SIZE == 0 || i == csvData.Count - 1)
                        {
                            if (transaction != null)
                            {
                                transaction.Commit();
                                transaction.Dispose();
                                transaction = null;
                                DebugConsole.Success($"Batch committed. Total: {i + 1}/{csvData.Count} (Created: {created}, Updated: {updated}, Errors: {errors})");
                            }
                        }

                        // OPTIMIZATION 5: Update UI only every 500 rows
                        if (i % 500 == 0 || i == csvData.Count - 1)
                        {
                            lblStatus.Text = $"Processing... {i + 1}/{csvData.Count} ({created} created, {updated} updated)";
                            Application.DoEvents();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DebugConsole.Error($"Critical import error: {ex.Message}");
                DebugConsole.Error($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Critical error during import: {ex.Message}", "Import Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var elapsed = DateTime.Now - startTime;

            // Log import completion
            DebugConsole.Success($"Import completed in {elapsed.TotalSeconds:F1} seconds");
            DebugConsole.Success($"Results: {created} created, {updated} updated, {skipped} skipped, {errors} errors");
            DebugConsole.Success($"Speed: {(csvData.Count / elapsed.TotalSeconds):F0} records/second");

            // Log import
            AuditService.LogImport(
                System.IO.Path.GetFileName(txtFilePath.Text),
                csvData.Count,
                created,
                updated,
                errors);

            // Show results
            string message = $"Import completed in {elapsed.TotalSeconds:F1} seconds:\n\n" +
                           $"Total rows processed: {csvData.Count}\n" +
                           $"Created: {created}\n" +
                           $"Updated: {updated}\n" +
                           $"Skipped: {skipped}\n" +
                           $"Errors: {errors}\n\n" +
                           $"Speed: {(csvData.Count / elapsed.TotalSeconds):F0} records/second";

            if (errorMessages.Count > 0)
            {
                message += "\n\nFirst errors:\n" + string.Join("\n", errorMessages);
            }

            MessageBox.Show(message, "Import Complete",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            progressBar.Visible = false;
            btnImport.Enabled = true;
            btnBrowse.Enabled = true;
            lblStatus.Text = $"Import completed: {created} created, {updated} updated, {errors} errors";
        }

        private string ApplySpecialMapping(string fieldName, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;

            string normalizedField = fieldName.Trim().ToLower();

            // Rfd Type Mapping
            if (normalizedField == "rfd type" || normalizedField == "refund type")
            {
                switch (value.Trim())
                {
                    case "1": return "1. Check from IRS";
                    case "2": return "2. DD from IRS";
                    case "3": return "3. RESERVED";
                    case "4": return "4. Baldue";
                    case "5": return "5. RT-BANK";
                }
            }
            // Type (Filing Status) Mapping
            else if (normalizedField == "type" || normalizedField == "filing status")
            {
                switch (value.Trim())
                {
                    case "1": return "1. Single";
                    case "2": return "2. MFJ";
                    case "3": return "3. MFS";
                    case "4": return "4. Head of Household";
                    case "5": return "5. Qual Surviving Spouse";
                }
            }

            return value;
        }
    }
}
