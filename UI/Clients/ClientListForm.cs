using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using ReactCRM.Database;
using ReactCRM.Models;
using ReactCRM.Services;
using ReactCRM.UI.Components;
using ReactCRM.UI.Tasks;
using Newtonsoft.Json;

namespace ReactCRM.UI.Clients
{
    public partial class ClientListForm : Form
    {
        private DataGridView gridClients;
        private TextBox txtSearch;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnRefresh;
        private Button btnClose;
        private Label lblTotal;
        private ClientRepository repository;
        private CustomFieldRepository customFieldRepository;
        private List<CustomField> customFields;

        // ===== CACHE EN MEMORIA =====
        private static List<Client> cachedClients = null;
        private static DateTime? cacheLoadedAt = null;
        private static readonly object cacheLock = new object();

        // ===== CONFIGURACIÓN DE COLUMNAS =====
        private static readonly string ColumnOrderConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ReactCRM", "columnorder.config.json");

        public ClientListForm()
        {
            repository = new ClientRepository();
            customFieldRepository = new CustomFieldRepository();
            customFields = customFieldRepository.GetAll() ?? new List<CustomField>();

            // Pre-cargar caché en background si no está cargado
            if (cachedClients == null)
            {
                System.Diagnostics.Debug.WriteLine("Pre-loading cache in background...");
                _ = Task.Run(() => PreLoadCache());
            }

            InitializeComponent();
            _ = LoadClientsAsync();
        }

