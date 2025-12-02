using System;

using System.Drawing;

using System.Windows.Forms;

using System.Linq;

using System.Collections.Generic;

using ReactCRM.Database;

using ReactCRM.Models;

using ReactCRM.Services;



namespace ReactCRM.UI.Time

{

    public partial class TimeClockForm : Form

    {

        private TimeEntryRepository timeEntryRepository;

        private int currentUserId;

        private bool isAdmin;



        private Label lblCurrentStatus;

        private Label lblCurrentTime;

        private Button btnClockIn;

        private Button btnClockOut;

        private DataGridView gridTimeEntries;

        private System.Windows.Forms.Timer clockTimer;

        private Panel panelHeader;

        private Panel panelClock;

        private Panel panelHistory;

        private Button btnGenerateReport;



        public TimeClockForm()

        {

            timeEntryRepository = new TimeEntryRepository();

            currentUserId = AuthService.Instance.GetCurrentUserId();

            isAdmin = AuthService.Instance.GetCurrentUserRole() == "Admin";



            InitializeComponent();

            LoadTimeEntries();

            UpdateCurrentStatus();



            // Start clock timer to update current time

            clockTimer = new System.Windows.Forms.Timer();

            clockTimer.Interval = 1000; // Update every second

            clockTimer.Tick += ClockTimer_Tick;

            clockTimer.Start();

        }



        private void InitializeComponent()

