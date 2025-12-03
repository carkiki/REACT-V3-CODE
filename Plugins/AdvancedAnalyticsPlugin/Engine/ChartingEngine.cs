using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WinForms;
using ReactCRM.Plugins.AdvancedAnalytics.Models;
using DrawingColor = System.Drawing.Color;

namespace ReactCRM.Plugins.AdvancedAnalytics.Engine
{
    /// <summary>
    /// Advanced charting engine with stock-market style visualizations
    /// </summary>
    public class ChartingEngine
    {
        private readonly Plot _plot;
        private readonly FormsPlot? _formsPlot;

        public ChartingEngine()
        {
            _plot = new Plot();
        }

        public ChartingEngine(FormsPlot formsPlot)
        {
            _formsPlot = formsPlot;
            _plot = formsPlot.Plot;
        }

        /// <summary>
        /// Renders a complete chart based on configuration
        /// </summary>
        public void RenderChart(ChartConfiguration config)
        {
            // Clear existing plot
            _plot.Clear();

            // Apply style theme
            ApplyStyle(config.Style);

            // Set title and labels
            _plot.Title(config.Title);
            _plot.XLabel(config.XAxisLabel);
            _plot.YLabel(config.YAxisLabel);

            // Render each data series
            foreach (var series in config.Series)
            {
                RenderSeries(series);
            }

            // Apply advanced features
            if (config.ShowGrid)
            {
                _plot.Grid.MajorLineColor = ScottPlot.Color.FromColor(DrawingColor.FromArgb(50, config.GridColor));
                _plot.Grid.MinorLineColor = ScottPlot.Color.FromColor(DrawingColor.FromArgb(25, config.GridColor));
            }
            else
            {
                _plot.HideGrid();
            }

            if (config.ShowLegend && config.Series.Count > 1)
            {
                _plot.ShowLegend();
            }

            // Enable interactivity if using FormsPlot
            if (_formsPlot != null)
            {
                _formsPlot.Interaction.Enable(config.EnableZoom);
            }

            // Refresh the plot
            _formsPlot?.Refresh();
        }

        /// <summary>
        /// Renders a single data series
        /// </summary>
        private void RenderSeries(DataSeries series)
        {
            if (!series.Points.Any())
                return;

            var color = ParseColor(series.Color);

            switch (series.Type)
            {
                case SeriesType.Line:
                    RenderLineSeries(series, color);
                    break;

                case SeriesType.Bar:
                    RenderBarSeries(series, color);
                    break;

                case SeriesType.Area:
                    RenderAreaSeries(series, color);
                    break;

                case SeriesType.Scatter:
                    RenderScatterSeries(series, color);
                    break;

                case SeriesType.Candlestick:
                    RenderCandlestickSeries(series, color);
                    break;
            }
        }

        /// <summary>
        /// Renders line chart (ideal for trends, time series)
        /// </summary>
        private void RenderLineSeries(DataSeries series, ScottPlot.Color color)
        {
            double[] values = series.Points.Select(p => p.Value).ToArray();
            double[] positions = Enumerable.Range(0, series.Points.Count).Select(i => (double)i).ToArray();

            var line = _plot.Add.SignalXY(positions, values);
            line.Color = color;
            line.LineWidth = 2;
            line.LegendText = series.Name;
            line.MarkerSize = 0; // No markers for cleaner look

            // Add tick labels
            string[] labels = series.Points.Select(p => p.Label).ToArray();
            if (labels.Length > 0)
            {
                double[] tickPositions = Enumerable.Range(0, labels.Length).Select(i => (double)i).ToArray();
                _plot.Axes.Bottom.SetTicks(tickPositions, labels);
                _plot.Axes.Bottom.TickLabelStyle.Rotation = 45;
                _plot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleRight;
            }
        }

        /// <summary>
        /// Renders bar chart (ideal for comparisons, categorical data)
        /// </summary>
        private void RenderBarSeries(DataSeries series, ScottPlot.Color color)
        {
            double[] values = series.Points.Select(p => p.Value).ToArray();
            double[] positions = Enumerable.Range(0, series.Points.Count).Select(i => (double)i).ToArray();

            var bars = _plot.Add.Bars(positions, values);
            foreach (var bar in bars.Bars)
            {
                bar.FillColor = color;
                bar.LineColor = color.WithAlpha(200);
                bar.LineWidth = 1;
            }
            bars.LegendText = series.Name;

            // Add tick labels
            string[] labels = series.Points.Select(p => p.Label).ToArray();
            if (labels.Length > 0)
            {
                double[] tickPositions = Enumerable.Range(0, labels.Length).Select(i => (double)i).ToArray();
                _plot.Axes.Bottom.SetTicks(tickPositions, labels);
                _plot.Axes.Bottom.TickLabelStyle.Rotation = 45;
                _plot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleRight;
            }
        }

