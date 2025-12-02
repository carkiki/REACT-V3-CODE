using System;

using System.Drawing;

using System.IO;

using System.Linq;

using System.Windows.Forms;

using ReactCRM.Database;

using ReactCRM.Models;

using ReactCRM.Services;



namespace ReactCRM.UI.Time

{

    public class WorkerHoursReportForm : Form

    {

        private ComboBox cmbWorker;

        private ComboBox cmbPeriod;

        private DateTimePicker dtpStartDate;

        private DateTimePicker dtpEndDate;

        private Label lblCustomDates;

        private CheckBox chkIncludePayRate;

        private NumericUpDown numPayRate;

        private Button btnGenerate;

        private Button btnCancel;



        private WorkerRepository workerRepository;



        public WorkerHoursReportForm()

        {

            workerRepository = new WorkerRepository();

            InitializeComponent();

            LoadWorkers();

        }



        private void InitializeComponent()

        {

            this.Text = "Generate Worker Hours Report";

            this.Size = new Size(550, 500);

            this.StartPosition = FormStartPosition.CenterParent;

            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            this.MaximizeBox = false;

            this.MinimizeBox = false;



            var mainPanel = new Panel

            {

                Dock = DockStyle.Fill,

                Padding = new Padding(20),

                AutoScroll = true

            };



            int yPos = 20;

            int labelWidth = 150;

            int fieldWidth = 300;

            int fieldHeight = 25;

            int spacing = 15;



            // Title

            var lblTitle = new Label

            {

                Text = "📊 Worker Hours Report Generator",

                Location = new Point(20, yPos),

                Size = new Size(480, 30),

                Font = new Font(UITheme.Fonts.BodyRegular.FontFamily, 14, FontStyle.Bold),

                ForeColor = UITheme.Colors.HeaderPrimary

            };

            mainPanel.Controls.Add(lblTitle);

            yPos += 40;



            // Worker selection

            var lblWorker = new Label

            {

                Text = "Select Worker:",

                Location = new Point(20, yPos),

                Size = new Size(labelWidth, fieldHeight),

                Font = UITheme.Fonts.BodyRegular,

                TextAlign = ContentAlignment.MiddleLeft

            };

            mainPanel.Controls.Add(lblWorker);



            cmbWorker = new ComboBox

            {

                Location = new Point(180, yPos),

                Size = new Size(fieldWidth, fieldHeight),

                Font = UITheme.Fonts.BodyRegular,

                DropDownStyle = ComboBoxStyle.DropDownList

            };

            mainPanel.Controls.Add(cmbWorker);

            yPos += fieldHeight + spacing;



            // Period selection

            var lblPeriod = new Label

            {

                Text = "Report Period:",

                Location = new Point(20, yPos),

                Size = new Size(labelWidth, fieldHeight),

                Font = UITheme.Fonts.BodyRegular,

                TextAlign = ContentAlignment.MiddleLeft

            };

            mainPanel.Controls.Add(lblPeriod);



            cmbPeriod = new ComboBox

            {

                Location = new Point(180, yPos),

                Size = new Size(fieldWidth, fieldHeight),

                Font = UITheme.Fonts.BodyRegular,

                DropDownStyle = ComboBoxStyle.DropDownList

            };

            cmbPeriod.Items.AddRange(new object[] {

                "This Week (Last 7 days)",

                "Last 2 Weeks (14 days)",

                "This Month (Last 30 days)",

                "Custom Date Range"

            });

            cmbPeriod.SelectedIndex = 0;

            cmbPeriod.SelectedIndexChanged += CmbPeriod_SelectedIndexChanged;

            mainPanel.Controls.Add(cmbPeriod);

            yPos += fieldHeight + spacing + 10;



            // Custom date range section

            lblCustomDates = new Label

            {

                Text = "📅 Custom Date Range:",

                Location = new Point(20, yPos),

                Size = new Size(460, 25),

                Font = new Font(UITheme.Fonts.BodyRegular.FontFamily, 10, FontStyle.Bold),

                ForeColor = UITheme.Colors.HeaderPrimary,

                Visible = false

            };

            mainPanel.Controls.Add(lblCustomDates);

            yPos += 30;



            // Start date

            var lblStartDate = new Label

            {

                Text = "Start Date:",

                Location = new Point(40, yPos),

                Size = new Size(130, fieldHeight),

                Font = UITheme.Fonts.BodyRegular,

                TextAlign = ContentAlignment.MiddleLeft,

                Visible = false

            };

            mainPanel.Controls.Add(lblStartDate);



            dtpStartDate = new DateTimePicker

            {

                Location = new Point(180, yPos),

                Size = new Size(fieldWidth, fieldHeight),

                Font = UITheme.Fonts.BodyRegular,

                Format = DateTimePickerFormat.Short,

                Visible = false

            };

            mainPanel.Controls.Add(dtpStartDate);

            yPos += fieldHeight + spacing;



            // End date

            var lblEndDate = new Label

            {

                Text = "End Date:",

                Location = new Point(40, yPos),

                Size = new Size(130, fieldHeight),

                Font = UITheme.Fonts.BodyRegular,

                TextAlign = ContentAlignment.MiddleLeft,

                Visible = false

            };

            mainPanel.Controls.Add(lblEndDate);



            dtpEndDate = new DateTimePicker

            {

                Location = new Point(180, yPos),

                Size = new Size(fieldWidth, fieldHeight),

                Font = UITheme.Fonts.BodyRegular,

                Format = DateTimePickerFormat.Short,

                Value = DateTime.Now,

                Visible = false

            };

            mainPanel.Controls.Add(dtpEndDate);

            yPos += fieldHeight + spacing + 15;



            // Separator line

            var separator = new Label

            {

                Location = new Point(20, yPos),

                Size = new Size(480, 2),

                BorderStyle = BorderStyle.Fixed3D

            };

            mainPanel.Controls.Add(separator);

            yPos += 15;



            // Pay rate section

            var lblPaySection = new Label

            {

                Text = "💰 Pay Calculation (Optional):",

                Location = new Point(20, yPos),

                Size = new Size(460, 25),

                Font = new Font(UITheme.Fonts.BodyRegular.FontFamily, 10, FontStyle.Bold),

                ForeColor = UITheme.Colors.HeaderPrimary

            };

            mainPanel.Controls.Add(lblPaySection);

            yPos += 30;



            chkIncludePayRate = new CheckBox

            {

                Text = "Include pay calculation in report",

                Location = new Point(40, yPos),

                Size = new Size(280, fieldHeight),

                Font = UITheme.Fonts.BodyRegular,

                Checked = false

            };

            chkIncludePayRate.CheckedChanged += ChkIncludePayRate_CheckedChanged;

            mainPanel.Controls.Add(chkIncludePayRate);

            yPos += fieldHeight + 10;



            var lblPayRate = new Label

            {

                Text = "Hourly Rate ($):",

                Location = new Point(40, yPos),

                Size = new Size(130, fieldHeight),

                Font = UITheme.Fonts.BodyRegular,

                TextAlign = ContentAlignment.MiddleLeft,

                Enabled = false

            };

            mainPanel.Controls.Add(lblPayRate);



            numPayRate = new NumericUpDown

            {

                Location = new Point(180, yPos),

                Size = new Size(150, fieldHeight),

                Font = UITheme.Fonts.BodyRegular,

                DecimalPlaces = 2,

                Minimum = 0,

                Maximum = 999.99m,

                Value = 15.00m,

                Increment = 0.25m,

                Enabled = false

            };

            mainPanel.Controls.Add(numPayRate);

            yPos += fieldHeight + spacing;



            // Info message

            var lblInfo = new Label

            {

                Text = "💡 Tip: The report will include date, clock in/out times, breaks, total hours, " +

                       "and pay calculations if enabled. The PDF will be saved to your Downloads folder.",

                Location = new Point(20, yPos),

                Size = new Size(480, 50),

                Font = UITheme.Fonts.BodySmall,

                ForeColor = UITheme.Colors.TextSecondary

            };

            mainPanel.Controls.Add(lblInfo);



            // Store references to labels that need to be toggled

            dtpStartDate.Tag = lblStartDate;

            dtpEndDate.Tag = lblEndDate;

            numPayRate.Tag = lblPayRate;



            // Button panel

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

                BackColor = Color.FromArgb(127, 140, 141),

                ForeColor = Color.White,

                FlatStyle = FlatStyle.Flat,

                Font = UITheme.Fonts.BodyRegular,

                Cursor = Cursors.Hand,

                Anchor = AnchorStyles.Bottom | AnchorStyles.Right

            };

