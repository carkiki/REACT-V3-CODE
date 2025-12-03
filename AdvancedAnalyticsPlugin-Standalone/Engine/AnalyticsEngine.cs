using System;
using System.Collections.Generic;
using System.Linq;
using ReactCRM.Plugins.AdvancedAnalytics.Models;

namespace ReactCRM.Plugins.AdvancedAnalytics.Engine
{
    /// <summary>
    /// Advanced analytics engine with intelligent algorithms for data analysis
    /// </summary>
    public class AnalyticsEngine
    {
        /// <summary>
        /// Analyzes data and generates insights automatically
        /// </summary>
        public List<Insight> GenerateInsights(AnalyticsResult result)
        {
            var insights = new List<Insight>();

            foreach (var series in result.Series)
            {
                // Detect trends
                var trendInsight = DetectTrend(series);
                if (trendInsight != null)
                    insights.Add(trendInsight);

                // Detect anomalies
                var anomalies = DetectAnomalies(series);
                insights.AddRange(anomalies);

                // Calculate volatility
                var volatilityInsight = AnalyzeVolatility(series);
                if (volatilityInsight != null)
                    insights.Add(volatilityInsight);
            }

            // Cross-series analysis
            if (result.Series.Count > 1)
            {
                var correlationInsights = AnalyzeCorrelations(result.Series);
                insights.AddRange(correlationInsights);
            }

            // Overall performance insights
            var performanceInsight = AnalyzePerformance(result);
            if (performanceInsight != null)
                insights.Add(performanceInsight);

            return insights;
        }

        /// <summary>
        /// Detects trend direction and strength
        /// </summary>
        public TrendAnalysis AnalyzeTrend(DataSeries series)
        {
            if (series.Points.Count < 3)
            {
                return new TrendAnalysis
                {
                    Direction = TrendDirection.Stable,
                    Slope = 0,
                    RSquared = 0,
                    Confidence = 0,
                    Description = "Datos insuficientes para análisis de tendencia"
                };
            }

            // Linear regression
            double[] x = Enumerable.Range(0, series.Points.Count).Select(i => (double)i).ToArray();
            double[] y = series.Points.Select(p => p.Value).ToArray();

            var (slope, intercept, rSquared) = CalculateLinearRegressionWithR2(x, y);

            // Determine direction based on slope
            TrendDirection direction;
            if (Math.Abs(slope) < 0.01)
                direction = TrendDirection.Stable;
            else if (slope > 0)
                direction = TrendDirection.Increasing;
            else
                direction = TrendDirection.Decreasing;

            // Check for volatility
            var stdDev = CalculateStdDev(y);
            var mean = y.Average();
            var coefficientOfVariation = mean != 0 ? stdDev / Math.Abs(mean) : 0;

            if (coefficientOfVariation > 0.5)
                direction = TrendDirection.Volatile;

            string description = direction switch
            {
                TrendDirection.Increasing => $"Tendencia alcista con pendiente {slope:F2}",
                TrendDirection.Decreasing => $"Tendencia bajista con pendiente {slope:F2}",
                TrendDirection.Stable => "Tendencia estable sin cambios significativos",
                TrendDirection.Volatile => $"Alta volatilidad (CV: {coefficientOfVariation:P0})",
                _ => "Análisis no disponible"
            };

            return new TrendAnalysis
            {
                Direction = direction,
                Slope = slope,
                RSquared = rSquared,
                Confidence = rSquared * 100,
                Description = description
            };
        }

        /// <summary>
        /// Detects trend and creates insight
        /// </summary>
        private Insight? DetectTrend(DataSeries series)
        {
            var trend = AnalyzeTrend(series);

            if (trend.Confidence < 30)
                return null; // Not confident enough

            var severity = trend.Direction switch
            {
                TrendDirection.Increasing => InsightSeverity.Positive,
                TrendDirection.Decreasing => InsightSeverity.Warning,
                TrendDirection.Volatile => InsightSeverity.Critical,
                _ => InsightSeverity.Info
            };

            return new Insight
            {
                Type = InsightType.Trend,
                Title = $"Tendencia detectada en {series.Name}",
                Description = trend.Description + $". Confianza: {trend.Confidence:F1}%",
                Severity = severity,
                Data = new Dictionary<string, object>
                {
                    ["slope"] = trend.Slope,
                    ["rSquared"] = trend.RSquared,
                    ["direction"] = trend.Direction.ToString()
                }
            };
        }