        /// <summary>
        /// Renders area chart (emphasizes magnitude of change)
        /// </summary>
        private void RenderAreaSeries(DataSeries series, ScottPlot.Color color)
        {
            double[] values = series.Points.Select(p => p.Value).ToArray();
            double[] positions = Enumerable.Range(0, series.Points.Count).Select(i => (double)i).ToArray();

            // Create filled area
            var fillColor = color.WithAlpha(50);
            var lineColor = color;

            var line = _plot.Add.SignalXY(positions, values);
            line.Color = lineColor;
            line.LineWidth = 2;
            line.FillY = true;
            line.FillYColor = fillColor;
            line.LegendText = series.Name;

            // Add tick labels
            string[] labels = series.Points.Select(p => p.Label).ToArray();
            if (labels.Length > 0)
            {
                double[] tickPositions = Enumerable.Range(0, labels.Length).Select(i => (double)i).ToArray();
                _plot.Axes.Bottom.SetTicks(tickPositions, labels);
                _plot.Axes.Bottom.TickLabelStyle.Rotation = 45;
                _plot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleRight;
            }
        }

        /// <summary>
        /// Renders scatter plot (shows correlation, distribution)
        /// </summary>
        private void RenderScatterSeries(DataSeries series, ScottPlot.Color color)
        {
            double[] values = series.Points.Select(p => p.Value).ToArray();
            double[] positions = Enumerable.Range(0, series.Points.Count).Select(i => (double)i).ToArray();

            var scatter = _plot.Add.Scatter(positions, values);
            scatter.Color = color;
            scatter.MarkerSize = 8;
            scatter.LineWidth = 0;
            scatter.LegendText = series.Name;
        }

        /// <summary>
        /// Renders candlestick chart (stock market style OHLC)
        /// Note: This is a simplified version. For full OHLC, data needs Open/High/Low/Close values
        /// </summary>
        private void RenderCandlestickSeries(DataSeries series, ScottPlot.Color color)
        {
            // For now, render as bars with error ranges
            // In a full implementation, this would use actual OHLC data
            RenderBarSeries(series, color);
        }

        /// <summary>
        /// Adds a moving average line to the chart
        /// </summary>
        public void AddMovingAverage(DataSeries series, int period)
        {
            if (series.Points.Count < period)
                return;

            var ma = series.CalculateMovingAverage(period);
            RenderLineSeries(ma, ScottPlot.Color.FromHex("#FFA500")); // Orange
        }

        /// <summary>
        /// Adds a trend line to the chart
        /// </summary>
        public void AddTrendLine(DataSeries series)
        {
            if (series.Points.Count < 2)
                return;

            double[] x = Enumerable.Range(0, series.Points.Count).Select(i => (double)i).ToArray();
            double[] y = series.Points.Select(p => p.Value).ToArray();

            // Calculate linear regression
            var (slope, intercept) = CalculateLinearRegression(x, y);

            // Create trend line points
            double[] trendY = x.Select(xi => slope * xi + intercept).ToArray();

            var trendLine = _plot.Add.SignalXY(x, trendY);
            trendLine.Color = ScottPlot.Color.FromHex("#FF0000"); // Red
            trendLine.LineWidth = 2;
            trendLine.LinePattern = LinePattern.Dashed;
            trendLine.LegendText = $"{series.Name} (Tendencia)";
        }

        /// <summary>
        /// Adds horizontal reference line
        /// </summary>
        public void AddReferenceLine(double value, string label, ScottPlot.Color color)
        {
            var line = _plot.Add.HorizontalLine(value);
            line.Color = color;
            line.LineWidth = 1;
            line.LinePattern = LinePattern.Dotted;
            line.LegendText = label;
        }

        /// <summary>
        /// Applies visual style theme
        /// </summary>
        private void ApplyStyle(ChartStyle style)
        {
            switch (style)
            {
                case ChartStyle.Professional:
                    ApplyProfessionalStyle();
                    break;

                case ChartStyle.StockMarket:
                    ApplyStockMarketStyle();
                    break;

                case ChartStyle.Scientific:
                    ApplyScientificStyle();
                    break;

                case ChartStyle.Modern:
                    ApplyModernStyle();
                    break;

                case ChartStyle.Dark:
                    ApplyDarkStyle();
                    break;
            }
        }

        private void ApplyProfessionalStyle()
        {
            _plot.FigureBackground.Color = ScottPlot.Color.FromHex("#FFFFFF");
            _plot.DataBackground.Color = ScottPlot.Color.FromHex("#FFFFFF");
            _plot.Grid.MajorLineColor = ScottPlot.Color.FromColor(DrawingColor.FromArgb(200, 200, 200));
            _plot.Axes.Color(ScottPlot.Color.FromHex("#000000"));
        }