            btnCancel.FlatAppearance.BorderSize = 0;

            btnCancel.Location = new Point(buttonPanel.Width - 230, 10);

            buttonPanel.Controls.Add(btnCancel);



            btnGenerate = new Button

            {

                Text = "📄 Generate Report",

                Width = 150,

                Height = 35,

                BackColor = UITheme.Colors.ButtonSuccess,

                ForeColor = Color.White,

                FlatStyle = FlatStyle.Flat,

                Font = UITheme.Fonts.BodyRegular,

                Cursor = Cursors.Hand,

                Anchor = AnchorStyles.Bottom | AnchorStyles.Right

            };

            btnGenerate.FlatAppearance.BorderSize = 0;

            btnGenerate.Location = new Point(buttonPanel.Width - 170, 10);

            btnGenerate.Click += BtnGenerate_Click;

            buttonPanel.Controls.Add(btnGenerate);



            this.Controls.Add(mainPanel);

            this.Controls.Add(buttonPanel);

            this.CancelButton = btnCancel;

        }



        private void LoadWorkers()

        {

            var workers = workerRepository.GetAllWorkers()

                .Where(w => w.IsActive)

                .OrderBy(w => w.Username)

                .ToList();



            cmbWorker.Items.Clear();

            foreach (var worker in workers)

            {

                cmbWorker.Items.Add(new ComboBoxItem { Text = worker.Username, Value = worker.Id });

            }



            if (cmbWorker.Items.Count > 0)

                cmbWorker.SelectedIndex = 0;

        }