        /// <summary>
        /// Detects anomalies using statistical methods
        /// </summary>
        private List<Insight> DetectAnomalies(DataSeries series)
        {
            var insights = new List<Insight>();

            if (series.Points.Count < 5)
                return insights;

            var values = series.Points.Select(p => p.Value).ToList();
            var mean = values.Average();
            var stdDev = CalculateStdDev(values);

            // Use 2 standard deviations as threshold
            var threshold = 2 * stdDev;

            var anomalies = series.Points
                .Select((p, i) => new { Point = p, Index = i })
                .Where(x => Math.Abs(x.Point.Value - mean) > threshold)
                .ToList();

            foreach (var anomaly in anomalies)
            {
                var deviation = (anomaly.Point.Value - mean) / stdDev;
                var direction = deviation > 0 ? "superior" : "inferior";

                insights.Add(new Insight
                {
                    Type = InsightType.Anomaly,
                    Title = $"Anomalía detectada en {series.Name}",
                    Description = $"Valor {anomaly.Point.Value:F2} en '{anomaly.Point.Label}' " +
                                  $"está {Math.Abs(deviation):F1}σ {direction} al promedio ({mean:F2})",
                    Severity = Math.Abs(deviation) > 3 ? InsightSeverity.Critical : InsightSeverity.Warning,
                    Data = new Dictionary<string, object>
                    {
                        ["value"] = anomaly.Point.Value,
                        ["mean"] = mean,
                        ["deviation"] = deviation,
                        ["index"] = anomaly.Index
                    }
                });
            }

            return insights;
        }

        /// <summary>
        /// Analyzes volatility and risk
        /// </summary>
        private Insight? AnalyzeVolatility(DataSeries series)
        {
            if (series.Points.Count < 3)
                return null;

            var values = series.Points.Select(p => p.Value).ToList();
            var mean = values.Average();
            var stdDev = CalculateStdDev(values);
            var coefficientOfVariation = mean != 0 ? stdDev / Math.Abs(mean) : 0;

            // Only report significant volatility
            if (coefficientOfVariation < 0.2)
                return null;

            var severity = coefficientOfVariation switch
            {
                > 0.5 => InsightSeverity.Critical,
                > 0.3 => InsightSeverity.Warning,
                _ => InsightSeverity.Info
            };

            return new Insight
            {
                Type = InsightType.Pattern,
                Title = $"Análisis de volatilidad: {series.Name}",
                Description = $"Coeficiente de variación: {coefficientOfVariation:P1}. " +
                              $"Desviación estándar: {stdDev:F2}",
                Severity = severity,
                Data = new Dictionary<string, object>
                {
                    ["stdDev"] = stdDev,
                    ["mean"] = mean,
                    ["cv"] = coefficientOfVariation
                }
            };
        }

        /// <summary>
        /// Analyzes correlations between multiple series
        /// </summary>
        private List<Insight> AnalyzeCorrelations(List<DataSeries> seriesList)
        {
            var insights = new List<Insight>();

            for (int i = 0; i < seriesList.Count - 1; i++)
            {
                for (int j = i + 1; j < seriesList.Count; j++)
                {
                    var correlation = CalculateCorrelation(seriesList[i], seriesList[j]);

                    if (Math.Abs(correlation) > 0.7) // Strong correlation
                    {
                        var type = correlation > 0 ? "positiva" : "negativa";
                        var strength = Math.Abs(correlation) > 0.9 ? "muy fuerte" : "fuerte";

                        insights.Add(new Insight
                        {
                            Type = InsightType.Correlation,
                            Title = $"Correlación {strength} detectada",
                            Description = $"Correlación {type} ({correlation:F2}) entre " +
                                          $"'{seriesList[i].Name}' y '{seriesList[j].Name}'",
                            Severity = InsightSeverity.Info,
                            Data = new Dictionary<string, object>
                            {
                                ["correlation"] = correlation,
                                ["series1"] = seriesList[i].Name,
                                ["series2"] = seriesList[j].Name
                            }
                        });
                    }
                }
            }

            return insights;
        }

