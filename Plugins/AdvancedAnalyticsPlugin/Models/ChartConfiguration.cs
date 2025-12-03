using System;
using System.Collections.Generic;
using System.Drawing;

namespace ReactCRM.Plugins.AdvancedAnalytics.Models
{
    /// <summary>
    /// Configuration for chart rendering
    /// </summary>
    public class ChartConfiguration
    {
        public string Title { get; set; } = "Analytics Chart";
        public string XAxisLabel { get; set; } = "X Axis";
        public string YAxisLabel { get; set; } = "Y Axis";
        public ChartStyle Style { get; set; } = ChartStyle.Professional;
        public bool ShowLegend { get; set; } = true;
        public bool ShowGrid { get; set; } = true;
        public bool ShowDataLabels { get; set; } = false;
        public bool Enable3D { get; set; } = false;
        public int Width { get; set; } = 800;
        public int Height { get; set; } = 600;

        // Stock market specific features
        public bool ShowMovingAverage { get; set; } = false;
        public int MovingAveragePeriod { get; set; } = 20;
        public bool ShowTrendLine { get; set; } = false;
        public bool ShowVolume { get; set; } = false;
        public bool ShowCandlesticks { get; set; } = false;

        // Advanced features
        public bool EnableZoom { get; set; } = true;
        public bool EnablePan { get; set; } = true;
        public bool EnableTooltips { get; set; } = true;
        public bool EnableCrosshair { get; set; } = false;

        public Color BackgroundColor { get; set; } = Color.White;
        public Color GridColor { get; set; } = Color.LightGray;
        public Color TextColor { get; set; } = Color.Black;

        public List<DataSeries> Series { get; set; } = new List<DataSeries>();
    }

    /// <summary>
    /// Chart visual styles
    /// </summary>
    public enum ChartStyle
    {
        Professional,    // Clean, business-ready charts
        StockMarket,    // Financial/trading style
        Scientific,     // Academic/research style
        Modern,         // Colorful modern design
        Dark           // Dark mode theme
    }

    /// <summary>
    /// Query configuration for data extraction
    /// </summary>
    public class QueryConfiguration
    {
        public List<FieldSelection> SelectedFields { get; set; } = new List<FieldSelection>();
        public List<FilterRule> Filters { get; set; } = new List<FilterRule>();
        public string GroupByField { get; set; } = string.Empty;
        public AggregationFunction Aggregation { get; set; } = AggregationFunction.Count;
        public string DateRangeField { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Limit { get; set; }
        public string OrderBy { get; set; } = string.Empty;
        public bool OrderDescending { get; set; } = false;
    }

    /// <summary>
    /// Field selection for queries
    /// </summary>
    public class FieldSelection
    {
        public string FieldName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public FieldType Type { get; set; }
        public bool IsCustomField { get; set; }
        public AggregationFunction? Aggregation { get; set; }
    }

    /// <summary>
    /// Filter rule for data queries
    /// </summary>
    public class FilterRule
    {
        public string FieldName { get; set; } = string.Empty;
        public FilterOperator Operator { get; set; }
        public object? Value { get; set; }
        public FilterLogic Logic { get; set; } = FilterLogic.And;
    }

    /// <summary>
    /// Field types
    /// </summary>
    public enum FieldType
    {
        Text,
        Number,
        Date,
        Boolean,
        Dropdown
    }

    /// <summary>
    /// Aggregation functions
    /// </summary>
    public enum AggregationFunction
    {
        None,
        Count,
        Sum,
        Average,
        Min,
        Max,
        Median,
        StdDev
    }

    /// <summary>
    /// Filter operators
    /// </summary>
    public enum FilterOperator
    {
        Equals,
        NotEquals,
        GreaterThan,
        LessThan,
        GreaterOrEqual,
        LessOrEqual,
        Contains,
        StartsWith,
        EndsWith,
        IsNull,
        IsNotNull,
        Between
    }

    /// <summary>
    /// Filter logic combinators
    /// </summary>
    public enum FilterLogic
    {
        And,
        Or
    }
}