        {

            this.Text = "Marcaje de Horario - Time Clock";

            this.StartPosition = FormStartPosition.CenterParent;

            this.Size = new Size(1000, 700);

            this.MinimumSize = new Size(800, 600);

            this.FormBorderStyle = FormBorderStyle.Sizable;

            this.BackColor = Color.FromArgb(236, 240, 241);



            // Header Panel

            panelHeader = new Panel

            {

                Dock = DockStyle.Top,

                Height = 80,

                BackColor = Color.FromArgb(52, 73, 94),

                Padding = new Padding(20)

            };



            var lblTitle = new Label

            {

                Text = "⏰ MARCAJE DE HORARIO",

                Font = new Font("Segoe UI", 18, FontStyle.Bold),

                ForeColor = Color.White,

                AutoSize = true,

                Location = new Point(20, 15)

            };

            panelHeader.Controls.Add(lblTitle);



            var lblUser = new Label

            {

                Text = $"Usuario: {AuthService.Instance.GetCurrentUsername()}",

                Font = new Font("Segoe UI", 10),

                ForeColor = Color.FromArgb(189, 195, 199),

                AutoSize = true,

                Location = new Point(20, 50)

            };

            panelHeader.Controls.Add(lblUser);



            // Clock Panel (Main)

            panelClock = new Panel

            {

                Dock = DockStyle.Top,

                Height = 280,

                BackColor = Color.White,

                Padding = new Padding(20)

            };



            // Current Time Display (Large Clock)

            lblCurrentTime = new Label

            {

                Text = DateTime.Now.ToString("HH:mm:ss"),

                Font = new Font("Segoe UI", 48, FontStyle.Bold),

                ForeColor = Color.FromArgb(52, 73, 94),

                Location = new Point(20, 20),

                AutoSize = true

            };

            panelClock.Controls.Add(lblCurrentTime);



            var lblDate = new Label

            {

                Text = DateTime.Now.ToString("dddd, dd MMMM yyyy"),

                Font = new Font("Segoe UI", 14),

                ForeColor = Color.FromArgb(127, 140, 141),

                Location = new Point(20, 90),

                AutoSize = true

            };

            panelClock.Controls.Add(lblDate);



            // Current Status

            lblCurrentStatus = new Label

            {

                Text = "Estado: Cargando...",

                Font = new Font("Segoe UI", 14, FontStyle.Bold),

                ForeColor = Color.FromArgb(41, 128, 185),

                Location = new Point(20, 140),

                AutoSize = true

            };

            panelClock.Controls.Add(lblCurrentStatus);



            // Clock In Button

            btnClockIn = new Button

            {

                Text = "🟢 MARCAR ENTRADA",

                Location = new Point(20, 190),

                Size = new Size(220, 60),

                BackColor = Color.FromArgb(46, 204, 113),

                ForeColor = Color.White,

                FlatStyle = FlatStyle.Flat,

                Font = new Font("Segoe UI", 12, FontStyle.Bold),

                Cursor = Cursors.Hand

            };

            btnClockIn.FlatAppearance.BorderSize = 0;

            btnClockIn.Click += BtnClockIn_Click;

            panelClock.Controls.Add(btnClockIn);



            // Clock Out Button

            btnClockOut = new Button

            {

                Text = "🔴 MARCAR SALIDA",

                Location = new Point(260, 190),

                Size = new Size(220, 60),

                BackColor = Color.FromArgb(231, 76, 60),

                ForeColor = Color.White,

                FlatStyle = FlatStyle.Flat,

                Font = new Font("Segoe UI", 12, FontStyle.Bold),

                Cursor = Cursors.Hand,

                Enabled = false

            };

            btnClockOut.FlatAppearance.BorderSize = 0;

            btnClockOut.Click += BtnClockOut_Click;

            panelClock.Controls.Add(btnClockOut);



            // Generate Report Button (Admin only) - moved to clock panel for visibility

            if (isAdmin)

            {

                btnGenerateReport = new Button

                {

                    Text = "📊 Generar Reporte",

                    Location = new Point(500, 190),

                    Size = new Size(200, 60),

                    BackColor = Color.FromArgb(155, 89, 182),

                    ForeColor = Color.White,

                    FlatStyle = FlatStyle.Flat,

                    Font = new Font("Segoe UI", 11, FontStyle.Bold),

                    Cursor = Cursors.Hand

                };

                btnGenerateReport.FlatAppearance.BorderSize = 0;

                btnGenerateReport.Click += BtnGenerateReport_Click;

                panelClock.Controls.Add(btnGenerateReport);

            }



            // History Panel

            panelHistory = new Panel

            {

                Dock = DockStyle.Fill,

                BackColor = Color.White,

                Padding = new Padding(20)

            };



            var lblHistory = new Label

            {

                Text = isAdmin ? "📋 Historial de Marcajes (Todos los trabajadores)" : "📋 Mi Historial de Marcajes",

                Font = new Font("Segoe UI", 12, FontStyle.Bold),

                ForeColor = Color.FromArgb(52, 73, 94),

                Location = new Point(20, 10),

                AutoSize = true

            };

            panelHistory.Controls.Add(lblHistory);



            // Time entries grid

            gridTimeEntries = new DataGridView

            {

                Location = new Point(20, 50),

                Size = new Size(940, 220),

                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,

                SelectionMode = DataGridViewSelectionMode.FullRowSelect,

                MultiSelect = false,

                ReadOnly = !isAdmin,  // Editable only for admins

                AllowUserToAddRows = false,

                AllowUserToDeleteRows = false,

                BackgroundColor = Color.White,

                BorderStyle = BorderStyle.Fixed3D,

                RowHeadersVisible = false,

                Font = new Font("Segoe UI", 9),

                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right

            };

            gridTimeEntries.CellValueChanged += GridTimeEntries_CellValueChanged;



            // Setup columns

            SetupTimeTrackingGrid();



            panelHistory.Controls.Add(gridTimeEntries);



            // Add all panels to form

            this.Controls.Add(panelHistory);

            this.Controls.Add(panelClock);

            this.Controls.Add(panelHeader);

        }



        private void SetupTimeTrackingGrid()