        /// <summary>
        /// Analyzes overall performance and generates recommendations
        /// </summary>
        private Insight? AnalyzePerformance(AnalyticsResult result)
        {
            var stats = result.GetOverallStatistics();

            if (stats.TotalDataPoints == 0)
                return null;

            var description = $"Análisis completo de {stats.TotalDataPoints} puntos de datos " +
                              $"en {stats.SeriesCount} serie(s). " +
                              $"Rango global: {stats.GlobalMin:F2} - {stats.GlobalMax:F2}. " +
                              $"Promedio: {stats.GlobalAverage:F2}";

            return new Insight
            {
                Type = InsightType.Recommendation,
                Title = "Resumen del análisis",
                Description = description,
                Severity = InsightSeverity.Info,
                Data = new Dictionary<string, object>
                {
                    ["totalPoints"] = stats.TotalDataPoints,
                    ["seriesCount"] = stats.SeriesCount,
                    ["average"] = stats.GlobalAverage,
                    ["range"] = stats.GlobalRange
                }
            };
        }

        /// <summary>
        /// Calculates moving average for a series
        /// </summary>
        public DataSeries CalculateMovingAverage(DataSeries series, int period)
        {
            return series.CalculateMovingAverage(period);
        }

        /// <summary>
        /// Calculates exponential moving average (EMA)
        /// </summary>
        public DataSeries CalculateEMA(DataSeries series, int period)
        {
            var ema = new DataSeries
            {
                Name = $"{series.Name} (EMA{period})",
                Type = SeriesType.Line,
                Color = "#e74c3c",
                SourceField = series.SourceField
            };

            if (series.Points.Count < period)
                return ema;

            double multiplier = 2.0 / (period + 1);
            double emaValue = series.Points.Take(period).Average(p => p.Value);

            for (int i = 0; i < series.Points.Count; i++)
            {
                if (i < period - 1)
                    continue;

                if (i == period - 1)
                {
                    emaValue = series.Points.Take(period).Average(p => p.Value);
                }
                else
                {
                    emaValue = (series.Points[i].Value - emaValue) * multiplier + emaValue;
                }

                ema.Points.Add(new DataPoint
                {
                    Label = series.Points[i].Label,
                    Value = emaValue,
                    Timestamp = series.Points[i].Timestamp
                });
            }

            return ema;
        }

        /// <summary>
        /// Calculates Relative Strength Index (RSI) - common in stock analysis
        /// </summary>
        public DataSeries CalculateRSI(DataSeries series, int period = 14)
        {
            var rsi = new DataSeries
            {
                Name = $"{series.Name} (RSI{period})",
                Type = SeriesType.Line,
                Color = "#9b59b6",
                SourceField = series.SourceField
            };

            if (series.Points.Count < period + 1)
                return rsi;

            var gains = new List<double>();
            var losses = new List<double>();

            // Calculate price changes
            for (int i = 1; i < series.Points.Count; i++)
            {
                double change = series.Points[i].Value - series.Points[i - 1].Value;
                gains.Add(change > 0 ? change : 0);
                losses.Add(change < 0 ? Math.Abs(change) : 0);
            }

            // Calculate RSI
            for (int i = period; i < gains.Count; i++)
            {
                double avgGain = gains.Skip(i - period).Take(period).Average();
                double avgLoss = losses.Skip(i - period).Take(period).Average();

                double rs = avgLoss == 0 ? 100 : avgGain / avgLoss;
                double rsiValue = 100 - (100 / (1 + rs));

                rsi.Points.Add(new DataPoint
                {
                    Label = series.Points[i + 1].Label,
                    Value = rsiValue,
                    Timestamp = series.Points[i + 1].Timestamp
                });
            }

            return rsi;
        }