        private void PreLoadCache()
        {
            try
            {
                lock (cacheLock)
                {
                    if (cachedClients == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Loading cache in background...");
                        cachedClients = repository.GetAllClients();
                        cacheLoadedAt = DateTime.Now;
                        System.Diagnostics.Debug.WriteLine($"Background cache loaded: {cachedClients.Count} clients");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error pre-loading cache: {ex.Message}");
            }
        }

        public static void InvalidateCache()
        {
            lock (cacheLock)
            {
                cachedClients = null;
                cacheLoadedAt = null;
            }
        }

        private string GetAgeDisplay(DateTime? dob)
        {
            if (!dob.HasValue)
                return "N/A";

            var today = DateTime.Today;
            int age = today.Year - dob.Value.Year;

            if (dob.Value.Date > today.AddYears(-age))
                age--;

            return $"{age} years";
        }

        private void InitializeComponent()
        {
            this.Text = "Client Management";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(800, 500);

            // Search Panel
            var panelSearch = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(236, 240, 241),
                Padding = new Padding(10)
            };

            var lblSearch = new Label
            {
                Text = "Search:",
                Location = new Point(10, 18),
                AutoSize = true,
                Font = new Font("Segoe UI", 10)
            };

            txtSearch = new TextBox
            {
                Location = new Point(70, 15),
                Width = 300,
                Font = new Font("Segoe UI", 10)
            };
            txtSearch.TextChanged += (s, e) => FilterClients();

            btnAdd = new Button
            {
                Text = "Add Client",
                Location = new Point(400, 12),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnAdd.Click += BtnAdd_Click;

            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(510, 12),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnRefresh.Click += async (s, e) => await LoadClientsAsync(forceReload: true);

            lblTotal = new Label
            {
                Text = "Total: 0",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(650, 18)
            };

            panelSearch.Controls.Add(lblSearch);
            panelSearch.Controls.Add(txtSearch);
            panelSearch.Controls.Add(btnAdd);
            panelSearch.Controls.Add(btnRefresh);
            panelSearch.Controls.Add(lblTotal);

            // ===== DataGridView con scroll horizontal y reordenamiento =====
            gridClients = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None, // Permitir scroll horizontal
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToOrderColumns = true, // Permitir reordenar columnas
                ScrollBars = ScrollBars.Both, // Scroll horizontal y vertical
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9)
            };
            gridClients.DoubleClick += BtnEdit_Click;
            gridClients.ColumnDisplayIndexChanged += GridClients_ColumnDisplayIndexChanged;

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

            btnDelete = new Button
            {
                Text = "Delete",
                Location = new Point(120, 12),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnDelete.Click += BtnDelete_Click;

            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(230, 12),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(127, 140, 141),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnClose.Click += (s, e) => this.Close();

            panelBottom.Controls.Add(btnEdit);
            panelBottom.Controls.Add(btnDelete);
            panelBottom.Controls.Add(btnClose);

            this.Controls.Add(gridClients);
            this.Controls.Add(panelSearch);
            this.Controls.Add(panelBottom);

            if (!AuthService.Instance.CanEditClients())
            {
                btnAdd.Enabled = false;
                btnEdit.Enabled = false;
                btnDelete.Enabled = false;
            }
        }

        private async Task LoadClientsAsync(bool forceReload = false)
        {
            try
            {
                lblTotal.Text = "Loading...";
                gridClients.Enabled = false;
                btnRefresh.Enabled = false;

                var clients = await Task.Run(() =>
                {
                    lock (cacheLock)
                    {
                        if (cachedClients != null && !forceReload)
                        {
                            System.Diagnostics.Debug.WriteLine($"Using cached clients ({cachedClients.Count} clients)");
                            return cachedClients;
                        }

                        System.Diagnostics.Debug.WriteLine("Loading clients from database...");
                        customFields = customFieldRepository.GetAll() ?? new List<CustomField>();
                        cachedClients = repository.GetAllClients();
                        cacheLoadedAt = DateTime.Now;
                        System.Diagnostics.Debug.WriteLine($"Loaded {cachedClients.Count} clients");
                        return cachedClients;
                    }
                });

                gridClients.DataSource = null;
                gridClients.Columns.Clear();

                gridClients.Columns.Add("Id", "ID");
                gridClients.Columns.Add("SSN", "SSN");
                gridClients.Columns.Add("Name", "Name");
                gridClients.Columns.Add("Age", "Age");
                gridClients.Columns.Add("Phone", "Phone");
                gridClients.Columns.Add("Email", "Email");

                foreach (var field in customFields)
                {
                    gridClients.Columns.Add(field.FieldName, field.Label);
                }

                gridClients.Columns.Add("CreatedAt", "Created At");
                gridClients.Columns.Add("LastUpdated", "Last Updated");

                var rowData = new System.Collections.Concurrent.ConcurrentBag<object[]>();

                System.Threading.Tasks.Parallel.ForEach(clients, new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                },
                client =>
                {
                    try
                    {
                        var row = new List<object>
                        {
                            client.Id,
                            client.SSN ?? "",
                            client.Name ?? "",
                            GetAgeDisplay(client.DOB),
                            client.Phone ?? "",
                            client.Email ?? ""
                        };

                        foreach (var field in customFields)
                        {
                            var value = client.GetExtraDataValue(field.FieldName) ?? "";
                            row.Add(value);
                        }

                        row.Add(client.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
                        row.Add(client.LastUpdated.ToString("yyyy-MM-dd HH:mm"));

                        rowData.Add(row.ToArray());
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading client {client?.Id}: {ex.Message}");
                    }
                });

                foreach (var row in rowData)
                {
                    gridClients.Rows.Add(row);
                }

                // Anchos fijos para scroll horizontal
                gridClients.Columns["Id"].Width = 50;
                gridClients.Columns["SSN"].Width = 120;
                gridClients.Columns["Name"].Width = 200;
                gridClients.Columns["Age"].Width = 100;
                gridClients.Columns["Phone"].Width = 120;
                gridClients.Columns["Email"].Width = 200;

                foreach (var field in customFields)
                {
                    if (gridClients.Columns.Contains(field.FieldName))
                    {
                        gridClients.Columns[field.FieldName].Width = 150;
                    }
                }

                gridClients.Columns["CreatedAt"].Width = 140;
                gridClients.Columns["LastUpdated"].Width = 140;

                lblTotal.Text = $"Total: {clients.Count}";

                // Cargar orden de columnas guardado
                LoadColumnOrder();

                AuditService.LogAction("View", "ClientList", null,
                    $"Viewed client list ({clients.Count} clients)");
            }
            catch (Exception ex)
            {
                lblTotal.Text = "Error loading";
                MessageBox.Show($"Error loading clients: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                gridClients.Enabled = true;
                btnRefresh.Enabled = true;
            }
        }

        private void FilterClients()
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                _ = LoadClientsAsync();
                return;
            }

            try
            {
                if (cachedClients == null)
                {
                    _ = LoadClientsAsync();
                    return;
                }

                string searchTerm = txtSearch.Text.ToLower();
                var clients = cachedClients.AsParallel()
                    .WithDegreeOfParallelism(Environment.ProcessorCount)
                    .Where(c =>
                        (c.SSN?.ToLower().Contains(searchTerm) ?? false) ||
                        (c.Name?.ToLower().Contains(searchTerm) ?? false) ||
                        (c.Phone?.ToLower().Contains(searchTerm) ?? false) ||
                        (c.Email?.ToLower().Contains(searchTerm) ?? false) ||
                        (c.ExtraData != null && c.ExtraData.Any(kvp =>
                            kvp.Value?.ToString()?.ToLower().Contains(searchTerm) ?? false))
                    ).ToList();

                gridClients.DataSource = null;
                gridClients.Columns.Clear();

                gridClients.Columns.Add("Id", "ID");
                gridClients.Columns.Add("SSN", "SSN");
                gridClients.Columns.Add("Name", "Name");
                gridClients.Columns.Add("Age", "Age");
                gridClients.Columns.Add("Phone", "Phone");
                gridClients.Columns.Add("Email", "Email");

                foreach (var field in customFields)
                {
                    gridClients.Columns.Add(field.FieldName, field.Label);
                }

                gridClients.Columns.Add("CreatedAt", "Created At");
                gridClients.Columns.Add("LastUpdated", "Last Updated");

                var rowData = new System.Collections.Concurrent.ConcurrentBag<object[]>();

                System.Threading.Tasks.Parallel.ForEach(clients, new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                },
                client =>
                {
                    var row = new List<object>
                    {
                        client.Id,
                        client.SSN ?? "",
                        client.Name ?? "",
                        GetAgeDisplay(client.DOB),
                        client.Phone ?? "",
                        client.Email ?? ""
                    };

                    foreach (var field in customFields)
                    {
                        var value = client.GetExtraDataValue(field.FieldName) ?? "";
                        row.Add(value);
                    }

                    row.Add(client.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
                    row.Add(client.LastUpdated.ToString("yyyy-MM-dd HH:mm"));

                    rowData.Add(row.ToArray());
                });

                foreach (var row in rowData)
                {
                    gridClients.Rows.Add(row);
                }

                gridClients.Columns["Id"].Width = 50;
                gridClients.Columns["SSN"].Width = 120;
                gridClients.Columns["Name"].Width = 200;
                gridClients.Columns["Age"].Width = 100;
                gridClients.Columns["Phone"].Width = 120;
                gridClients.Columns["Email"].Width = 200;

                foreach (var field in customFields)
                {
                    if (gridClients.Columns.Contains(field.FieldName))
                    {
                        gridClients.Columns[field.FieldName].Width = 150;
                    }
                }

                gridClients.Columns["CreatedAt"].Width = 140;
                gridClients.Columns["LastUpdated"].Width = 140;

                lblTotal.Text = $"Total: {clients.Count}";

                // Cargar orden de columnas guardado
                LoadColumnOrder();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching clients: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!AuthService.Instance.CanEditClients())
            {
                MessageBox.Show("You don't have permission to add clients.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var form = new ClientEditForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                // Guardar posición del scroll
                int scrollPosition = gridClients.FirstDisplayedScrollingRowIndex;
                int selectedRowIndex = gridClients.SelectedRows.Count > 0 ? gridClients.SelectedRows[0].Index : -1;

                InvalidateCache();
                _ = LoadClientsAsync().ContinueWith(t =>
                {
                    // Restaurar posición del scroll
                    this.Invoke((MethodInvoker)delegate
                    {
                        if (scrollPosition >= 0 && scrollPosition < gridClients.Rows.Count)
                        {
                            gridClients.FirstDisplayedScrollingRowIndex = scrollPosition;
                        }
                        if (selectedRowIndex >= 0 && selectedRowIndex < gridClients.Rows.Count)
                        {
                            gridClients.Rows[selectedRowIndex].Selected = true;
                        }
                    });
                });
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (!AuthService.Instance.CanEditClients())
            {
                MessageBox.Show("You don't have permission to edit clients.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (gridClients.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a client to edit.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int clientId = (int)gridClients.SelectedRows[0].Cells["Id"].Value;
            var client = repository.GetClientById(clientId);

            if (client != null)
            {
                // Guardar posición del scroll
                int scrollPosition = gridClients.FirstDisplayedScrollingRowIndex;
                int selectedRowIndex = gridClients.SelectedRows[0].Index;

                var form = new ClientEditForm(client);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    InvalidateCache();
                    _ = LoadClientsAsync().ContinueWith(t =>
                    {
                        // Restaurar posición del scroll
                        this.Invoke((MethodInvoker)delegate
                        {
                            if (scrollPosition >= 0 && scrollPosition < gridClients.Rows.Count)
                            {
                                gridClients.FirstDisplayedScrollingRowIndex = scrollPosition;
                            }
                            if (selectedRowIndex >= 0 && selectedRowIndex < gridClients.Rows.Count)
                            {
                                gridClients.Rows[selectedRowIndex].Selected = true;
                            }
                        });
                    });
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (!AuthService.Instance.CanEditClients())
            {
                MessageBox.Show("You don't have permission to delete clients.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (gridClients.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a client to delete.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int clientId = (int)gridClients.SelectedRows[0].Cells["Id"].Value;
            string clientName = gridClients.SelectedRows[0].Cells["Name"].Value.ToString();
            int selectedRowIndex = gridClients.SelectedRows[0].Index;
            int scrollPosition = gridClients.FirstDisplayedScrollingRowIndex;

            var result = MessageBox.Show(
                $"Are you sure you want to delete client '{clientName}'?\n\nThis will also delete all associated files and history.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    int userId = AuthService.Instance.GetCurrentUserId();

                    if (repository.DeleteClient(clientId, userId))
                    {
                        MessageBox.Show("Client deleted successfully.", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        InvalidateCache();
                        _ = LoadClientsAsync().ContinueWith(t =>
                        {
                            // Restaurar posición del scroll (ajustado por fila eliminada)
                            this.Invoke((MethodInvoker)delegate
                            {
                                int adjustedIndex = selectedRowIndex > 0 ? selectedRowIndex - 1 : 0;
                                if (scrollPosition >= 0 && scrollPosition < gridClients.Rows.Count)
                                {
                                    gridClients.FirstDisplayedScrollingRowIndex = scrollPosition;
                                }
                                if (adjustedIndex >= 0 && adjustedIndex < gridClients.Rows.Count)
                                {
                                    gridClients.Rows[adjustedIndex].Selected = true;
                                }
                            });
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting client: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ===== MÉTODOS PARA GUARDAR/CARGAR ORDEN DE COLUMNAS =====

        private void GridClients_ColumnDisplayIndexChanged(object sender, DataGridViewColumnEventArgs e)
        {
            SaveColumnOrder();
        }

        private void SaveColumnOrder()
        {
            try
            {
                var columnOrder = new Dictionary<string, int>();
                foreach (DataGridViewColumn column in gridClients.Columns)
                {
                    columnOrder[column.Name] = column.DisplayIndex;
                }

                var directory = Path.GetDirectoryName(ColumnOrderConfigPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonConvert.SerializeObject(columnOrder, Formatting.Indented);
                File.WriteAllText(ColumnOrderConfigPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving column order: {ex.Message}");
            }
        }

        private void LoadColumnOrder()
        {
            try
            {
                if (!File.Exists(ColumnOrderConfigPath))
                    return;

                var json = File.ReadAllText(ColumnOrderConfigPath);
                var columnOrder = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);

                if (columnOrder == null || columnOrder.Count == 0)
                    return;

                foreach (var kvp in columnOrder)
                {
                    if (gridClients.Columns.Contains(kvp.Key))
                    {
                        gridClients.Columns[kvp.Key].DisplayIndex = kvp.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading column order: {ex.Message}");
            }
        }
    }

    // ===== CLIENT EDIT FORM CON DRAG-AND-DROP =====

    public partial class ClientEditForm : Form
    {
        private Client client;
        private ClientRepository repository;
        private CustomFieldRepository customFieldRepository;
        private List<CustomField> customFields;
        private bool isEditMode;

        private TextBox txtSSN;
        private TextBox txtName;
        private DateTimePicker dtpDOB;
        private Label lblAge;
        private TextBox txtAddress;
        private TextBox txtPhone;
        private TextBox txtEmail;
        private ComboBox cmbStatus;
        private TextBox txtNotes;  // NUEVO: Campo de notas
        private DateTimePicker dtpNoteDate;  // Fecha de la nota
        private FlowLayoutPanel flpTaxpayerFields;
        private FlowLayoutPanel flpCustomFields;
        private Button btnSave;
        private Button btnCancel;
        private Button btnViewFiles;
        private Button btnViewChanges;
        private TaskListWidget taskWidget;

        // ===== DRAG AND DROP =====
        private Panel draggedPanel;
        private int dragSourceIndex;
        private FlowLayoutPanel dragSourcePanel;
        private static readonly string FieldOrderConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ReactCRM", "fieldorder.config.json");

        public ClientEditForm()
        {
            this.repository = new ClientRepository();
            this.customFieldRepository = new CustomFieldRepository();
            this.customFields = customFieldRepository.GetAll() ?? new List<CustomField>();
            this.client = new Client
            {
                ExtraData = new Dictionary<string, object>()
            };
            this.client.SetExtraDataValue("Status", "Active");
            this.isEditMode = false;

            InitializeComponent();
            LoadClientData();
        }

        public ClientEditForm(Client client)
        {
            this.repository = new ClientRepository();
            this.customFieldRepository = new CustomFieldRepository();
            this.customFields = customFieldRepository.GetAll() ?? new List<CustomField>();
            this.client = client;
            this.isEditMode = true;

            InitializeComponent();
            LoadClientData();
        }

        private void InitializeComponent()
        {
            this.Text = isEditMode ? "Edit Client" : "New Client";
            this.Size = new Size(1200, 850);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(1000, 700);

            // Panel principal con scroll
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(10)
            };

            // ===== PANEL IZQUIERDO: TAXPAYER INFORMATION =====
            var leftPanel = new Panel
            {
                Location = new Point(10, 10),
                Width = 550,
                Height = 580,
                BorderStyle = BorderStyle.None
            };

            var grpTaxpayer = new GroupBox
            {
                Text = "Taxpayer Information (Drag to reorder)",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Padding = new Padding(10)
            };

            flpTaxpayerFields = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(10),
                AllowDrop = true
            };

            flpTaxpayerFields.DragEnter += FlowPanel_DragEnter;
            flpTaxpayerFields.DragDrop += FlowPanel_DragDrop;
            flpTaxpayerFields.DragOver += FlowPanel_DragOver;

            GenerateTaxpayerFields();

            grpTaxpayer.Controls.Add(flpTaxpayerFields);
            leftPanel.Controls.Add(grpTaxpayer);

            // ===== PANEL DERECHO: EXTRA FIELDS =====
            var rightPanel = new Panel
            {
                Location = new Point(570, 10),
                Width = 550,
                Height = 280,
                BorderStyle = BorderStyle.None
            };

            var grpExtra = new GroupBox
            {
                Text = "Extra (Drag to reorder)",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Padding = new Padding(10)
            };

            flpCustomFields = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(10),
                AllowDrop = true
            };

            flpCustomFields.DragEnter += FlowPanel_DragEnter;
            flpCustomFields.DragDrop += FlowPanel_DragDrop;
            flpCustomFields.DragOver += FlowPanel_DragOver;

            GenerateCustomFields();

            grpExtra.Controls.Add(flpCustomFields);
            rightPanel.Controls.Add(grpExtra);

            mainPanel.Controls.Add(leftPanel);
            mainPanel.Controls.Add(rightPanel);

            // ===== TASK WIDGET (only in edit mode) =====
            if (isEditMode)
            {
                var taskPanel = new Panel
                {
                    Location = new Point(570, 300),
                    Width = 550,
                    Height = 400,
                    BorderStyle = BorderStyle.None
                };

                taskWidget = new TaskListWidget
                {
                    Dock = DockStyle.Fill
                };
                taskWidget.LoadClientTasks(client.Id);
                taskWidget.SetTitle($"📋 Tasks for {client.Name}");
                taskWidget.AddTaskClicked += TaskWidget_AddTaskClicked;
                taskWidget.TaskClicked += TaskWidget_TaskClicked;
                taskWidget.TaskCompleted += (s, task) => taskWidget.RefreshTasks();

                taskPanel.Controls.Add(taskWidget);
                mainPanel.Controls.Add(taskPanel);
            }

            // Botones
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(20, 10, 20, 10)
            };

            btnCancel = new Button
            {
                Text = "Cancel",
                Width = 100,
                Height = 35,
                DialogResult = DialogResult.Cancel,
                Location = new Point(buttonPanel.Width - 230, 10),
                BackColor = Color.FromArgb(127, 140, 141),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            btnSave = new Button
            {
                Text = "Save",
                Width = 100,
                Height = 35,
                DialogResult = DialogResult.OK,
                Location = new Point(buttonPanel.Width - 120, 10),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSave.Click += BtnSave_Click;

            if (isEditMode)
            {
                btnViewFiles = new Button
                {
                    Text = "View Files",
                    Width = 120,
                    Height = 35,
                    Location = new Point(20, 10),
                    BackColor = Color.FromArgb(52, 152, 219),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9),
                    Cursor = Cursors.Hand
                };
                btnViewFiles.Click += (s, e) =>
                {
                    var filesForm = new ClientFilesForm(client.Id, client);
                    filesForm.ShowDialog();
                };
                buttonPanel.Controls.Add(btnViewFiles);

                btnViewChanges = new Button
                {
                    Text = "View Changes",
                    Width = 120,
                    Height = 35,
                    Location = new Point(150, 10),
                    BackColor = Color.FromArgb(155, 89, 182),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9),
                    Cursor = Cursors.Hand
                };
                btnViewChanges.Click += (s, e) =>
                {
                    var historyForm = new FieldHistoryForm(client.Id, client);
                    historyForm.ShowDialog();
                };
                buttonPanel.Controls.Add(btnViewChanges);
            }

            buttonPanel.Controls.Add(btnCancel);
            buttonPanel.Controls.Add(btnSave);

            this.Controls.Add(mainPanel);
            this.Controls.Add(buttonPanel);
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;

            // Cargar orden guardado
            LoadFieldOrder();
        }

        // ===== GENERAR CAMPOS ARRASTRABLES =====

        private void GenerateTaxpayerFields()
        {
            flpTaxpayerFields.Controls.Clear();

            // SSN
            var pnlSSN = CreateDraggableField("SSN", "SSN:");
            txtSSN = new TextBox
            {
                Location = new Point(130, 7),
                Width = 350,
                MaxLength = 50,
                Font = new Font("Segoe UI", 9)
            };
            pnlSSN.Controls.Add(txtSSN);
            EnableDragForChildControls(pnlSSN);
            flpTaxpayerFields.Controls.Add(pnlSSN);

            // Name
            var pnlName = CreateDraggableField("Name", "Name: *");
            txtName = new TextBox
            {
                Location = new Point(130, 7),
                Width = 350,
                MaxLength = 200,
                Font = new Font("Segoe UI", 9)
            };
            pnlName.Controls.Add(txtName);
            EnableDragForChildControls(pnlName);
            flpTaxpayerFields.Controls.Add(pnlName);

            // DOB
            var pnlDOB = CreateDraggableField("DOB", "Date of Birth:");
            dtpDOB = new DateTimePicker
            {
                Location = new Point(130, 7),
                Width = 170,
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 9)
            };
            dtpDOB.ValueChanged += (s, e) => UpdateAge();
            lblAge = new Label
            {
                Location = new Point(310, 10),
                Width = 120,
                Text = "",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9)
            };
            pnlDOB.Controls.Add(dtpDOB);
            pnlDOB.Controls.Add(lblAge);
            EnableDragForChildControls(pnlDOB);
            flpTaxpayerFields.Controls.Add(pnlDOB);

            // Address
            var pnlAddress = CreateDraggableField("Address", "Address:", 70);
            txtAddress = new TextBox
            {
                Location = new Point(130, 7),
                Width = 350,
                Height = 60,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9)
            };
            pnlAddress.Controls.Add(txtAddress);
            EnableDragForChildControls(pnlAddress);
            flpTaxpayerFields.Controls.Add(pnlAddress);

            // Phone
            var pnlPhone = CreateDraggableField("Phone", "Phone:");
            txtPhone = new TextBox
            {
                Location = new Point(130, 7),
                Width = 350,
                MaxLength = 50,
                Font = new Font("Segoe UI", 9)
            };
            pnlPhone.Controls.Add(txtPhone);
            EnableDragForChildControls(pnlPhone);
            flpTaxpayerFields.Controls.Add(pnlPhone);

            // Email
            var pnlEmail = CreateDraggableField("Email", "Email:");
            txtEmail = new TextBox
            {
                Location = new Point(130, 7),
                Width = 350,
                MaxLength = 200,
                Font = new Font("Segoe UI", 9)
            };
            pnlEmail.Controls.Add(txtEmail);
            EnableDragForChildControls(pnlEmail);
            flpTaxpayerFields.Controls.Add(pnlEmail);

            // Status
            var pnlStatus = CreateDraggableField("Status", "Status: *");
            cmbStatus = new ComboBox
            {
                Location = new Point(130, 7),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbStatus.Items.AddRange(new object[] { "Active", "Inactive", "Pending" });
            pnlStatus.Controls.Add(cmbStatus);
            EnableDragForChildControls(pnlStatus);
            flpTaxpayerFields.Controls.Add(pnlStatus);
        }

        private void GenerateCustomFields()
        {
            flpCustomFields.Controls.Clear();

            // ===== NOTES FIELD (Always first in Extra panel) =====
            var pnlNotes = CreateDraggableField("Notes", "Notes:", 120);

            // Date label
            var lblNoteDate = new Label
            {
                Text = "Date:",
                Location = new Point(130, 10),
                Width = 50,
                Font = new Font("Segoe UI", 9)
            };
            pnlNotes.Controls.Add(lblNoteDate);

            // Date picker
            dtpNoteDate = new DateTimePicker
            {
                Location = new Point(185, 7),
                Width = 150,
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 9)
            };
            pnlNotes.Controls.Add(dtpNoteDate);

            // Note text
            txtNotes = new TextBox
            {
                Location = new Point(130, 35),
                Width = 350,
                Height = 75,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9)
            };
            pnlNotes.Controls.Add(txtNotes);
            EnableDragForChildControls(pnlNotes);
            flpCustomFields.Controls.Add(pnlNotes);

            // ===== CUSTOM FIELDS =====
            foreach (var field in customFields)
            {
                var fieldPanel = CreateDraggableField($"custom_{field.Id}",
                    field.Label + (field.IsRequired ? " *" : "") + ":");

                Control inputControl = null;

                switch (field.FieldType)
                {
                    case CustomField.TYPE_TEXT:
                    case "Text":
                        inputControl = new TextBox
                        {
                            Name = $"custom_{field.Id}",
                            Location = new Point(130, 7),
                            Width = 350,
                            Tag = field,
                            Font = new Font("Segoe UI", 9)
                        };
                        break;

                    case CustomField.TYPE_NUMBER:
                    case "Number":
                        inputControl = new NumericUpDown
                        {
                            Name = $"custom_{field.Id}",
                            Location = new Point(130, 7),
                            Width = 150,
                            Maximum = 999999999,
                            Minimum = -999999999,
                            DecimalPlaces = 2,
                            Tag = field,
                            Font = new Font("Segoe UI", 9)
                        };
                        break;

                    case CustomField.TYPE_DATE:
                    case "Date":
                        inputControl = new DateTimePicker
                        {
                            Name = $"custom_{field.Id}",
                            Location = new Point(130, 7),
                            Width = 200,
                            Format = DateTimePickerFormat.Short,
                            Tag = field,
                            Font = new Font("Segoe UI", 9)
                        };
                        break;

                    case CustomField.TYPE_DROPDOWN:
                    case "Dropdown":
                        var combo = new ComboBox
                        {
                            Name = $"custom_{field.Id}",
                            Location = new Point(130, 7),
                            Width = 200,
                            DropDownStyle = ComboBoxStyle.DropDownList,
                            Tag = field,
                            Font = new Font("Segoe UI", 9)
                        };

                        if (!string.IsNullOrEmpty(field.Options))
                        {
                            var options = field.Options.Split(',');
                            combo.Items.AddRange(options);
                        }
                        inputControl = combo;
                        break;

                    case CustomField.TYPE_CHECKBOX:
                    case "Checkbox":
                        inputControl = new CheckBox
                        {
                            Name = $"custom_{field.Id}",
                            Location = new Point(130, 7),
                            Width = 350,
                            Tag = field,
                            Font = new Font("Segoe UI", 9)
                        };
                        break;
                }

                if (inputControl != null)
                {
                    fieldPanel.Controls.Add(inputControl);
                    EnableDragForChildControls(fieldPanel);
                    flpCustomFields.Controls.Add(fieldPanel);
                }
            }
        }

        private Panel CreateDraggableField(string fieldName, string labelText, int height = 40)
        {
            var panel = new Panel
            {
                Name = $"panel_{fieldName}",
                Width = 500,
                Height = height,
                Margin = new Padding(5),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 250),
                Cursor = Cursors.SizeAll,
                AllowDrop = true,
                Tag = fieldName
            };

            var label = new Label
            {
                Text = labelText,
                Location = new Point(5, 10),
                Width = 120,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            panel.Controls.Add(label);

            // Eventos drag and drop del panel
            panel.MouseDown += Panel_MouseDown;
            panel.DragEnter += Panel_DragEnter;

            // Forward mouse events from label to panel
            label.MouseDown += (s, e) => Panel_MouseDown(panel, e);

            return panel;
        }

        // Método helper para agregar drag support a controles hijos
        private void EnableDragForChildControls(Panel panel)
        {
            foreach (Control control in panel.Controls)
            {
                // Skip controls that have their own interaction (dropdowns, date pickers, checkboxes, etc.)
                if (control is Label ||           // Label ya está configurado en CreateDraggableField
                    control is ComboBox ||        // ComboBox necesita click para abrir dropdown
                    control is DateTimePicker ||  // DateTimePicker necesita click para abrir calendario
                    control is CheckBox ||        // CheckBox necesita click para toggle
                    control is NumericUpDown)     // NumericUpDown necesita click para incrementar/decrementar
                {
                    continue;
                }

                // Solo aplicar drag a TextBox y otros controles de entrada simple
                // Forward mouse events from child controls to parent panel
                control.MouseDown += (s, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        Panel_MouseDown(panel, e);
                    }
                };
            }
        }

        // ===== DRAG AND DROP HANDLERS =====

        private void Panel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && sender is Panel panel)
            {
                draggedPanel = panel;
                dragSourcePanel = panel.Parent as FlowLayoutPanel;
                dragSourceIndex = dragSourcePanel.Controls.GetChildIndex(panel);
                panel.DoDragDrop(panel, DragDropEffects.Move);
            }
        }

        private void Panel_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Panel)))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void FlowPanel_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Panel)))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void FlowPanel_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Panel)))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void FlowPanel_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Panel)) && sender is FlowLayoutPanel targetPanel)
            {
                var panel = e.Data.GetData(typeof(Panel)) as Panel;

                if (panel != null)
                {
                    // Obtener posición del mouse en el panel
                    var clientPoint = targetPanel.PointToClient(new Point(e.X, e.Y));

                    // Encontrar el índice donde insertar
                    int insertIndex = targetPanel.Controls.Count;
                    for (int i = 0; i < targetPanel.Controls.Count; i++)
                    {
                        var control = targetPanel.Controls[i];
                        if (clientPoint.Y < control.Top + control.Height / 2)
                        {
                            insertIndex = i;
                            break;
                        }
                    }

                    // Remover del panel original
                    dragSourcePanel?.Controls.Remove(panel);

                    // Insertar en nueva posición
                    targetPanel.Controls.Add(panel);
                    targetPanel.Controls.SetChildIndex(panel, insertIndex);

                    // Guardar orden
                    SaveFieldOrder();
                }
            }
        }

        // ===== GUARDAR/CARGAR ORDEN DE CAMPOS =====

        private void SaveFieldOrder()
        {
            try
            {
                var orderData = new
                {
                    Taxpayer = GetFieldOrder(flpTaxpayerFields),
                    Extra = GetFieldOrder(flpCustomFields)
                };

                var directory = Path.GetDirectoryName(FieldOrderConfigPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonConvert.SerializeObject(orderData, Formatting.Indented);
                File.WriteAllText(FieldOrderConfigPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving field order: {ex.Message}");
            }
        }

        private List<string> GetFieldOrder(FlowLayoutPanel panel)
        {
            var order = new List<string>();
            foreach (Control control in panel.Controls)
            {
                if (control is Panel p && p.Tag != null)
                {
                    order.Add(p.Tag.ToString());
                }
            }
            return order;
        }

        private void LoadFieldOrder()
        {
            try
            {
                if (!File.Exists(FieldOrderConfigPath))
                    return;

                var json = File.ReadAllText(FieldOrderConfigPath);
                dynamic orderData = JsonConvert.DeserializeObject(json);

                if (orderData == null)
                    return;

                // Reordenar Taxpayer
                if (orderData.Taxpayer != null)
                {
                    ReorderFields(flpTaxpayerFields, orderData.Taxpayer.ToObject<List<string>>());
                }

                // Reordenar Extra
                if (orderData.Extra != null)
                {
                    ReorderFields(flpCustomFields, orderData.Extra.ToObject<List<string>>());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading field order: {ex.Message}");
            }
        }

        private void ReorderFields(FlowLayoutPanel panel, List<string> order)
        {
            // Collect all controls first
            var allControls = new List<Panel>();
            foreach (Control control in panel.Controls)
            {
                if (control is Panel p)
                {
                    allControls.Add(p);
                }
            }

            // Remove all controls from panel
            panel.SuspendLayout();
            panel.Controls.Clear();

            // Add controls back in the saved order
            foreach (var fieldName in order)
            {
                var control = allControls.FirstOrDefault(p => p.Tag?.ToString() == fieldName);
                if (control != null)
                {
                    panel.Controls.Add(control);
                    allControls.Remove(control); // Remove from list to avoid duplicates
                }
            }

            // Add any remaining controls that weren't in the saved order (new fields)
            foreach (var control in allControls)
            {
                panel.Controls.Add(control);
            }

            panel.ResumeLayout();
        }

        private Panel FindPanelByTag(FlowLayoutPanel parent, string tag)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is Panel p && p.Tag?.ToString() == tag)
                    return p;
            }
            return null;
        }

        // ===== MÉTODOS EXISTENTES =====

        private void LoadClientData()
        {
            if (client == null) return;

            txtSSN.Text = client.SSN;
            txtName.Text = client.Name;

            if (client.DOB.HasValue && client.DOB.Value.Year > 1900)
            {
                dtpDOB.Value = client.DOB.Value;
            }
            else
            {
                dtpDOB.Value = DateTime.Now.AddYears(-30);
            }

            UpdateAge();

            txtAddress.Text = client.GetExtraDataValue("Address") ?? "";
            txtPhone.Text = client.Phone;
            txtEmail.Text = client.Email;
            cmbStatus.Text = client.GetExtraDataValue("Status") ?? "Active";
            txtNotes.Text = client.Notes ?? "";  // FIX: Usar campo directo Notes

            // Cargar fecha de nota
            var noteDateStr = client.GetExtraDataValue("NoteDate");
            if (!string.IsNullOrEmpty(noteDateStr) && DateTime.TryParse(noteDateStr, out DateTime noteDate))
            {
                dtpNoteDate.Value = noteDate;
            }
            else
            {
                dtpNoteDate.Value = DateTime.Now;
            }

            if (client.ExtraData != null)
            {
                foreach (var field in customFields)
                {
                    var control = FindControlByName($"custom_{field.Id}");
                    if (control != null)
                    {
                        var value = client.GetExtraDataValue(field.FieldName);

                        if (control is TextBox txt)
                        {
                            txt.Text = value ?? "";
                        }
                        else if (control is NumericUpDown num)
                        {
                            if (decimal.TryParse(value, out decimal numValue))
                            {
                                num.Value = numValue;
                            }
                        }
                        else if (control is DateTimePicker dtp)
                        {
                            if (DateTime.TryParse(value, out DateTime dateValue))
                            {
                                dtp.Value = dateValue;
                            }
                        }
                        else if (control is ComboBox cmb)
                        {
                            cmb.Text = value ?? "";
                        }
                        else if (control is CheckBox chk)
                        {
                            chk.Checked = value?.ToLower() == "true";
                        }
                    }
                }
            }
        }

        private Control FindControlByName(string name)
        {
            // Buscar en ambos paneles
            foreach (Control panel in flpTaxpayerFields.Controls)
            {
                foreach (Control ctrl in panel.Controls)
                {
                    if (ctrl.Name == name)
                        return ctrl;
                }
            }

            foreach (Control panel in flpCustomFields.Controls)
            {
                foreach (Control ctrl in panel.Controls)
                {
                    if (ctrl.Name == name)
                        return ctrl;
                }
            }

            return null;
        }

        private void UpdateAge()
        {
            var age = DateTime.Now.Year - dtpDOB.Value.Year;
            if (dtpDOB.Value.Date > DateTime.Now.AddYears(-age)) age--;

            lblAge.Text = $"Age: {age} years";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(cmbStatus.Text))
            {
                MessageBox.Show("Status is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbStatus.Focus();
                return;
            }

            foreach (var field in customFields)
            {
                if (field.IsRequired)
                {
                    var control = FindControlByName($"custom_{field.Id}");
                    if (control != null)
                    {
                        bool isEmpty = false;

                        if (control is TextBox txt)
                        {
                            isEmpty = string.IsNullOrWhiteSpace(txt.Text);
                        }
                        else if (control is ComboBox cmb)
                        {
                            isEmpty = string.IsNullOrWhiteSpace(cmb.Text);
                        }

                        if (isEmpty)
                        {
                            MessageBox.Show($"{field.Label} is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            control.Focus();
                            return;
                        }
                    }
                }
            }

            client.SSN = txtSSN.Text.Trim();
            client.Name = txtName.Text.Trim();
            client.DOB = dtpDOB.Value.Date;
            client.Phone = txtPhone.Text.Trim();
            client.Email = txtEmail.Text.Trim();
            client.Notes = txtNotes.Text.Trim();  // FIX: Guardar en campo directo Notes

            client.SetExtraDataValue("Address", txtAddress.Text.Trim());
            client.SetExtraDataValue("Status", cmbStatus.Text);
            client.SetExtraDataValue("NoteDate", dtpNoteDate.Value.ToString("yyyy-MM-dd"));

            foreach (var field in customFields)
            {
                var control = FindControlByName($"custom_{field.Id}");
                if (control != null)
                {
                    object value = null;

                    if (control is TextBox txt)
                    {
                        value = txt.Text;
                    }
                    else if (control is NumericUpDown num)
                    {
                        value = num.Value;
                    }
                    else if (control is DateTimePicker dtp)
                    {
                        value = dtp.Value.ToString("yyyy-MM-dd");
                    }
                    else if (control is ComboBox cmb)
                    {
                        value = cmb.Text;
                    }
                    else if (control is CheckBox chk)
                    {
                        value = chk.Checked;
                    }

                    if (value != null)
                    {
                        client.SetExtraDataValue(field.FieldName, value);
                    }
                }
            }

            try
            {
                int userId = AuthService.Instance.GetCurrentUserId();

                if (isEditMode)
                {
                    repository.UpdateClient(client, userId);
                    MessageBox.Show("Client updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    client.Id = repository.CreateClient(client, userId);
                    MessageBox.Show("Client created successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving client: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TaskWidget_AddTaskClicked(object sender, EventArgs e)
        {
            // Create a new task for this client (pre-select the client)
            System.Diagnostics.Debug.WriteLine($"Creating task for client ID: {client.Id}, Name: {client.Name}");
            var taskForm = new TaskEditForm(client.Id);
            if (taskForm.ShowDialog() == DialogResult.OK)
            {
                // Refresh task list after task is created
                System.Diagnostics.Debug.WriteLine($"Task created successfully, refreshing task list for client {client.Id}");
                taskWidget?.RefreshTasks();
            }
        }

        private void TaskWidget_TaskClicked(object sender, TodoTask task)
        {
            // Open task edit form to view/edit the task
            if (task != null)
            {
                var taskForm = new TaskEditForm(task);
                if (taskForm.ShowDialog() == DialogResult.OK)
                {
                    // Refresh task list after task is edited
                    taskWidget?.RefreshTasks();
                }
            }
        }
    }
}