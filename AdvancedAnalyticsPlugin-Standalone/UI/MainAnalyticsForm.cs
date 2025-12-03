using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ScottPlot.WinForms;
using ReactCRM.Plugins.AdvancedAnalytics.Engine;
using ReactCRM.Plugins.AdvancedAnalytics.Models;

namespace ReactCRM.Plugins.AdvancedAnalytics.UI
{
    /// <summary>
    /// Main analytics form with advanced data visualization and analysis
    /// </summary>
    public partial class MainAnalyticsForm : Form
    {
        private readonly DataQueryEngine _queryEngine;
        private readonly AnalyticsEngine _analyticsEngine;
        private ChartingEngine? _chartingEngine;
        private AnalyticsResult? _currentResult;

        // UI Controls
        private SplitContainer mainSplitContainer;
        private Panel leftPanel;
        private Panel rightPanel;
        private FormsPlot chartControl;
        private TabControl leftTabControl;
        private TabPage dataTabPage;
        private TabPage chartTabPage;
        private TabPage analysisTabPage;

        // Data tab controls
        private CheckedListBox fieldsCheckedListBox;
        private Label fieldsLabel;
        private Button selectAllFieldsButton;
        private Button clearFieldsButton;
        private GroupBox groupByGroupBox;
        private ComboBox groupByComboBox;
        private GroupBox aggregationGroupBox;
        private ComboBox aggregationComboBox;

        // Chart tab controls
        private GroupBox chartStyleGroupBox;
        private ComboBox chartStyleComboBox;
        private GroupBox chartTypeGroupBox;
        private ComboBox chartTypeComboBox;
        private CheckBox showLegendCheckBox;
        private CheckBox showGridCheckBox;
        private CheckBox showMovingAverageCheckBox;
        private NumericUpDown maPeriodNumeric;
        private CheckBox showTrendLineCheckBox;
        private TextBox chartTitleTextBox;

        // Analysis tab controls
        private CheckBox generateInsightsCheckBox;
        private CheckBox detectAnomaliesCheckBox;
        private CheckBox calculateRSICheckBox;
        private CheckBox calculateEMACheckBox;
        private ListBox insightsListBox;

        // Bottom panel controls
        private Panel bottomPanel;
        private Button executeButton;
        private Button exportPdfButton;
        private Button saveChartButton;
        private Label statusLabel;
        private ProgressBar progressBar;

        // Performance controls (in Data tab)
        private GroupBox performanceGroupBox;
        private Label maxRecordsLabel;
        private NumericUpDown maxRecordsNumeric;
        private Label maxChartPointsLabel;
        private NumericUpDown maxChartPointsNumeric;
        private CheckBox enableSamplingCheckBox;

        public MainAnalyticsForm()
        {
            _queryEngine = new DataQueryEngine();
            _analyticsEngine = new AnalyticsEngine();

            InitializeComponent();
            InitializeCustomComponents();
            LoadAvailableFields();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Advanced Analytics & Reporting";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.MinimumSize = new Size(1200, 700);

            // Main split container
            mainSplitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 350,
                FixedPanel = FixedPanel.Panel1,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Left panel (configuration)
            leftPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            // Right panel (chart)
            rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            // Chart control
            chartControl = new FormsPlot
            {
                Dock = DockStyle.Fill
            };

            // Left tab control
            leftTabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            // Tab pages
            dataTabPage = new TabPage("Datos");
            chartTabPage = new TabPage("Gráfica");
            analysisTabPage = new TabPage("Análisis");

            leftTabControl.TabPages.Add(dataTabPage);
            leftTabControl.TabPages.Add(chartTabPage);
            leftTabControl.TabPages.Add(analysisTabPage);

            // Bottom panel
            bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(250, 250, 250),
                Padding = new Padding(10)
            };

            // Add controls
            rightPanel.Controls.Add(chartControl);
            leftPanel.Controls.Add(leftTabControl);
            mainSplitContainer.Panel1.Controls.Add(leftPanel);
            mainSplitContainer.Panel2.Controls.Add(rightPanel);
            this.Controls.Add(mainSplitContainer);
            this.Controls.Add(bottomPanel);

            this.ResumeLayout();
        }

        private void InitializeCustomComponents()
        {
            InitializeDataTab();
            InitializeChartTab();
            InitializeAnalysisTab();
            InitializeBottomPanel();

            // Initialize charting engine
            _chartingEngine = new ChartingEngine(chartControl);
        }

