using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using ReactCRM.Database;
using ReactCRM.Services;

namespace ReactCRM.UI.Charts
{
    public partial class ChartBuilderForm : Form
    {
        private Panel panelControls;
        private Panel panelChart;
        private ComboBox cboChartType;
        private Button btnGenerate;
        private Button btnClose;
        private Label lblTitle;
        private ClientRepository clientRepository;

        public ChartBuilderForm()
        {
            clientRepository = new ClientRepository();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Analytics & Charts";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(800, 600);

            // Header Panel
            var panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(52, 73, 94),
                Padding = new Padding(15)
            };

            lblTitle = new Label
            {
                Text = "Analytics Dashboard",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 10),
                AutoSize = true
            };

            var lblSubtitle = new Label
            {
                Text = "Visualize your CRM data",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(189, 195, 199),
                Location = new Point(15, 40),
                AutoSize = true
            };

            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(lblSubtitle);

            // Controls Panel
            panelControls = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(236, 240, 241),
                Padding = new Padding(15)
            };

            var lblChartType = new Label
            {
                Text = "Select Chart Type:",
                Location = new Point(15, 18),
                AutoSize = true,
                Font = new Font("Segoe UI", 10)
            };

            cboChartType = new ComboBox
            {
                Location = new Point(140, 15),
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cboChartType.Items.AddRange(new object[] {
                "Clients by Creation Date",
                "Activity Timeline",
                "Custom Fields Usage",
                "Monthly Growth",
                "Data Quality Report"
            });
            cboChartType.SelectedIndex = 0;

            btnGenerate = new Button
            {
                Text = "Generate Chart",
                Location = new Point(400, 12),
                Size = new Size(130, 35),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnGenerate.Click += BtnGenerate_Click;

            panelControls.Controls.Add(lblChartType);
            panelControls.Controls.Add(cboChartType);
            panelControls.Controls.Add(btnGenerate);

            // Chart Panel (Main Display Area)
            panelChart = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20),
                AutoScroll = true
            };

            // Bottom Panel
            var panelBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(236, 240, 241),
                Padding = new Padding(10)
            };

            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(850, 12),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(127, 140, 141),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnClose.Click += (s, e) => this.Close();

            panelBottom.Controls.Add(btnClose);

            // Add controls to form
            this.Controls.Add(panelChart);
            this.Controls.Add(panelControls);
            this.Controls.Add(panelHeader);
            this.Controls.Add(panelBottom);

            // Show initial chart
            GenerateChart();
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            GenerateChart();
        }

        private void GenerateChart()
        {
            panelChart.Controls.Clear();

            try
            {
                string chartType = cboChartType.SelectedItem?.ToString();

                switch (chartType)
                {
                    case "Clients by Creation Date":
                        ShowClientsByCreationDate();
                        break;

                    case "Activity Timeline":
                        ShowActivityTimeline();
                        break;

                    case "Custom Fields Usage":
                        ShowCustomFieldsUsage();
                        break;

                    case "Monthly Growth":
                        ShowMonthlyGrowth();
                        break;

                    case "Data Quality Report":
                        ShowDataQualityReport();
                        break;

                    default:
                        ShowSummaryStats();
                        break;
                }

                AuditService.LogAction("View", "Analytics", null,
                    $"Viewed chart: {chartType}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating chart: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowSummaryStats()
        {
            var stats = GetSummaryStatistics();

            int yPos = 20;
            int cardWidth = 250;
            int cardHeight = 120;
            int spacing = 20;
            int xPos = 20;

            // Total Clients Card
            var clientsCard = CreateStatCard("Total Clients", stats.TotalClients.ToString(),
                "All clients in the system", Color.FromArgb(52, 152, 219),
                xPos, yPos, cardWidth, cardHeight);
            panelChart.Controls.Add(clientsCard);
            xPos += cardWidth + spacing;

            // This Month Card
            var monthCard = CreateStatCard("This Month", stats.ClientsThisMonth.ToString(),
                "Clients added this month", Color.FromArgb(46, 204, 113),
                xPos, yPos, cardWidth, cardHeight);
            panelChart.Controls.Add(monthCard);
            xPos += cardWidth + spacing;

            // This Week Card
            var weekCard = CreateStatCard("This Week", stats.ClientsThisWeek.ToString(),
                "Clients added this week", Color.FromArgb(241, 196, 15),
                xPos, yPos, cardWidth, cardHeight);
            panelChart.Controls.Add(weekCard);

            // Data Completeness
            yPos += cardHeight + spacing;
            xPos = 20;

            var completenessCard = CreateStatCard("Data Quality", $"{stats.DataCompleteness:F1}%",
                "Average data completeness", Color.FromArgb(155, 89, 182),
                xPos, yPos, cardWidth, cardHeight);
            panelChart.Controls.Add(completenessCard);
        }

        private void ShowClientsByCreationDate()
        {
            var data = GetClientsByDate();

            // Create a simple bar chart representation
            int yPos = 20;
            int maxWidth = 700;
            int maxValue = data.Values.Count > 0 ? data.Values.Max() : 1;

            var lblChartTitle = new Label
            {
                Text = "Clients by Creation Date (Last 30 Days)",
                Location = new Point(20, yPos),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true
            };
            panelChart.Controls.Add(lblChartTitle);
            yPos += 40;

            foreach (var item in data)
            {
                int barWidth = maxValue > 0 ? (item.Value * maxWidth) / maxValue : 0;

                var lblDate = new Label
                {
                    Text = item.Key.ToString("MMM dd"),
                    Location = new Point(20, yPos + 2),
                    Width = 80,
                    Font = new Font("Segoe UI", 9)
                };

                var pnlBar = new Panel
                {
                    Location = new Point(110, yPos),
                    Size = new Size(barWidth, 20),
                    BackColor = Color.FromArgb(52, 152, 219)
                };

                var lblValue = new Label
                {
                    Text = item.Value.ToString(),
                    Location = new Point(120 + barWidth, yPos + 2),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                };

                panelChart.Controls.Add(lblDate);
                panelChart.Controls.Add(pnlBar);
                panelChart.Controls.Add(lblValue);

                yPos += 30;
            }
        }

        private void ShowActivityTimeline()
        {
            var activities = GetRecentActivities();

            int yPos = 20;

            var lblTitle = new Label
            {
                Text = "Recent Activity Timeline",
                Location = new Point(20, yPos),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true
            };
            panelChart.Controls.Add(lblTitle);
            yPos += 40;

            foreach (var activity in activities)
            {
                var pnlActivity = new Panel
                {
                    Location = new Point(20, yPos),
                    Size = new Size(800, 60),
                    BackColor = Color.FromArgb(236, 240, 241),
                    BorderStyle = BorderStyle.FixedSingle
                };

                var lblTime = new Label
                {
                    Text = activity.Timestamp.ToString("HH:mm"),
                    Location = new Point(10, 10),
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    AutoSize = true
                };

                var lblAction = new Label
                {
                    Text = activity.ActionType,
                    Location = new Point(10, 30),
                    Font = new Font("Segoe UI", 8),
                    ForeColor = Color.Gray,
                    AutoSize = true
                };

                var lblDescription = new Label
                {
                    Text = activity.Description,
                    Location = new Point(80, 10),
                    Width = 600,
                    Font = new Font("Segoe UI", 9),
                    AutoSize = false,
                    Height = 40
                };

                var lblUser = new Label
                {
                    Text = activity.User,
                    Location = new Point(700, 10),
                    Font = new Font("Segoe UI", 8, FontStyle.Italic),
                    ForeColor = Color.Gray,
                    AutoSize = true
                };

                pnlActivity.Controls.Add(lblTime);
                pnlActivity.Controls.Add(lblAction);
                pnlActivity.Controls.Add(lblDescription);
                pnlActivity.Controls.Add(lblUser);

                panelChart.Controls.Add(pnlActivity);
                yPos += 70;
            }
        }

        private void ShowCustomFieldsUsage()
        {
            var usage = GetCustomFieldsUsage();

            int yPos = 20;

            var lblTitle = new Label
            {
                Text = "Custom Fields Usage Statistics",
                Location = new Point(20, yPos),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true
            };
            panelChart.Controls.Add(lblTitle);
            yPos += 40;

            if (usage.Count == 0)
            {
                var lblNoData = new Label
                {
                    Text = "No custom fields defined yet.",
                    Location = new Point(20, yPos),
                    Font = new Font("Segoe UI", 10, FontStyle.Italic),
                    ForeColor = Color.Gray,
                    AutoSize = true
                };
                panelChart.Controls.Add(lblNoData);
                return;
            }

            int totalClients = clientRepository.GetAllClients().Count;
            int maxWidth = 600;

            foreach (var item in usage)
            {
                int percentage = totalClients > 0 ? (item.Value * 100) / totalClients : 0;
                int barWidth = (percentage * maxWidth) / 100;

                var lblField = new Label
                {
                    Text = item.Key,
                    Location = new Point(20, yPos + 2),
                    Width = 200,
                    Font = new Font("Segoe UI", 9)
                };

                var pnlBar = new Panel
                {
                    Location = new Point(230, yPos),
                    Size = new Size(barWidth, 20),
                    BackColor = Color.FromArgb(46, 204, 113)
                };

                var lblPercentage = new Label
                {
                    Text = $"{percentage}% ({item.Value}/{totalClients})",
                    Location = new Point(240 + barWidth, yPos + 2),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9)
                };

                panelChart.Controls.Add(lblField);
                panelChart.Controls.Add(pnlBar);
                panelChart.Controls.Add(lblPercentage);

                yPos += 30;
            }
        }

        private void ShowMonthlyGrowth()
        {
            var growth = GetMonthlyGrowth();

            int yPos = 20;

            var lblTitle = new Label
            {
                Text = "Monthly Client Growth (Last 12 Months)",
                Location = new Point(20, yPos),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true
            };
            panelChart.Controls.Add(lblTitle);
            yPos += 40;

            int maxWidth = 600;
            int maxValue = growth.Values.Count > 0 ? growth.Values.Max() : 1;

            foreach (var item in growth)
            {
                int barWidth = maxValue > 0 ? (item.Value * maxWidth) / maxValue : 0;

                var lblMonth = new Label
                {
                    Text = item.Key,
                    Location = new Point(20, yPos + 2),
                    Width = 100,
                    Font = new Font("Segoe UI", 9)
                };

                var pnlBar = new Panel
                {
                    Location = new Point(130, yPos),
                    Size = new Size(barWidth, 25),
                    BackColor = Color.FromArgb(155, 89, 182)
                };

                var lblValue = new Label
                {
                    Text = $"{item.Value} clients",
                    Location = new Point(140 + barWidth, yPos + 4),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9)
                };

                panelChart.Controls.Add(lblMonth);
                panelChart.Controls.Add(pnlBar);
                panelChart.Controls.Add(lblValue);

                yPos += 35;
            }
        }

        private void ShowDataQualityReport()
        {
            var quality = GetDataQualityMetrics();

            int yPos = 20;

            var lblTitle = new Label
            {
                Text = "Data Quality Report",
                Location = new Point(20, yPos),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true
            };
            panelChart.Controls.Add(lblTitle);
            yPos += 40;

            // Create quality metrics
            CreateQualityMetric("Complete Records", quality.CompleteRecords, quality.TotalRecords, yPos);
            yPos += 80;

            CreateQualityMetric("Records with Email", quality.WithEmail, quality.TotalRecords, yPos);
            yPos += 80;

            CreateQualityMetric("Records with Phone", quality.WithPhone, quality.TotalRecords, yPos);
            yPos += 80;

            CreateQualityMetric("Records with DOB", quality.WithDOB, quality.TotalRecords, yPos);
        }

        private void CreateQualityMetric(string label, int value, int total, int yPos)
        {
            int percentage = total > 0 ? (value * 100) / total : 0;
            Color color = percentage >= 80 ? Color.FromArgb(46, 204, 113) :
                         percentage >= 50 ? Color.FromArgb(241, 196, 15) :
                         Color.FromArgb(231, 76, 60);

            var lblMetric = new Label
            {
                Text = label,
                Location = new Point(20, yPos),
                Width = 200,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            var pnlProgress = new Panel
            {
                Location = new Point(230, yPos),
                Size = new Size(500, 30),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(236, 240, 241)
            };

            var pnlFill = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size((percentage * 500) / 100, 30),
                BackColor = color
            };

            var lblPercentage = new Label
            {
                Text = $"{percentage}%",
                Location = new Point(10, 5),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };

            pnlFill.Controls.Add(lblPercentage);
            pnlProgress.Controls.Add(pnlFill);

            var lblCount = new Label
            {
                Text = $"{value} / {total}",
                Location = new Point(740, yPos + 5),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };

            panelChart.Controls.Add(lblMetric);
            panelChart.Controls.Add(pnlProgress);
            panelChart.Controls.Add(lblCount);
        }

        private Panel CreateStatCard(string title, string value, string subtitle, Color color, int x, int y, int width, int height)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = color,
                BorderStyle = BorderStyle.None
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Location = new Point(15, 15),
                AutoSize = true
            };

            var lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 40),
                AutoSize = true
            };

            var lblSubtitle = new Label
            {
                Text = subtitle,
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(220, 220, 220),
                Location = new Point(15, 90),
                Width = width - 30,
                AutoSize = false
            };

            card.Controls.Add(lblTitle);
            card.Controls.Add(lblValue);
            card.Controls.Add(lblSubtitle);

            return card;
        }

        // Data retrieval methods
        private SummaryStats GetSummaryStatistics()
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();

                var stats = new SummaryStats();
                stats.TotalClients = DbConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM Clients");

                var monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                stats.ClientsThisMonth = DbConnection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Clients WHERE CreatedAt >= @date",
                    new SqliteParameter("@date", monthStart.ToString("yyyy-MM-dd")));

                var weekStart = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek);
                stats.ClientsThisWeek = DbConnection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Clients WHERE CreatedAt >= @date",
                    new SqliteParameter("@date", weekStart.ToString("yyyy-MM-dd")));

                // Calculate data completeness
                int withEmail = DbConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM Clients WHERE Email IS NOT NULL AND Email != ''");
                int withPhone = DbConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM Clients WHERE Phone IS NOT NULL AND Phone != ''");
                int withDOB = DbConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM Clients WHERE DOB IS NOT NULL");

                if (stats.TotalClients > 0)
                {
                    stats.DataCompleteness = ((withEmail + withPhone + withDOB) * 100.0) / (stats.TotalClients * 3);
                }

                return stats;
            }
        }

        private Dictionary<DateTime, int> GetClientsByDate()
        {
            var result = new Dictionary<DateTime, int>();
            var startDate = DateTime.Now.AddDays(-30);

            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                string sql = @"
                    SELECT date(CreatedAt) as Date, COUNT(*) as Count
                    FROM Clients
                    WHERE CreatedAt >= @startDate
                    GROUP BY date(CreatedAt)
                    ORDER BY date(CreatedAt)";

                using (var cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@startDate", startDate.ToString("yyyy-MM-dd"));

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime date = DateTime.Parse(reader["Date"].ToString());
                            int count = Convert.ToInt32(reader["Count"]);
                            result[date] = count;
                        }
                    }
                }
            }

            return result;
        }

        private List<Models.AuditLog> GetRecentActivities()
        {
            return AuditService.GetAuditLogs(DateTime.Today, DateTime.Today.AddDays(1))
                .OrderByDescending(a => a.Timestamp)
                .Take(10)
                .ToList();
        }

        private Dictionary<string, int> GetCustomFieldsUsage()
        {
            var result = new Dictionary<string, int>();
            var customFields = new CustomFieldRepository().GetAll();

            foreach (var field in customFields)
            {
                int count = DbConnection.ExecuteScalar<int>(
                    $"SELECT COUNT(*) FROM Clients WHERE json_extract(ExtraData, '$.{field.FieldName}') IS NOT NULL");
                result[field.Label] = count;
            }

            return result;
        }

        private Dictionary<string, int> GetMonthlyGrowth()
        {
            var result = new Dictionary<string, int>();

            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                string sql = @"
                    SELECT strftime('%Y-%m', CreatedAt) as Month, COUNT(*) as Count
                    FROM Clients
                    WHERE CreatedAt >= date('now', '-12 months')
                    GROUP BY strftime('%Y-%m', CreatedAt)
                    ORDER BY Month";

                using (var cmd = new SqliteCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string month = reader["Month"].ToString();
                        int count = Convert.ToInt32(reader["Count"]);
                        result[month] = count;
                    }
                }
            }

            return result;
        }

        private DataQualityMetrics GetDataQualityMetrics()
        {
            var metrics = new DataQualityMetrics();
            metrics.TotalRecords = DbConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM Clients");
            metrics.WithEmail = DbConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM Clients WHERE Email IS NOT NULL AND Email != ''");
            metrics.WithPhone = DbConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM Clients WHERE Phone IS NOT NULL AND Phone != ''");
            metrics.WithDOB = DbConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM Clients WHERE DOB IS NOT NULL");

            metrics.CompleteRecords = DbConnection.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM Clients WHERE Email IS NOT NULL AND Email != '' AND Phone IS NOT NULL AND Phone != '' AND DOB IS NOT NULL");

            return metrics;
        }

        // Helper classes
        private class SummaryStats
        {
            public int TotalClients { get; set; }
            public int ClientsThisMonth { get; set; }
            public int ClientsThisWeek { get; set; }
            public double DataCompleteness { get; set; }
        }

        private class DataQualityMetrics
        {
            public int TotalRecords { get; set; }
            public int CompleteRecords { get; set; }
            public int WithEmail { get; set; }
            public int WithPhone { get; set; }
            public int WithDOB { get; set; }
        }
    }
}