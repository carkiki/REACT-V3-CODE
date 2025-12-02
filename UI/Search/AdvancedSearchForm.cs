using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using ReactCRM.Database;
using ReactCRM.Models;
using ReactCRM.Services;

namespace ReactCRM.UI.Search
{
    public partial class AdvancedSearchForm : Form
    {
        private Panel panelCriteria;
        private DataGridView gridResults;
        private Button btnAddCriteria;
        private Button btnSearch;
        private Button btnClear;
        private Button btnExport;
        private Button btnClose;
        private Label lblResultCount;
        private List<SearchCriteriaControl> criteriaControls;
        private ClientRepository clientRepository;
        private CustomFieldRepository customFieldRepository;

        public AdvancedSearchForm()
        {
            clientRepository = new ClientRepository();
            customFieldRepository = new CustomFieldRepository();
            criteriaControls = new List<SearchCriteriaControl>();

            InitializeComponent();
            AddCriteriaControl();
        }

        private void InitializeComponent()
        {
            this.Text = "Advanced Client Search";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(900, 600);

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
                Text = "Advanced Search",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 10),
                AutoSize = true
            };

            var lblSubtitle = new Label
            {
                Text = "Build complex queries to find clients",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(189, 195, 199),
                Location = new Point(15, 40),
                AutoSize = true
            };

            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(lblSubtitle);

            // Criteria Panel
            panelCriteria = new Panel
            {
                Dock = DockStyle.Top,
                Height = 250,
                AutoScroll = true,
                BackColor = Color.FromArgb(236, 240, 241),
                Padding = new Padding(10)
            };