        private void CmbPeriod_SelectedIndexChanged(object sender, EventArgs e)

        {

            bool isCustom = cmbPeriod.SelectedIndex == 3;



            lblCustomDates.Visible = isCustom;

            dtpStartDate.Visible = isCustom;

            dtpEndDate.Visible = isCustom;

            ((Label)dtpStartDate.Tag).Visible = isCustom;

            ((Label)dtpEndDate.Tag).Visible = isCustom;



            if (isCustom)

            {

                // Set default custom range to last 7 days

                dtpEndDate.Value = DateTime.Now;

                dtpStartDate.Value = DateTime.Now.AddDays(-6);

            }

        }



        private void ChkIncludePayRate_CheckedChanged(object sender, EventArgs e)

        {

            numPayRate.Enabled = chkIncludePayRate.Checked;

            ((Label)numPayRate.Tag).Enabled = chkIncludePayRate.Checked;

        }



        private void BtnGenerate_Click(object sender, EventArgs e)

        {

            // Validation

            if (cmbWorker.SelectedItem == null)

            {

                MessageBox.Show("Please select a worker.", "Validation Error",

                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }



            try

            {

                btnGenerate.Enabled = false;

                btnGenerate.Text = "Generating...";

                this.Cursor = Cursors.WaitCursor;



                var selectedWorker = (ComboBoxItem)cmbWorker.SelectedItem;

                int workerId = selectedWorker.Value;



                // Determine date range

                DateTime startDate, endDate;

                WorkerHoursReportService.ReportPeriod period;



                switch (cmbPeriod.SelectedIndex)

                {

                    case 0: // This Week

                        period = WorkerHoursReportService.ReportPeriod.Weekly;

                        endDate = DateTime.Now.Date;

                        (startDate, endDate) = WorkerHoursReportService.CalculatePeriodRange(period, endDate);

                        break;

                    case 1: // Last 2 Weeks

                        period = WorkerHoursReportService.ReportPeriod.Biweekly;

                        endDate = DateTime.Now.Date;

                        (startDate, endDate) = WorkerHoursReportService.CalculatePeriodRange(period, endDate);

                        break;

                    case 2: // This Month

                        period = WorkerHoursReportService.ReportPeriod.Monthly;

                        endDate = DateTime.Now.Date;

                        (startDate, endDate) = WorkerHoursReportService.CalculatePeriodRange(period, endDate);

                        break;

                    case 3: // Custom

                        period = WorkerHoursReportService.ReportPeriod.Custom;

                        startDate = dtpStartDate.Value.Date;

                        endDate = dtpEndDate.Value.Date;



                        if (startDate > endDate)

                        {

                            MessageBox.Show("Start date must be before or equal to end date.", "Validation Error",

                                MessageBoxButtons.OK, MessageBoxIcon.Warning);

                            return;

                        }

                        break;

                    default:

                        MessageBox.Show("Please select a report period.", "Validation Error",

                            MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        return;

                }



                // Get pay rate if enabled

                decimal? hourlyRate = chkIncludePayRate.Checked ? numPayRate.Value : null;



                // Generate PDF

                var reportService = new WorkerHoursReportService();

                byte[] pdfData = reportService.GenerateReport(workerId, startDate, endDate, hourlyRate, period);



                // Save to Downloads folder

                string downloadsPath = Path.Combine(

                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),

                    "Downloads");



                if (!Directory.Exists(downloadsPath))

                    downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);



                string fileName = $"WorkerHours_{selectedWorker.Text}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.pdf";

                string filePath = Path.Combine(downloadsPath, fileName);



                // Handle file name collision

                int counter = 1;

                while (File.Exists(filePath))

                {

                    fileName = $"WorkerHours_{selectedWorker.Text}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}_{counter}.pdf";

                    filePath = Path.Combine(downloadsPath, fileName);

                    counter++;

                }



                File.WriteAllBytes(filePath, pdfData);



                // Show success message with option to open

                var result = MessageBox.Show(

                    $"Report generated successfully!\n\n" +

                    $"File saved to:\n{filePath}\n\n" +

                    $"Would you like to open the PDF now?",

                    "Success",

                    MessageBoxButtons.YesNo,

                    MessageBoxIcon.Information);



                if (result == DialogResult.Yes)

                {

                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo

                    {

                        FileName = filePath,

                        UseShellExecute = true

                    });

                }



                this.DialogResult = DialogResult.OK;

                this.Close();

            }

            catch (Exception ex)

            {

                MessageBox.Show($"Error generating report: {ex.Message}", "Error",

                    MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

            finally

            {

                btnGenerate.Enabled = true;

                btnGenerate.Text = "📄 Generate Report";

                this.Cursor = Cursors.Default;

            }

        }



        private class ComboBoxItem

        {

            public string Text { get; set; }

            public int Value { get; set; }



            public override string ToString()

            {

                return Text;

            }

        }

    }

}