        {

            gridTimeEntries.Columns.Clear();



            // Hidden ID column for editing

            gridTimeEntries.Columns.Add("EntryId", "ID");

            gridTimeEntries.Columns["EntryId"].Visible = false;



            if (isAdmin)

            {

                gridTimeEntries.Columns.Add("Worker", "Trabajador");

                gridTimeEntries.Columns["Worker"].Width = 150;

                gridTimeEntries.Columns["Worker"].ReadOnly = true;

            }



            gridTimeEntries.Columns.Add("Date", "Fecha");

            gridTimeEntries.Columns.Add("ClockIn", "Entrada");

            gridTimeEntries.Columns.Add("ClockOut", "Salida");

            gridTimeEntries.Columns.Add("Duration", "Duración");

            gridTimeEntries.Columns.Add("Break", "Descanso (min)");

            gridTimeEntries.Columns.Add("Notes", "Notas");

            gridTimeEntries.Columns.Add("Status", "Estado");



            gridTimeEntries.Columns["Date"].Width = 100;

            gridTimeEntries.Columns["Date"].ReadOnly = true;

            gridTimeEntries.Columns["ClockIn"].Width = 100;

            gridTimeEntries.Columns["ClockOut"].Width = 100;

            gridTimeEntries.Columns["Duration"].Width = 100;

            gridTimeEntries.Columns["Duration"].ReadOnly = true;

            gridTimeEntries.Columns["Break"].Width = 80;

            gridTimeEntries.Columns["Notes"].Width = 150;

            gridTimeEntries.Columns["Status"].Width = 100;

            gridTimeEntries.Columns["Status"].ReadOnly = true;

        }



        private void ClockTimer_Tick(object sender, EventArgs e)

        {

            lblCurrentTime.Text = DateTime.Now.ToString("HH:mm:ss");

        }



        private void LoadTimeEntries()

        {

            try

            {

                // Load time entries - Admin sees all, workers see only their own

                List<TimeEntry> entries;

                if (isAdmin)

                {

                    // Admin can see all time entries for all workers

                    entries = timeEntryRepository.GetAll();

                }

                else

                {

                    // Workers only see their own time entries

                    entries = timeEntryRepository.GetByWorkerId(currentUserId);

                }



                gridTimeEntries.Rows.Clear();



                // Get worker info for admin view

                WorkerRepository workerRepo = null;

                Dictionary<int, string> workerNames = new Dictionary<int, string>();



                if (isAdmin)

                {

                    workerRepo = new WorkerRepository();

                    var workers = workerRepo.GetAllWorkers();

                    foreach (var worker in workers)

                    {

                        workerNames[worker.Id] = worker.Username;

                    }

                }



                DateTime? lastDate = null;



                foreach (var entry in entries.OrderByDescending(e => e.Date).ThenByDescending(e => e.ClockIn).Take(50))

                {

                    // Add yellow divider when date changes

                    if (lastDate.HasValue && entry.Date.Date != lastDate.Value.Date)

                    {

                        try

                        {

                            var dividerRow = new DataGridViewRow();

                            dividerRow.CreateCells(gridTimeEntries);



                            // Initialize all cells to empty string to avoid InvalidOperationException

                            for (int i = 0; i < dividerRow.Cells.Count; i++)

                            {

                                dividerRow.Cells[i].Value = "";

                            }



                            // Set divider text

                            string dividerText = $"──────── {entry.Date:dddd, dd MMMM yyyy} ────────";

                            int dividerColumnIndex = isAdmin ? 1 : 0;  // Skip ID column, show in first visible column

                            dividerRow.Cells[dividerColumnIndex].Value = dividerText;



                            // Yellow background

                            dividerRow.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 59);

                            dividerRow.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);

                            dividerRow.DefaultCellStyle.ForeColor = Color.FromArgb(52, 73, 94);

                            dividerRow.ReadOnly = true;



                            gridTimeEntries.Rows.Add(dividerRow);

                        }

                        catch (Exception ex)

                        {

                            System.Diagnostics.Debug.WriteLine($"Error adding divider row: {ex.Message}");

                        }

                    }



                    List<string> rowData = new List<string>();



                    // Add entry ID (hidden column)

                    rowData.Add(entry.Id.ToString());



                    if (isAdmin)

                    {

                        string workerName = workerNames.ContainsKey(entry.WorkerId)

                            ? workerNames[entry.WorkerId]

                            : $"Worker #{entry.WorkerId}";

                        rowData.Add(workerName);

                    }



                    rowData.AddRange(new[]

                    {

                        entry.Date.ToString("yyyy-MM-dd"),

                        entry.ClockIn.ToString("HH:mm:ss"),

                        entry.ClockOut?.ToString("HH:mm:ss") ?? "---",

                        entry.GetDurationString(),

                        entry.BreakMinutes.ToString(),

                        entry.Notes ?? "",

                        entry.IsClockedIn() ? "En progreso" : "Completado"

                    });



                    gridTimeEntries.Rows.Add(rowData.ToArray());



                    lastDate = entry.Date;

                }



