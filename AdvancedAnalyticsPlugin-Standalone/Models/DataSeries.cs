using System;
using System.Collections.Generic;
using System.Linq;

namespace ReactCRM.Plugins.AdvancedAnalytics.Models
{
    /// <summary>
    /// Represents a data series for charting and analysis
    /// </summary>
    public class DataSeries
    {
        public string Name { get; set; } = string.Empty;
        public List<DataPoint> Points { get; set; } = new List<DataPoint>();
        public string Color { get; set; } = "#3498db";
        public SeriesType Type { get; set; } = SeriesType.Line;
        public string SourceField { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets statistical summary of the data series
        /// </summary>
        public DataStatistics GetStatistics()
        {
            if (!Points.Any())
                return new DataStatistics();

            var values = Points.Select(p => p.Value).ToList();

            return new DataStatistics
            {
                Count = values.Count,
                Sum = values.Sum(),
                Average = values.Average(),
                Min = values.Min(),
                Max = values.Max(),
                StdDev = CalculateStdDev(values),
                Median = CalculateMedian(values)
            };
        }

        /// <summary>
        /// Applies a mathematical operation to all data points
        /// </summary>
        public void ApplyOperation(MathOperation operation, double operand)
        {
            foreach (var point in Points)
            {
                point.Value = operation switch
                {
                    MathOperation.Add => point.Value + operand,
                    MathOperation.Subtract => point.Value - operand,
                    MathOperation.Multiply => point.Value * operand,
                    MathOperation.Divide => operand != 0 ? point.Value / operand : point.Value,
                    _ => point.Value
                };
            }
        }

        /// <summary>
        /// Creates a moving average series
        /// </summary>
        public DataSeries CalculateMovingAverage(int period)
        {
            var ma = new DataSeries
            {
                Name = $"{Name} (MA{period})",
                Type = SeriesType.Line,
                Color = AdjustColor(Color, -30),
                SourceField = SourceField
            };

            for (int i = period - 1; i < Points.Count; i++)
            {
                var avg = Points.Skip(i - period + 1).Take(period).Average(p => p.Value);
                ma.Points.Add(new DataPoint
                {
                    Label = Points[i].Label,
                    Value = avg,
                    Timestamp = Points[i].Timestamp
                });
            }

            return ma;
        }

        private double CalculateStdDev(List<double> values)
        {
            if (values.Count < 2)
                return 0;

            double avg = values.Average();
            double sumOfSquares = values.Sum(v => Math.Pow(v - avg, 2));
            return Math.Sqrt(sumOfSquares / values.Count);
        }

        private double CalculateMedian(List<double> values)
        {
            var sorted = values.OrderBy(v => v).ToList();
            int count = sorted.Count;

            if (count % 2 == 0)
                return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
            else
                return sorted[count / 2];
        }

        private string AdjustColor(string hexColor, int amount)
        {
            // Simple color adjustment for MA lines
            return hexColor;
        }
    }

    /// <summary>
    /// Represents a single data point
    /// </summary>
    public class DataPoint
    {
        public string Label { get; set; } = string.Empty;
        public double Value { get; set; }
        public DateTime? Timestamp { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Statistical summary of a data series
    /// </summary>
    public class DataStatistics
    {
        public int Count { get; set; }
        public double Sum { get; set; }
        public double Average { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public double StdDev { get; set; }
        public double Median { get; set; }
        public double Range => Max - Min;
    }

    /// <summary>
    /// Types of chart series
    /// </summary>
    public enum SeriesType
    {
        Line,
        Bar,
        Area,
        Candlestick,
        Scatter
    }

    /// <summary>
    /// Mathematical operations
    /// </summary>
    public enum MathOperation
    {
        Add,
        Subtract,
        Multiply,
        Divide
    }
}