        private void ApplyStockMarketStyle()
        {
            // Bloomberg/Yahoo Finance inspired style
            _plot.FigureBackground.Color = ScottPlot.Color.FromColor(DrawingColor.FromArgb(240, 240, 240));
            _plot.DataBackground.Color = ScottPlot.Color.FromHex("#FFFFFF");
            _plot.Grid.MajorLineColor = ScottPlot.Color.FromColor(DrawingColor.FromArgb(220, 220, 220));
            _plot.Grid.MajorLineWidth = 1;
            _plot.Axes.Color(ScottPlot.Color.FromColor(DrawingColor.FromArgb(60, 60, 60)));
        }

        private void ApplyScientificStyle()
        {
            _plot.FigureBackground.Color = ScottPlot.Color.FromHex("#FFFFFF");
            _plot.DataBackground.Color = ScottPlot.Color.FromHex("#FFFFFF");
            _plot.Grid.MajorLineColor = ScottPlot.Color.FromColor(DrawingColor.FromArgb(150, 150, 150));
            _plot.Grid.MinorLineColor = ScottPlot.Color.FromColor(DrawingColor.FromArgb(200, 200, 200));
            _plot.Axes.Color(ScottPlot.Color.FromHex("#000000"));
        }

        private void ApplyModernStyle()
        {
            _plot.FigureBackground.Color = ScottPlot.Color.FromColor(DrawingColor.FromArgb(250, 250, 250));
            _plot.DataBackground.Color = ScottPlot.Color.FromHex("#FFFFFF");
            _plot.Grid.MajorLineColor = ScottPlot.Color.FromColor(DrawingColor.FromArgb(200, 220, 240));
            _plot.Axes.Color(ScottPlot.Color.FromColor(DrawingColor.FromArgb(80, 80, 80)));
        }

        private void ApplyDarkStyle()
        {
            _plot.FigureBackground.Color = ScottPlot.Color.FromColor(DrawingColor.FromArgb(30, 30, 30));
            _plot.DataBackground.Color = ScottPlot.Color.FromColor(DrawingColor.FromArgb(40, 40, 40));
            _plot.Grid.MajorLineColor = ScottPlot.Color.FromColor(DrawingColor.FromArgb(60, 60, 60));
            _plot.Axes.Color(ScottPlot.Color.FromColor(DrawingColor.FromArgb(200, 200, 200)));
            _plot.Axes.Title.Label.ForeColor = DrawingColor.White;
            _plot.Axes.Bottom.Label.ForeColor = DrawingColor.White;
            _plot.Axes.Left.Label.ForeColor = DrawingColor.White;
        }

        /// <summary>
        /// Saves the current chart to an image file
        /// </summary>
        public void SaveToFile(string filePath, int width = 1920, int height = 1080)
        {
            _plot.SavePng(filePath, width, height);
        }

        /// <summary>
        /// Gets the chart as a Bitmap for embedding in reports
        /// </summary>
        public Bitmap GetChartBitmap(int width = 800, int height = 600)
        {
            return new Bitmap(_plot.GetImage(width, height));
        }

        // Helper methods

        private ScottPlot.Color ParseColor(string hexColor)
        {
            try
            {
                if (hexColor.StartsWith("#"))
                    hexColor = hexColor.Substring(1);

                if (hexColor.Length == 6)
                {
                    int r = Convert.ToInt32(hexColor.Substring(0, 2), 16);
                    int g = Convert.ToInt32(hexColor.Substring(2, 2), 16);
                    int b = Convert.ToInt32(hexColor.Substring(4, 2), 16);
                    return ScottPlot.Color.FromColor(DrawingColor.FromArgb(r, g, b));
                }
            }
            catch
            {
                // Fall back to default color
            }

            return ScottPlot.Color.FromColor(DrawingColor.FromArgb(52, 152, 219)); // Default blue
        }

        private (double slope, double intercept) CalculateLinearRegression(double[] x, double[] y)
        {
            int n = x.Length;
            double sumX = x.Sum();
            double sumY = y.Sum();
            double sumXY = x.Zip(y, (xi, yi) => xi * yi).Sum();
            double sumX2 = x.Sum(xi => xi * xi);

            double slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            double intercept = (sumY - slope * sumX) / n;

            return (slope, intercept);
        }

        /// <summary>
        /// Creates a multi-panel chart for comparing multiple series
        /// </summary>
        public void CreateMultiPanelChart(List<DataSeries> seriesList, int columns = 2)
        {
            // For ScottPlot, we can create multiple subplots
            // This is a simplified version - full implementation would use subplot functionality
            RenderSeries(seriesList[0]);
        }
    }
}