                // Add initial date divider if there are entries

                if (entries.Count > 0 && lastDate.HasValue)

                {

                    try

                    {

                        var firstDividerRow = new DataGridViewRow();

                        firstDividerRow.CreateCells(gridTimeEntries);



                        // Initialize all cells

                        for (int i = 0; i < firstDividerRow.Cells.Count; i++)

                        {

                            firstDividerRow.Cells[i].Value = "";

                        }



                        string dividerText = $"──────── {lastDate.Value:dddd, dd MMMM yyyy} ────────";

                        int dividerColumnIndex = isAdmin ? 1 : 0;

                        firstDividerRow.Cells[dividerColumnIndex].Value = dividerText;



                        firstDividerRow.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 59);

                        firstDividerRow.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);

                        firstDividerRow.DefaultCellStyle.ForeColor = Color.FromArgb(52, 73, 94);

                        firstDividerRow.ReadOnly = true;



                        gridTimeEntries.Rows.Insert(0, firstDividerRow);

                    }

                    catch (Exception ex)

                    {

                        System.Diagnostics.Debug.WriteLine($"Error adding first divider row: {ex.Message}");

                    }

                }



                // Update current status

                UpdateCurrentStatus();

            }

            catch (Exception ex)

            {

                MessageBox.Show($"Error cargando entradas de tiempo: {ex.Message}", "Error",

                    MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }



        private void UpdateCurrentStatus()

        {

            try

            {

                var todayEntries = timeEntryRepository.GetByWorkerIdAndDate(currentUserId, DateTime.Today);

                var activeEntry = todayEntries.FirstOrDefault(e => e.IsClockedIn());



                if (activeEntry != null)

                {

                    lblCurrentStatus.Text = $"✅ Estado: MARCADO desde {activeEntry.ClockIn:HH:mm:ss}";

                    lblCurrentStatus.ForeColor = Color.FromArgb(46, 204, 113);

                    btnClockIn.Enabled = false;

                    btnClockOut.Enabled = true;

                }

                else

                {

                    lblCurrentStatus.Text = "⭕ Estado: NO MARCADO";

                    lblCurrentStatus.ForeColor = Color.FromArgb(231, 76, 60);

                    btnClockIn.Enabled = true;

                    btnClockOut.Enabled = false;

                }

            }

            catch (Exception ex)

            {

                System.Diagnostics.Debug.WriteLine($"Error actualizando estado: {ex.Message}");

            }

        }



        private void BtnClockIn_Click(object sender, EventArgs e)

        {

            try

            {

                // Create new time entry

                var entry = new TimeEntry

                {

                    WorkerId = currentUserId,

                    Date = DateTime.Today,

                    ClockIn = DateTime.Now,

                    ClockOut = null,

                    EntryType = "Regular",

                    Notes = "",

                    BreakMinutes = 0,

                    IsActive = true,

                    IsApproved = false

                };



                timeEntryRepository.Create(entry);



                MessageBox.Show($"✅ Entrada marcada exitosamente a las {DateTime.Now:HH:mm:ss}", "Éxito",

                    MessageBoxButtons.OK, MessageBoxIcon.Information);



                // Reload time entries and update status

                LoadTimeEntries();

            }

            catch (Exception ex)

            {

                MessageBox.Show($"❌ Error al marcar entrada: {ex.Message}", "Error",

                    MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }



        private void BtnClockOut_Click(object sender, EventArgs e)

        {

            try

            {

                // Find active time entry

                var todayEntries = timeEntryRepository.GetByWorkerIdAndDate(currentUserId, DateTime.Today);

                var activeEntry = todayEntries.FirstOrDefault(e => e.IsClockedIn());



                if (activeEntry == null)

                {

                    MessageBox.Show("⚠️ No hay entrada activa para marcar salida.", "Advertencia",

                        MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    return;

                }



                // Update clock out time

                activeEntry.ClockOut = DateTime.Now;

                timeEntryRepository.Update(activeEntry);



                var duration = activeEntry.ClockOut.Value - activeEntry.ClockIn;



                MessageBox.Show($"✅ Salida marcada exitosamente a las {DateTime.Now:HH:mm:ss}\n\nTiempo trabajado: {duration.Hours}h {duration.Minutes}m",

                    "Éxito",

                    MessageBoxButtons.OK, MessageBoxIcon.Information);



                // Reload time entries and update status

                LoadTimeEntries();

            }

            catch (Exception ex)

            {

                MessageBox.Show($"❌ Error al marcar salida: {ex.Message}", "Error",

                    MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }



        private void GridTimeEntries_CellValueChanged(object sender, DataGridViewCellEventArgs e)

        {

            if (!isAdmin || e.RowIndex < 0 || e.ColumnIndex < 0)

                return;



            try

            {

                var row = gridTimeEntries.Rows[e.RowIndex];



                // Skip divider rows

                if (row.Cells["EntryId"].Value == null || string.IsNullOrEmpty(row.Cells["EntryId"].Value.ToString()))

                    return;



                int entryId = int.Parse(row.Cells["EntryId"].Value.ToString());

                var entry = timeEntryRepository.GetById(entryId);



                if (entry == null)

                    return;



                string columnName = gridTimeEntries.Columns[e.ColumnIndex].Name;



                // Update based on column

                if (columnName == "ClockIn")

                {

                    if (DateTime.TryParse(row.Cells["ClockIn"].Value.ToString(), out DateTime clockIn))

                    {

                        entry.ClockIn = entry.Date.Date.Add(clockIn.TimeOfDay);

                        timeEntryRepository.Update(entry);

                        MessageBox.Show("✅ Hora de entrada actualizada", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    }

                }

                else if (columnName == "ClockOut")

                {

                    string clockOutValue = row.Cells["ClockOut"].Value?.ToString();

                    if (string.IsNullOrEmpty(clockOutValue) || clockOutValue == "---")

                    {

                        entry.ClockOut = null;

                    }

                    else if (DateTime.TryParse(clockOutValue, out DateTime clockOut))

                    {

                        entry.ClockOut = entry.Date.Date.Add(clockOut.TimeOfDay);

                    }

                    timeEntryRepository.Update(entry);

                    MessageBox.Show("✅ Hora de salida actualizada", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }

                else if (columnName == "Break")

                {

                    if (int.TryParse(row.Cells["Break"].Value.ToString(), out int breakMinutes))

                    {

                        entry.BreakMinutes = breakMinutes;

                        timeEntryRepository.Update(entry);

                    }

                }

                else if (columnName == "Notes")

                {

                    entry.Notes = row.Cells["Notes"].Value?.ToString() ?? "";

                    timeEntryRepository.Update(entry);

                }



                // Refresh to update duration

                LoadTimeEntries();

            }

            catch (Exception ex)

            {

                MessageBox.Show($"Error actualizando entrada: {ex.Message}", "Error",

                    MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }



        private void BtnGenerateReport_Click(object sender, EventArgs e)

        {

            try

            {

                // Open the comprehensive worker hours report form

                var reportForm = new WorkerHoursReportForm();

                reportForm.ShowDialog();

            }

            catch (Exception ex)

            {

                MessageBox.Show($"Error opening report form: {ex.Message}", "Error",

                    MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }



        protected override void OnFormClosed(FormClosedEventArgs e)

        {

            base.OnFormClosed(e);

            clockTimer?.Stop();

            clockTimer?.Dispose();

        }

    }

}