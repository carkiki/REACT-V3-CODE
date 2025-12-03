using System;
using System.Collections.Generic;

namespace ReactCRM.Plugins.AdvancedAnalytics.Models
{
    /// <summary>
    /// Result of an analytics query and computation
    /// </summary>
    public class AnalyticsResult
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
        public string QueryDescription { get; set; } = string.Empty;
        public List<DataSeries> Series { get; set; } = new List<DataSeries>();
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        public List<Insight> Insights { get; set; } = new List<Insight>();
        public int TotalRecordsAnalyzed { get; set; }
        public TimeSpan ExecutionTime { get; set; }

        /// <summary>
        /// Summary statistics across all series
        /// </summary>
        public OverallStatistics GetOverallStatistics()
        {
            var stats = new OverallStatistics();

            foreach (var series in Series)
            {
                var seriesStats = series.GetStatistics();
                stats.TotalDataPoints += seriesStats.Count;
                stats.SeriesCount++;

                if (seriesStats.Count > 0)
                {
                    stats.GlobalMin = Math.Min(stats.GlobalMin ?? seriesStats.Min, seriesStats.Min);
                    stats.GlobalMax = Math.Max(stats.GlobalMax ?? seriesStats.Max, seriesStats.Max);
                    stats.GlobalSum += seriesStats.Sum;
                }
            }

            if (stats.TotalDataPoints > 0)
                stats.GlobalAverage = stats.GlobalSum / stats.TotalDataPoints;

            return stats;
        }
    }

    /// <summary>
    /// Overall statistics across multiple data series
    /// </summary>
    public class OverallStatistics
    {
        public int SeriesCount { get; set; }
        public int TotalDataPoints { get; set; }
        public double? GlobalMin { get; set; }
        public double? GlobalMax { get; set; }
        public double GlobalSum { get; set; }
        public double GlobalAverage { get; set; }
        public double GlobalRange => (GlobalMax ?? 0) - (GlobalMin ?? 0);
    }

    /// <summary>
    /// Automatically generated insight from data analysis
    /// </summary>
    public class Insight
    {
        public InsightType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public InsightSeverity Severity { get; set; } = InsightSeverity.Info;
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Types of insights
    /// </summary>
    public enum InsightType
    {
        Trend,              // Upward/downward trend detected
        Anomaly,            // Unusual data point detected
        Correlation,        // Correlation between series
        Seasonality,        // Seasonal pattern detected
        Threshold,          // Value crossed threshold
        Pattern,            // Specific pattern detected
        Recommendation      // Recommended action
    }

    /// <summary>
    /// Insight severity levels
    /// </summary>
    public enum InsightSeverity
    {
        Info,
        Warning,
        Critical,
        Positive
    }

    /// <summary>
    /// Trend analysis result
    /// </summary>
    public class TrendAnalysis
    {
        public TrendDirection Direction { get; set; }
        public double Slope { get; set; }
        public double RSquared { get; set; }
        public double Confidence { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Trend direction
    /// </summary>
    public enum TrendDirection
    {
        Increasing,
        Decreasing,
        Stable,
        Volatile
    }
}