        /// <summary>
        /// Detects seasonal patterns in time series data
        /// </summary>
        public Insight? DetectSeasonality(DataSeries series)
        {
            if (series.Points.Count < 12) // Need at least 12 points for monthly seasonality
                return null;

            // Simple seasonality detection using autocorrelation
            // In a full implementation, this would use more sophisticated methods

            return new Insight
            {
                Type = InsightType.Seasonality,
                Title = "Análisis de estacionalidad",
                Description = "Se requieren más datos históricos para un análisis confiable",
                Severity = InsightSeverity.Info
            };
        }

        // Helper methods

        private double CalculateStdDev(IEnumerable<double> values)
        {
            var valuesList = values.ToList();
            if (valuesList.Count < 2)
                return 0;

            double avg = valuesList.Average();
            double sumOfSquares = valuesList.Sum(v => Math.Pow(v - avg, 2));
            return Math.Sqrt(sumOfSquares / valuesList.Count);
        }

        private (double slope, double intercept, double rSquared) CalculateLinearRegressionWithR2(double[] x, double[] y)
        {
            int n = x.Length;
            double sumX = x.Sum();
            double sumY = y.Sum();
            double sumXY = x.Zip(y, (xi, yi) => xi * yi).Sum();
            double sumX2 = x.Sum(xi => xi * xi);
            double sumY2 = y.Sum(yi => yi * yi);

            double slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            double intercept = (sumY - slope * sumX) / n;

            // Calculate R-squared
            double meanY = y.Average();
            double ssTotal = y.Sum(yi => Math.Pow(yi - meanY, 2));
            double ssResidual = y.Zip(x, (yi, xi) => Math.Pow(yi - (slope * xi + intercept), 2)).Sum();
            double rSquared = ssTotal != 0 ? 1 - (ssResidual / ssTotal) : 0;

            return (slope, intercept, rSquared);
        }

        private double CalculateCorrelation(DataSeries series1, DataSeries series2)
        {
            // Only calculate for series with matching number of points
            int minCount = Math.Min(series1.Points.Count, series2.Points.Count);
            if (minCount < 3)
                return 0;

            var x = series1.Points.Take(minCount).Select(p => p.Value).ToArray();
            var y = series2.Points.Take(minCount).Select(p => p.Value).ToArray();

            double meanX = x.Average();
            double meanY = y.Average();

            double covariance = x.Zip(y, (xi, yi) => (xi - meanX) * (yi - meanY)).Sum() / minCount;
            double stdX = Math.Sqrt(x.Sum(xi => Math.Pow(xi - meanX, 2)) / minCount);
            double stdY = Math.Sqrt(y.Sum(yi => Math.Pow(yi - meanY, 2)) / minCount);

            return stdX != 0 && stdY != 0 ? covariance / (stdX * stdY) : 0;
        }

        /// <summary>
        /// Performs operations between two series
        /// </summary>
        public DataSeries CombineSeries(DataSeries series1, DataSeries series2, MathOperation operation)
        {
            var result = new DataSeries
            {
                Name = $"{series1.Name} {operation} {series2.Name}",
                Type = SeriesType.Line
            };

            int minCount = Math.Min(series1.Points.Count, series2.Points.Count);

            for (int i = 0; i < minCount; i++)
            {
                double value = operation switch
                {
                    MathOperation.Add => series1.Points[i].Value + series2.Points[i].Value,
                    MathOperation.Subtract => series1.Points[i].Value - series2.Points[i].Value,
                    MathOperation.Multiply => series1.Points[i].Value * series2.Points[i].Value,
                    MathOperation.Divide => series2.Points[i].Value != 0
                        ? series1.Points[i].Value / series2.Points[i].Value
                        : 0,
                    _ => 0
                };

                result.Points.Add(new DataPoint
                {
                    Label = series1.Points[i].Label,
                    Value = value,
                    Timestamp = series1.Points[i].Timestamp
                });
            }

            return result;
        }
    }
}