        private void InitializeDataTab()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                AutoScroll = true
            };

            int yPosition = 10;

            // Fields selection
            fieldsLabel = new Label
            {
                Text = "Seleccionar Campos:",
                Location = new Point(10, yPosition),
                Size = new Size(300, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            panel.Controls.Add(fieldsLabel);
            yPosition += 25;

            // Select/Clear buttons
            var buttonPanel = new FlowLayoutPanel
            {
                Location = new Point(10, yPosition),
                Size = new Size(300, 30),
                FlowDirection = FlowDirection.LeftToRight
            };

            selectAllFieldsButton = new Button
            {
                Text = "Seleccionar Todos",
                Size = new Size(120, 25),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            selectAllFieldsButton.Click += SelectAllFieldsButton_Click;

            clearFieldsButton = new Button
            {
                Text = "Limpiar",
                Size = new Size(80, 25),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            clearFieldsButton.Click += ClearFieldsButton_Click;

            buttonPanel.Controls.Add(selectAllFieldsButton);
            buttonPanel.Controls.Add(clearFieldsButton);
            panel.Controls.Add(buttonPanel);
            yPosition += 35;

            fieldsCheckedListBox = new CheckedListBox
            {
                Location = new Point(10, yPosition),
                Size = new Size(300, 200),
                CheckOnClick = true
            };
            panel.Controls.Add(fieldsCheckedListBox);
            yPosition += 210;

            // Group By
            groupByGroupBox = new GroupBox
            {
                Text = "Agrupar Por",
                Location = new Point(10, yPosition),
                Size = new Size(300, 60)
            };

            groupByComboBox = new ComboBox
            {
                Location = new Point(10, 25),
                Size = new Size(280, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            groupByComboBox.Items.Add("(Sin agrupar)");
            groupByGroupBox.Controls.Add(groupByComboBox);
            panel.Controls.Add(groupByGroupBox);
            yPosition += 70;

            // Aggregation
            aggregationGroupBox = new GroupBox
            {
                Text = "Agregación",
                Location = new Point(10, yPosition),
                Size = new Size(300, 60)
            };

            aggregationComboBox = new ComboBox
            {
                Location = new Point(10, 25),
                Size = new Size(280, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            aggregationComboBox.Items.AddRange(new object[]
            {
                "Conteo",
                "Suma",
                "Promedio",
                "Mínimo",
                "Máximo",
                "Mediana"
            });
            aggregationComboBox.SelectedIndex = 0;
            aggregationGroupBox.Controls.Add(aggregationComboBox);
            panel.Controls.Add(aggregationGroupBox);
            yPosition += 70;

            // Performance Options
            performanceGroupBox = new GroupBox
            {
                Text = "⚡ Optimización de Rendimiento",
                Location = new Point(10, yPosition),
                Size = new Size(300, 150),
                ForeColor = Color.FromArgb(41, 128, 185)
            };

            var perfYPos = 25;

            enableSamplingCheckBox = new CheckBox
            {
                Text = "Sampling inteligente",
                Location = new Point(10, perfYPos),
                Size = new Size(280, 20),
                Checked = true
            };
            performanceGroupBox.Controls.Add(enableSamplingCheckBox);
            perfYPos += 25;

            maxRecordsLabel = new Label
            {
                Text = "Máx. registros a procesar:",
                Location = new Point(10, perfYPos),
                Size = new Size(180, 20)
            };
            performanceGroupBox.Controls.Add(maxRecordsLabel);

            maxRecordsNumeric = new NumericUpDown
            {
                Location = new Point(200, perfYPos),
                Size = new Size(80, 25),
                Minimum = 0,
                Maximum = 50000,
                Value = 5000,
                Increment = 500
            };
            performanceGroupBox.Controls.Add(maxRecordsNumeric);
            perfYPos += 30;

            var infoLabel1 = new Label
            {
                Text = "(0 = sin límite)",
                Location = new Point(200, perfYPos),
                Size = new Size(80, 15),
                Font = new Font("Segoe UI", 7, FontStyle.Italic),
                ForeColor = Color.Gray
            };
            performanceGroupBox.Controls.Add(infoLabel1);
            perfYPos += 20;

            maxChartPointsLabel = new Label
            {
                Text = "Máx. puntos en gráfica:",
                Location = new Point(10, perfYPos),
                Size = new Size(180, 20)
            };
            performanceGroupBox.Controls.Add(maxChartPointsLabel);

            maxChartPointsNumeric = new NumericUpDown
            {
                Location = new Point(200, perfYPos),
                Size = new Size(80, 25),
                Minimum = 100,
                Maximum = 10000,
                Value = 1000,
                Increment = 100
            };
            performanceGroupBox.Controls.Add(maxChartPointsNumeric);

            panel.Controls.Add(performanceGroupBox);

            dataTabPage.Controls.Add(panel);
        }

        private void InitializeChartTab()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                AutoScroll = true
            };

            int yPosition = 10;

            // Chart title
            var titleLabel = new Label
            {
                Text = "Título de la Gráfica:",
                Location = new Point(10, yPosition),
                Size = new Size(300, 20)
            };
            panel.Controls.Add(titleLabel);
            yPosition += 25;

            chartTitleTextBox = new TextBox
            {
                Location = new Point(10, yPosition),
                Size = new Size(300, 25),
                Text = "Análisis de Datos"
            };
            panel.Controls.Add(chartTitleTextBox);
            yPosition += 35;

            // Chart style
            chartStyleGroupBox = new GroupBox
            {
                Text = "Estilo Visual",
                Location = new Point(10, yPosition),
                Size = new Size(300, 60)
            };

            chartStyleComboBox = new ComboBox
            {
                Location = new Point(10, 25),
                Size = new Size(280, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            chartStyleComboBox.Items.AddRange(new object[]
            {
                "Profesional",
                "Mercado de Valores",
                "Científico",
                "Moderno",
                "Oscuro"
            });
            chartStyleComboBox.SelectedIndex = 1; // Stock Market style
            chartStyleGroupBox.Controls.Add(chartStyleComboBox);
            panel.Controls.Add(chartStyleGroupBox);
            yPosition += 70;

            // Chart type
            chartTypeGroupBox = new GroupBox
            {
                Text = "Tipo de Gráfica",
                Location = new Point(10, yPosition),
                Size = new Size(300, 60)
            };

            chartTypeComboBox = new ComboBox
            {
                Location = new Point(10, 25),
                Size = new Size(280, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            chartTypeComboBox.Items.AddRange(new object[]
            {
                "Línea",
                "Barra",
                "Área",
                "Dispersión"
            });
            chartTypeComboBox.SelectedIndex = 0;
            chartTypeGroupBox.Controls.Add(chartTypeComboBox);
            panel.Controls.Add(chartTypeGroupBox);
            yPosition += 70;

            // Options
            showLegendCheckBox = new CheckBox
            {
                Text = "Mostrar leyenda",
                Location = new Point(10, yPosition),
                Size = new Size(300, 20),
                Checked = true
            };
            panel.Controls.Add(showLegendCheckBox);
            yPosition += 25;

            showGridCheckBox = new CheckBox
            {
                Text = "Mostrar cuadrícula",
                Location = new Point(10, yPosition),
                Size = new Size(300, 20),
                Checked = true
            };
            panel.Controls.Add(showGridCheckBox);
            yPosition += 25;

            showTrendLineCheckBox = new CheckBox
            {
                Text = "Mostrar línea de tendencia",
                Location = new Point(10, yPosition),
                Size = new Size(300, 20),
                Checked = false
            };
            panel.Controls.Add(showTrendLineCheckBox);
            yPosition += 30;

            // Moving average
            showMovingAverageCheckBox = new CheckBox
            {
                Text = "Promedio móvil (período):",
                Location = new Point(10, yPosition),
                Size = new Size(180, 20),
                Checked = false
            };
            panel.Controls.Add(showMovingAverageCheckBox);

            maPeriodNumeric = new NumericUpDown
            {
                Location = new Point(200, yPosition),
                Size = new Size(60, 25),
                Minimum = 2,
                Maximum = 100,
                Value = 20
            };
            panel.Controls.Add(maPeriodNumeric);

            chartTabPage.Controls.Add(panel);
        }

        private void InitializeAnalysisTab()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                AutoScroll = true
            };

            int yPosition = 10;

            var label = new Label
            {
                Text = "Opciones de Análisis Avanzado:",
                Location = new Point(10, yPosition),
                Size = new Size(300, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            panel.Controls.Add(label);
            yPosition += 30;

            generateInsightsCheckBox = new CheckBox
            {
                Text = "Generar insights automáticos",
                Location = new Point(10, yPosition),
                Size = new Size(300, 20),
                Checked = true
            };
            panel.Controls.Add(generateInsightsCheckBox);
            yPosition += 25;

            detectAnomaliesCheckBox = new CheckBox
            {
                Text = "Detectar anomalías",
                Location = new Point(10, yPosition),
                Size = new Size(300, 20),
                Checked = true
            };
            panel.Controls.Add(detectAnomaliesCheckBox);
            yPosition += 25;

            calculateRSICheckBox = new CheckBox
            {
                Text = "Calcular RSI (Índice de Fuerza Relativa)",
                Location = new Point(10, yPosition),
                Size = new Size(300, 20),
                Checked = false
            };
            panel.Controls.Add(calculateRSICheckBox);
            yPosition += 25;

            calculateEMACheckBox = new CheckBox
            {
                Text = "Calcular EMA (Media Móvil Exponencial)",
                Location = new Point(10, yPosition),
                Size = new Size(300, 20),
                Checked = false
            };
            panel.Controls.Add(calculateEMACheckBox);
            yPosition += 35;

            var insightsLabel = new Label
            {
                Text = "Insights Generados:",
                Location = new Point(10, yPosition),
                Size = new Size(300, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            panel.Controls.Add(insightsLabel);
            yPosition += 25;

            insightsListBox = new ListBox
            {
                Location = new Point(10, yPosition),
                Size = new Size(300, 250)
            };
            panel.Controls.Add(insightsListBox);

            analysisTabPage.Controls.Add(panel);
        }

        private void InitializeBottomPanel()
        {
            executeButton = new Button
            {
                Text = "🔍 Ejecutar Análisis",
                Location = new Point(10, 15),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            executeButton.Click += ExecuteButton_Click;
            bottomPanel.Controls.Add(executeButton);

            saveChartButton = new Button
            {
                Text = "💾 Guardar Gráfica",
                Location = new Point(170, 15),
                Size = new Size(140, 35),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            saveChartButton.Click += SaveChartButton_Click;
            bottomPanel.Controls.Add(saveChartButton);

            exportPdfButton = new Button
            {
                Text = "📄 Exportar PDF",
                Location = new Point(320, 15),
                Size = new Size(140, 35),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            exportPdfButton.Click += ExportPdfButton_Click;
            bottomPanel.Controls.Add(exportPdfButton);

            statusLabel = new Label
            {
                Text = "Listo. Seleccione campos y ejecute el análisis.",
                Location = new Point(470, 5),
                Size = new Size(600, 20),
                ForeColor = Color.FromArgb(100, 100, 100)
            };
            bottomPanel.Controls.Add(statusLabel);

            progressBar = new ProgressBar
            {
                Location = new Point(470, 30),
                Size = new Size(600, 20),
                Style = ProgressBarStyle.Continuous,
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Visible = false
            };
            bottomPanel.Controls.Add(progressBar);
        }

        private void LoadAvailableFields()
        {
            try
            {
                var fields = _queryEngine.GetAvailableFields();

                fieldsCheckedListBox.Items.Clear();
                groupByComboBox.Items.Clear();
                groupByComboBox.Items.Add("(Sin agrupar)");

                foreach (var field in fields)
                {
                    string displayText = field.IsCustomField
                        ? $"📝 {field.DisplayName}"
                        : $"📊 {field.DisplayName}";

                    fieldsCheckedListBox.Items.Add(new FieldItem(field, displayText));
                    groupByComboBox.Items.Add(new FieldItem(field, field.DisplayName));
                }

                groupByComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar campos: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void SelectAllFieldsButton_Click(object? sender, EventArgs e)
        {
            for (int i = 0; i < fieldsCheckedListBox.Items.Count; i++)
            {
                fieldsCheckedListBox.SetItemChecked(i, true);
            }
        }

        private void ClearFieldsButton_Click(object? sender, EventArgs e)
        {
            for (int i = 0; i < fieldsCheckedListBox.Items.Count; i++)
            {
                fieldsCheckedListBox.SetItemChecked(i, false);
            }
        }

        private async void ExecuteButton_Click(object? sender, EventArgs e)
        {
            try
            {
                // Show progress bar
                progressBar.Value = 0;
                progressBar.Visible = true;

                statusLabel.Text = "Ejecutando consulta...";
                statusLabel.ForeColor = Color.Blue;
                executeButton.Enabled = false;
                this.Cursor = Cursors.WaitCursor;

                // Configure query engine with performance settings
                _queryEngine.MaxRecords = (int)maxRecordsNumeric.Value;
                _queryEngine.MaxChartPoints = (int)maxChartPointsNumeric.Value;
                _queryEngine.EnableSmartSampling = enableSamplingCheckBox.Checked;

                // Set up progress callback
                _queryEngine.ProgressCallback = (percentage, message) =>
                {
                    if (progressBar.InvokeRequired)
                    {
                        progressBar.Invoke(new Action(() =>
                        {
                            progressBar.Value = Math.Min(percentage, 100);
                            statusLabel.Text = message;
                        }));
                    }
                    else
                    {
                        progressBar.Value = Math.Min(percentage, 100);
                        statusLabel.Text = message;
                    }
                };

                // Build query configuration
                var config = BuildQueryConfiguration();

                if (config.SelectedFields.Count == 0)
                {
                    MessageBox.Show(
                        "Por favor seleccione al menos un campo para analizar.",
                        "Validación",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                // Execute query asynchronously
                _currentResult = await System.Threading.Tasks.Task.Run(() => _queryEngine.ExecuteQuery(config));

                // Apply advanced analytics
                if (calculateRSICheckBox.Checked && _currentResult.Series.Any())
                {
                    var rsi = _analyticsEngine.CalculateRSI(_currentResult.Series[0]);
                    _currentResult.Series.Add(rsi);
                }

                if (calculateEMACheckBox.Checked && _currentResult.Series.Any())
                {
                    var ema = _analyticsEngine.CalculateEMA(_currentResult.Series[0], 20);
                    _currentResult.Series.Add(ema);
                }

                // Generate insights
                if (generateInsightsCheckBox.Checked)
                {
                    _currentResult.Insights = _analyticsEngine.GenerateInsights(_currentResult);
                    DisplayInsights(_currentResult.Insights);
                }

                // Render chart
                RenderChart();

                // Build status message with sampling info
                var statusMessage = $"✓ Análisis completado: {_currentResult.TotalRecordsAnalyzed} registros procesados en {_currentResult.ExecutionTime.TotalMilliseconds:F2}ms";

                // Add sampling info if applicable
                var sampledSeries = _currentResult.Series.Where(s => s.Metadata.ContainsKey("Sampled") && (bool)s.Metadata["Sampled"]).ToList();
                if (sampledSeries.Any())
                {
                    var totalOriginal = sampledSeries.Sum(s => (int)s.Metadata["OriginalCount"]);
                    var totalSampled = sampledSeries.Sum(s => (int)s.Metadata["SampledCount"]);
                    statusMessage += $" | Optimizado: {totalOriginal} → {totalSampled} puntos ({(100.0 * totalSampled / totalOriginal):F1}%)";
                }

                statusLabel.Text = statusMessage;
                statusLabel.ForeColor = Color.Green;
                progressBar.Value = 100;

                saveChartButton.Enabled = true;
                exportPdfButton.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al ejecutar análisis:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                statusLabel.Text = "Error al ejecutar análisis.";
                statusLabel.ForeColor = Color.Red;
                progressBar.Visible = false;
            }
            finally
            {
                executeButton.Enabled = true;
                this.Cursor = Cursors.Default;

                // Hide progress bar after a short delay
                System.Threading.Tasks.Task.Delay(2000).ContinueWith(_ =>
                {
                    if (progressBar.InvokeRequired)
                    {
                        progressBar.Invoke(new Action(() => progressBar.Visible = false));
                    }
                    else
                    {
                        progressBar.Visible = false;
                    }
                });
            }
        }

        private QueryConfiguration BuildQueryConfiguration()
        {
            var config = new QueryConfiguration();

            // Get selected fields
            foreach (FieldItem item in fieldsCheckedListBox.CheckedItems)
            {
                config.SelectedFields.Add(item.Field);
            }

            // Group by
            if (groupByComboBox.SelectedIndex > 0 && groupByComboBox.SelectedItem is FieldItem groupField)
            {
                config.GroupByField = groupField.Field.FieldName;
            }

            // Aggregation
            config.Aggregation = aggregationComboBox.SelectedIndex switch
            {
                0 => AggregationFunction.Count,
                1 => AggregationFunction.Sum,
                2 => AggregationFunction.Average,
                3 => AggregationFunction.Min,
                4 => AggregationFunction.Max,
                5 => AggregationFunction.Median,
                _ => AggregationFunction.Count
            };

            return config;
        }

        private void RenderChart()
        {
            if (_currentResult == null || _chartingEngine == null)
                return;

            var chartConfig = new ChartConfiguration
            {
                Title = chartTitleTextBox.Text,
                XAxisLabel = "Categoría",
                YAxisLabel = "Valor",
                Style = chartStyleComboBox.SelectedIndex switch
                {
                    0 => ChartStyle.Professional,
                    1 => ChartStyle.StockMarket,
                    2 => ChartStyle.Scientific,
                    3 => ChartStyle.Modern,
                    4 => ChartStyle.Dark,
                    _ => ChartStyle.StockMarket
                },
                ShowLegend = showLegendCheckBox.Checked,
                ShowGrid = showGridCheckBox.Checked,
                Series = _currentResult.Series
            };

            // Set series type based on selection
            var selectedType = chartTypeComboBox.SelectedIndex switch
            {
                0 => SeriesType.Line,
                1 => SeriesType.Bar,
                2 => SeriesType.Area,
                3 => SeriesType.Scatter,
                _ => SeriesType.Line
            };

            foreach (var series in chartConfig.Series.Where(s => !s.Name.Contains("RSI") && !s.Name.Contains("EMA")))
            {
                series.Type = selectedType;
            }

            _chartingEngine.RenderChart(chartConfig);

            // Add moving average if requested
            if (showMovingAverageCheckBox.Checked && _currentResult.Series.Any())
            {
                _chartingEngine.AddMovingAverage(_currentResult.Series[0], (int)maPeriodNumeric.Value);
            }

            // Add trend line if requested
            if (showTrendLineCheckBox.Checked && _currentResult.Series.Any())
            {
                _chartingEngine.AddTrendLine(_currentResult.Series[0]);
            }
        }

        private void DisplayInsights(List<Insight> insights)
        {
            insightsListBox.Items.Clear();

            foreach (var insight in insights)
            {
                string severity = insight.Severity switch
                {
                    InsightSeverity.Critical => "🔴",
                    InsightSeverity.Warning => "🟡",
                    InsightSeverity.Positive => "🟢",
                    _ => "🔵"
                };

                insightsListBox.Items.Add($"{severity} {insight.Title}");
            }
        }

        private void SaveChartButton_Click(object? sender, EventArgs e)
        {
            if (_chartingEngine == null)
                return;

            try
            {
                using var saveDialog = new SaveFileDialog
                {
                    Filter = "PNG Image|*.png|JPEG Image|*.jpg",
                    Title = "Guardar Gráfica",
                    FileName = $"chart_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    _chartingEngine.SaveToFile(saveDialog.FileName, 1920, 1080);
                    MessageBox.Show("Gráfica guardada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar gráfica: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportPdfButton_Click(object? sender, EventArgs e)
        {
            if (_currentResult == null || _chartingEngine == null)
                return;

            try
            {
                using var saveDialog = new SaveFileDialog
                {
                    Filter = "PDF Document|*.pdf",
                    Title = "Exportar Reporte PDF",
                    FileName = $"reporte_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    statusLabel.Text = "Generando reporte PDF...";

                    var chartConfig = new ChartConfiguration
                    {
                        Title = chartTitleTextBox.Text,
                        Series = _currentResult.Series
                    };

                    var pdfGenerator = new PdfReportGenerator(_currentResult, chartConfig);

                    // Add chart image
                    var chartImage = _chartingEngine.GetChartBitmap(1200, 800);
                    pdfGenerator.AddChartImage(chartImage);

                    // Generate PDF
                    pdfGenerator.GenerateReport(saveDialog.FileName);

                    this.Cursor = Cursors.Default;
                    statusLabel.Text = "Reporte PDF generado exitosamente.";

                    MessageBox.Show("Reporte PDF generado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Ask if user wants to open the file
                    if (MessageBox.Show("¿Desea abrir el archivo PDF?", "Abrir PDF", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = saveDialog.FileName,
                            UseShellExecute = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show($"Error al generar PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Helper class for field items
        private class FieldItem
        {
            public FieldSelection Field { get; }
            public string DisplayText { get; }

            public FieldItem(FieldSelection field, string displayText)
            {
                Field = field;
                DisplayText = displayText;
            }

            public override string ToString() => DisplayText;
        }
    }
}