            // Criteria Buttons Panel
            var panelCriteriaButtons = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(236, 240, 241),
                Padding = new Padding(10)
            };

            btnAddCriteria = new Button
            {
                Text = "+ Add Criteria",
                Location = new Point(10, 10),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnAddCriteria.Click += (s, e) => AddCriteriaControl();

            btnSearch = new Button
            {
                Text = "Search",
                Location = new Point(140, 10),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSearch.Click += BtnSearch_Click;

            btnClear = new Button
            {
                Text = "Clear All",
                Location = new Point(270, 10),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(127, 140, 141),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnClear.Click += BtnClear_Click;

            panelCriteriaButtons.Controls.Add(btnAddCriteria);
            panelCriteriaButtons.Controls.Add(btnSearch);
            panelCriteriaButtons.Controls.Add(btnClear);

            // Results Panel
            var panelResults = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var lblResults = new Label
            {
                Text = "Search Results:",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Height = 25
            };

            gridResults = new DataGridView
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

            panelResults.Controls.Add(gridResults);
            panelResults.Controls.Add(lblResults);

            // Bottom Panel
            var panelBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(236, 240, 241),
                Padding = new Padding(10)
            };

            lblResultCount = new Label
            {
                Text = "No results",
                Location = new Point(10, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            btnExport = new Button
            {
                Text = "Export to CSV",
                Location = new Point(800, 12),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(241, 196, 15),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnExport.Click += BtnExport_Click;

            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(930, 12),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(127, 140, 141),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnClose.Click += (s, e) => this.Close();

            panelBottom.Controls.Add(lblResultCount);
            panelBottom.Controls.Add(btnExport);
            panelBottom.Controls.Add(btnClose);

            // Add controls to form
            this.Controls.Add(panelResults);
            this.Controls.Add(panelCriteria);
            this.Controls.Add(panelCriteriaButtons);
            this.Controls.Add(panelHeader);
            this.Controls.Add(panelBottom);
        }

        private void AddCriteriaControl()
        {
            var criteriaControl = new SearchCriteriaControl(customFieldRepository);
            criteriaControl.Location = new Point(10, criteriaControls.Count * 45 + 10);
            criteriaControl.OnRemoveClicked += RemoveCriteriaControl;

            criteriaControls.Add(criteriaControl);
            panelCriteria.Controls.Add(criteriaControl);
        }

        private void RemoveCriteriaControl(SearchCriteriaControl control)
        {
            criteriaControls.Remove(control);
            panelCriteria.Controls.Remove(control);
            control.Dispose();

            // Reposition remaining controls
            for (int i = 0; i < criteriaControls.Count; i++)
            {
                criteriaControls[i].Location = new Point(10, i * 45 + 10);
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            foreach (var control in criteriaControls.ToList())
            {
                RemoveCriteriaControl(control);
            }
            AddCriteriaControl();
            gridResults.DataSource = null;
            lblResultCount.Text = "No results";
            btnExport.Enabled = false;
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                var results = PerformSearch();
                DisplayResults(results);

                AuditService.LogAction("Search", "Clients", null,
                    $"Advanced search performed - {results.Count} results");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error performing search: {ex.Message}", "Search Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<Client> PerformSearch()
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();

                var whereClauses = new List<string>();
                var parameters = new List<SqliteParameter>();

                int paramIndex = 0;
                foreach (var criteriaControl in criteriaControls)
                {
                    var criteria = criteriaControl.GetCriteria();
                    if (criteria == null) continue;

                    string clause = BuildWhereClause(criteria, ref paramIndex, parameters);
                    if (!string.IsNullOrEmpty(clause))
                    {
                        whereClauses.Add(clause);
                    }
                }

                string whereClause = whereClauses.Count > 0
                    ? "WHERE " + string.Join(" AND ", whereClauses)
                    : "";

                string sql = $@"
                    SELECT c.*, w.Username as CreatedByName, w2.Username as UpdatedByName
                    FROM Clients c
                    LEFT JOIN Workers w ON c.CreatedBy = w.Id
                    LEFT JOIN Workers w2 ON c.UpdatedBy = w2.Id
                    {whereClause}
                    ORDER BY c.Name";

                using (var cmd = new SqliteCommand(sql, conn))
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.Add(param);
                    }

                    var clients = new List<Client>();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            clients.Add(MapReaderToClient(reader));
                        }
                    }

                    return clients;
                }
            }
        }

        private string BuildWhereClause(SearchCriteria criteria, ref int paramIndex, List<SqliteParameter> parameters)
        {
            string paramName = $"@param{paramIndex++}";

            switch (criteria.Field)
            {
                case "SSN":
                case "Name":
                case "Phone":
                case "Email":
                    return BuildStandardFieldClause(criteria, paramName, parameters);

                case "DOB":
                    return BuildDateFieldClause(criteria, paramName, parameters);

                default:
                    if (criteria.Field.StartsWith("Custom: "))
                    {
                        return BuildCustomFieldClause(criteria, paramName, parameters);
                    }
                    return null;
            }
        }

        private string BuildStandardFieldClause(SearchCriteria criteria, string paramName, List<SqliteParameter> parameters)
        {
            switch (criteria.Operator)
            {
                case "Equals":
                    parameters.Add(new SqliteParameter(paramName, criteria.Value));
                    return $"{criteria.Field} = {paramName}";

                case "Contains":
                    parameters.Add(new SqliteParameter(paramName, $"%{criteria.Value}%"));
                    return $"{criteria.Field} LIKE {paramName}";

                case "Starts With":
                    parameters.Add(new SqliteParameter(paramName, $"{criteria.Value}%"));
                    return $"{criteria.Field} LIKE {paramName}";

                case "Ends With":
                    parameters.Add(new SqliteParameter(paramName, $"%{criteria.Value}"));
                    return $"{criteria.Field} LIKE {paramName}";

                case "Not Equals":
                    parameters.Add(new SqliteParameter(paramName, criteria.Value));
                    return $"{criteria.Field} != {paramName}";

                case "Is Empty":
                    return $"({criteria.Field} IS NULL OR {criteria.Field} = '')";

                case "Is Not Empty":
                    return $"({criteria.Field} IS NOT NULL AND {criteria.Field} != '')";

                default:
                    return null;
            }
        }

        private string BuildDateFieldClause(SearchCriteria criteria, string paramName, List<SqliteParameter> parameters)
        {
            if (DateTime.TryParse(criteria.Value, out DateTime dateValue))
            {
                string dateStr = dateValue.ToString("yyyy-MM-dd");

                switch (criteria.Operator)
                {
                    case "Equals":
                        parameters.Add(new SqliteParameter(paramName, dateStr));
                        return $"date(DOB) = {paramName}";

                    case "Before":
                        parameters.Add(new SqliteParameter(paramName, dateStr));
                        return $"date(DOB) < {paramName}";

                    case "After":
                        parameters.Add(new SqliteParameter(paramName, dateStr));
                        return $"date(DOB) > {paramName}";

                    case "Is Empty":
                        return "DOB IS NULL";

                    case "Is Not Empty":
                        return "DOB IS NOT NULL";
                }
            }

            return null;
        }

        private string BuildCustomFieldClause(SearchCriteria criteria, string paramName, List<SqliteParameter> parameters)
        {
            string fieldName = criteria.Field.Substring(8); // Remove "Custom: "
            string jsonPath = $"$.{fieldName}";

            switch (criteria.Operator)
            {
                case "Contains":
                    parameters.Add(new SqliteParameter(paramName, $"%{criteria.Value}%"));
                    return $"json_extract(ExtraData, '{jsonPath}') LIKE {paramName}";

                default:
                    parameters.Add(new SqliteParameter(paramName, criteria.Value));
                    return $"json_extract(ExtraData, '{jsonPath}') = {paramName}";
            }
        }

        private Client MapReaderToClient(SqliteDataReader reader)
        {
            var client = new Client
            {
                Id = Convert.ToInt32(reader["Id"]),
                SSN = reader["SSN"].ToString(),
                Name = reader["Name"].ToString(),
                Phone = reader["Phone"]?.ToString(),
                Email = reader["Email"]?.ToString(),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                LastUpdated = Convert.ToDateTime(reader["LastUpdated"])
            };

            if (reader["DOB"] != DBNull.Value)
            {
                client.DOB = Convert.ToDateTime(reader["DOB"]);
            }

            if (reader["ExtraData"] != DBNull.Value)
            {
                string extraDataJson = reader["ExtraData"].ToString();
                client.ExtraData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(extraDataJson);
            }

            return client;
        }

        private void DisplayResults(List<Client> clients)
        {
            gridResults.DataSource = null;
            gridResults.Columns.Clear();

            var displayData = clients.Select(c => new
            {
                c.Id,
                c.SSN,
                c.Name,
                DOB = c.DOB?.ToString("yyyy-MM-dd") ?? "",
                c.Phone,
                c.Email,
                CreatedAt = c.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            }).ToList();

            gridResults.DataSource = displayData;
            lblResultCount.Text = $"Found {clients.Count} client(s)";
            btnExport.Enabled = clients.Count > 0;
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            // Simple export functionality
            MessageBox.Show("Export functionality would be implemented here.", "Export",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    // Search Criteria Control
    public class SearchCriteriaControl : Panel
    {
        private ComboBox cboField;
        private ComboBox cboOperator;
        private TextBox txtValue;
        private Button btnRemove;
        private CustomFieldRepository customFieldRepository;

        public event Action<SearchCriteriaControl> OnRemoveClicked;

        public SearchCriteriaControl(CustomFieldRepository customFieldRepo)
        {
            customFieldRepository = customFieldRepo;
            this.Size = new Size(850, 40);
            this.BackColor = Color.White;
            this.BorderStyle = BorderStyle.FixedSingle;

            InitializeControls();
        }

        private void InitializeControls()
        {
            // Field ComboBox
            cboField = new ComboBox
            {
                Location = new Point(5, 8),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };

            var fields = new List<string> { "SSN", "Name", "DOB", "Phone", "Email" };
            var customFields = customFieldRepository.GetAll();
            foreach (var field in customFields)
            {
                fields.Add($"Custom: {field.Label}");
            }
            cboField.Items.AddRange(fields.ToArray());
            cboField.SelectedIndex = 0;
            cboField.SelectedIndexChanged += (s, e) => UpdateOperators();

            // Operator ComboBox
            cboOperator = new ComboBox
            {
                Location = new Point(215, 8),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            UpdateOperators();

            // Value TextBox
            txtValue = new TextBox
            {
                Location = new Point(375, 8),
                Width = 400,
                Font = new Font("Segoe UI", 9)
            };

            // Remove Button
            btnRemove = new Button
            {
                Text = "X",
                Location = new Point(785, 7),
                Size = new Size(30, 25),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRemove.Click += (s, e) => OnRemoveClicked?.Invoke(this);

            this.Controls.Add(cboField);
            this.Controls.Add(cboOperator);
            this.Controls.Add(txtValue);
            this.Controls.Add(btnRemove);
        }

        private void UpdateOperators()
        {
            cboOperator.Items.Clear();

            string field = cboField.SelectedItem?.ToString();
            if (field == "DOB")
            {
                cboOperator.Items.AddRange(new object[] {
                    "Equals", "Before", "After", "Is Empty", "Is Not Empty"
                });
            }
            else
            {
                cboOperator.Items.AddRange(new object[] {
                    "Equals", "Contains", "Starts With", "Ends With", "Not Equals", "Is Empty", "Is Not Empty"
                });
            }

            cboOperator.SelectedIndex = 0;
        }

        public SearchCriteria GetCriteria()
        {
            if (cboField.SelectedItem == null || cboOperator.SelectedItem == null)
                return null;

            string operatorValue = cboOperator.SelectedItem.ToString();
            if (operatorValue != "Is Empty" && operatorValue != "Is Not Empty" && string.IsNullOrWhiteSpace(txtValue.Text))
                return null;

            return new SearchCriteria
            {
                Field = cboField.SelectedItem.ToString(),
                Operator = operatorValue,
                Value = txtValue.Text.Trim()
            };
        }
    }

    // Search Criteria Model
    public class SearchCriteria
    {
        public string Field { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
    }
}