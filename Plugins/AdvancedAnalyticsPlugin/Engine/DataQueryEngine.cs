using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using ReactCRM.Database;
using ReactCRM.Models;
using ReactCRM.Plugins.AdvancedAnalytics.Models;

namespace ReactCRM.Plugins.AdvancedAnalytics.Engine
{
    /// <summary>
    /// Advanced data query engine for extracting and transforming database data
    /// Optimized for large datasets (3000+ records)
    /// </summary>
    public class DataQueryEngine
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();

        /// <summary>
        /// Maximum number of records to process. Set to 0 for unlimited.
        /// Default: 5000 for performance
        /// </summary>
        public int MaxRecords { get; set; } = 5000;

        /// <summary>
        /// Maximum number of data points to display in charts.
        /// Larger datasets will be intelligently sampled.
        /// Default: 1000 for responsive charting
        /// </summary>
        public int MaxChartPoints { get; set; } = 1000;

        /// <summary>
        /// Enable intelligent data sampling for large datasets
        /// </summary>
        public bool EnableSmartSampling { get; set; } = true;

        /// <summary>
        /// Progress callback for long-running operations
        /// </summary>
        public Action<int, string>? ProgressCallback { get; set; }

        /// <summary>
        /// Gets all available fields (native + custom) for query building
        /// </summary>
        public List<FieldSelection> GetAvailableFields()
        {
            var fields = new List<FieldSelection>();

            // Native fields from Clients table
            fields.Add(new FieldSelection
            {
                FieldName = "Id",
                DisplayName = "ID del Cliente",
                Type = FieldType.Number,
                IsCustomField = false
            });

            fields.Add(new FieldSelection
            {
                FieldName = "SSN",
                DisplayName = "SSN",
                Type = FieldType.Text,
                IsCustomField = false
            });

            fields.Add(new FieldSelection
            {
                FieldName = "Name",
                DisplayName = "Nombre",
                Type = FieldType.Text,
                IsCustomField = false
            });

            fields.Add(new FieldSelection
            {
                FieldName = "DOB",
                DisplayName = "Fecha de Nacimiento",
                Type = FieldType.Date,
                IsCustomField = false
            });

            fields.Add(new FieldSelection
            {
                FieldName = "Phone",
                DisplayName = "Teléfono",
                Type = FieldType.Text,
                IsCustomField = false
            });

            fields.Add(new FieldSelection
            {
                FieldName = "Email",
                DisplayName = "Email",
                Type = FieldType.Text,
                IsCustomField = false
            });

            fields.Add(new FieldSelection
            {
                FieldName = "CreatedAt",
                DisplayName = "Fecha de Creación",
                Type = FieldType.Date,
                IsCustomField = false
            });

            fields.Add(new FieldSelection
            {
                FieldName = "LastUpdated",
                DisplayName = "Última Actualización",
                Type = FieldType.Date,
                IsCustomField = false
            });

            // Get custom fields
            try
            {
                var customFieldRepo = new CustomFieldRepository();
                var customFields = customFieldRepo.GetAll();

                foreach (var cf in customFields.Where(f => f.IsActive))
                {
                    var fieldType = cf.FieldType.ToLower() switch
                    {
                        "number" => FieldType.Number,
                        "date" => FieldType.Date,
                        "checkbox" => FieldType.Boolean,
                        "dropdown" => FieldType.Dropdown,
                        _ => FieldType.Text
                    };

                    fields.Add(new FieldSelection
                    {
                        FieldName = cf.FieldName,
                        DisplayName = cf.Label,
                        Type = fieldType,
                        IsCustomField = true
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading custom fields: {ex.Message}");
            }

            return fields;
        }

        /// <summary>
        /// Executes a query and returns analytics results
        /// </summary>
        public AnalyticsResult ExecuteQuery(QueryConfiguration config)
        {
            _stopwatch.Restart();

            var result = new AnalyticsResult
            {
                QueryDescription = BuildQueryDescription(config)
            };

            try
            {
                ReportProgress(0, "Conectando a la base de datos...");

                using (var connection = DbConnection.GetConnection())
                {
                    connection.Open();

                    ReportProgress(10, "Cargando datos de clientes...");

                    // Get all clients with their data
                    var clientRepo = new ClientRepository();
                    var allClients = clientRepo.GetAllClients();

                    ReportProgress(30, $"Procesando {allClients.Count} registros...");

                    // Apply filters
                    var filteredClients = ApplyFilters(allClients, config.Filters);

                    // Apply MaxRecords limit if set
                    if (MaxRecords > 0 && filteredClients.Count > MaxRecords)
                    {
                        ReportProgress(40, $"Limitando a {MaxRecords} registros...");
                        filteredClients = filteredClients.Take(MaxRecords).ToList();
                    }

                    result.TotalRecordsAnalyzed = filteredClients.Count;

                    ReportProgress(50, "Construyendo series de datos...");

                    // Build data series based on configuration
                    if (!string.IsNullOrEmpty(config.GroupByField))
                    {
                        // Grouped data (e.g., count by month, sum by category)
                        result.Series.AddRange(BuildGroupedSeries(filteredClients, config));
                    }
                    else
                    {
                        // Individual data points
                        result.Series.AddRange(BuildIndividualSeries(filteredClients, config));
                    }

                    ReportProgress(80, "Aplicando agregaciones...");

                    // Apply aggregations if specified
                    foreach (var field in config.SelectedFields.Where(f => f.Aggregation.HasValue))
                    {
                        var series = BuildAggregatedSeries(filteredClients, field, config);
                        if (series != null)
                            result.Series.Add(series);
                    }

                    ReportProgress(100, "Consulta completada.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error executing query: {ex.Message}\n{ex.StackTrace}");
                ReportProgress(100, "Error en la consulta.");
                throw new Exception($"Error al ejecutar la consulta: {ex.Message}", ex);
            }

            _stopwatch.Stop();
            result.ExecutionTime = _stopwatch.Elapsed;

            // Add metadata about performance
            result.Metadata["MaxRecordsLimit"] = MaxRecords;
            result.Metadata["MaxChartPointsLimit"] = MaxChartPoints;
            result.Metadata["SmartSamplingEnabled"] = EnableSmartSampling;

            return result;
        }

        /// <summary>
        /// Applies filter rules to client list
        /// </summary>
        private List<Client> ApplyFilters(List<Client> clients, List<FilterRule> filters)
        {
            if (filters == null || !filters.Any())
                return clients;

            var filtered = clients.AsEnumerable();

            foreach (var filter in filters)
            {
                filtered = ApplySingleFilter(filtered, filter);
            }

            return filtered.ToList();
        }

        /// <summary>
        /// Applies a single filter rule
        /// </summary>
        private IEnumerable<Client> ApplySingleFilter(IEnumerable<Client> clients, FilterRule filter)
        {
            return clients.Where(client =>
            {
                object? fieldValue = GetFieldValue(client, filter.FieldName);

                return filter.Operator switch
                {
                    FilterOperator.Equals => CompareValues(fieldValue, filter.Value, (a, b) => a.Equals(b)),
                    FilterOperator.NotEquals => CompareValues(fieldValue, filter.Value, (a, b) => !a.Equals(b)),
                    FilterOperator.GreaterThan => CompareNumeric(fieldValue, filter.Value, (a, b) => a > b),
                    FilterOperator.LessThan => CompareNumeric(fieldValue, filter.Value, (a, b) => a < b),
                    FilterOperator.GreaterOrEqual => CompareNumeric(fieldValue, filter.Value, (a, b) => a >= b),
                    FilterOperator.LessOrEqual => CompareNumeric(fieldValue, filter.Value, (a, b) => a <= b),
                    FilterOperator.Contains => fieldValue?.ToString()?.Contains(filter.Value?.ToString() ?? "") ?? false,
                    FilterOperator.StartsWith => fieldValue?.ToString()?.StartsWith(filter.Value?.ToString() ?? "") ?? false,
                    FilterOperator.EndsWith => fieldValue?.ToString()?.EndsWith(filter.Value?.ToString() ?? "") ?? false,
                    FilterOperator.IsNull => fieldValue == null,
                    FilterOperator.IsNotNull => fieldValue != null,
                    _ => true
                };
            });
        }

        /// <summary>
        /// Gets field value from client (native or custom field)
        /// </summary>
        private object? GetFieldValue(Client client, string fieldName)
        {
            // Try native fields first
            var property = typeof(Client).GetProperty(fieldName);
            if (property != null)
                return property.GetValue(client);

            // Try custom fields
            return client.GetExtraDataValue(fieldName);
        }

        /// <summary>
        /// Builds series for grouped data
        /// </summary>
        private List<DataSeries> BuildGroupedSeries(List<Client> clients, QueryConfiguration config)
        {
            var seriesList = new List<DataSeries>();

            var groupedData = clients.GroupBy(c => GetFieldValue(c, config.GroupByField)?.ToString() ?? "N/A");

            foreach (var field in config.SelectedFields)
            {
                var series = new DataSeries
                {
                    Name = field.DisplayName,
                    SourceField = field.FieldName,
                    Type = SeriesType.Bar
                };

                foreach (var group in groupedData.OrderBy(g => g.Key))
                {
                    double value = 0;

                    if (config.Aggregation == AggregationFunction.Count)
                    {
                        value = group.Count();
                    }
                    else
                    {
                        var values = group
                            .Select(c => GetFieldValue(c, field.FieldName))
                            .Where(v => v != null && IsNumeric(v))
                            .Select(v => Convert.ToDouble(v))
                            .ToList();

                        if (values.Any())
                        {
                            value = config.Aggregation switch
                            {
                                AggregationFunction.Sum => values.Sum(),
                                AggregationFunction.Average => values.Average(),
                                AggregationFunction.Min => values.Min(),
                                AggregationFunction.Max => values.Max(),
                                AggregationFunction.Median => CalculateMedian(values),
                                _ => values.Count()
                            };
                        }
                    }

                    series.Points.Add(new DataPoint
                    {
                        Label = group.Key,
                        Value = value
                    });
                }

                if (series.Points.Any())
                    seriesList.Add(series);
            }

            return seriesList;
        }

        /// <summary>
        /// Builds series for individual data points
        /// </summary>
        private List<DataSeries> BuildIndividualSeries(List<Client> clients, QueryConfiguration config)
        {
            var seriesList = new List<DataSeries>();

            foreach (var field in config.SelectedFields.Where(f => !f.Aggregation.HasValue))
            {
                var series = new DataSeries
                {
                    Name = field.DisplayName,
                    SourceField = field.FieldName,
                    Type = SeriesType.Line
                };

                var orderedClients = clients;

                // Order by specified field or by Id
                if (!string.IsNullOrEmpty(config.OrderBy))
                {
                    orderedClients = config.OrderDescending
                        ? clients.OrderByDescending(c => GetFieldValue(c, config.OrderBy)).ToList()
                        : clients.OrderBy(c => GetFieldValue(c, config.OrderBy)).ToList();
                }

                // Apply limit if specified
                if (config.Limit.HasValue)
                {
                    orderedClients = orderedClients.Take(config.Limit.Value).ToList();
                }

                foreach (var client in orderedClients)
                {
                    var value = GetFieldValue(client, field.FieldName);

                    if (value != null && IsNumeric(value))
                    {
                        series.Points.Add(new DataPoint
                        {
                            Label = client.Name ?? $"Client {client.Id}",
                            Value = Convert.ToDouble(value),
                            Timestamp = client.CreatedAt
                        });
                    }
                }

                // Apply intelligent sampling if needed
                if (EnableSmartSampling && series.Points.Count > MaxChartPoints)
                {
                    ReportProgress(50, $"Aplicando sampling inteligente a {series.Name}...");
                    series = ApplyIntelligentSampling(series, MaxChartPoints);
                }

                if (series.Points.Any())
                    seriesList.Add(series);
            }

            return seriesList;
        }

        /// <summary>
        /// Applies intelligent sampling to reduce data points while preserving trends
        /// Uses LTTB (Largest Triangle Three Buckets) algorithm for optimal visualization
        /// </summary>
        private DataSeries ApplyIntelligentSampling(DataSeries series, int targetPoints)
        {
            if (series.Points.Count <= targetPoints)
                return series;

            var sampled = new DataSeries
            {
                Name = series.Name,
                SourceField = series.SourceField,
                Type = series.Type,
                Color = series.Color,
                Metadata = series.Metadata
            };

            // Always keep first and last points
            sampled.Points.Add(series.Points.First());

            // Calculate bucket size
            double bucketSize = (double)(series.Points.Count - 2) / (targetPoints - 2);

            // Sample middle points using LTTB algorithm
            int pointIndex = 0;
            for (int i = 0; i < targetPoints - 2; i++)
            {
                // Calculate average point in next bucket
                int avgRangeStart = (int)Math.Floor((i + 1) * bucketSize) + 1;
                int avgRangeEnd = (int)Math.Floor((i + 2) * bucketSize) + 1;
                avgRangeEnd = Math.Min(avgRangeEnd, series.Points.Count);

                double avgX = 0;
                double avgY = 0;
                int avgRangeLength = avgRangeEnd - avgRangeStart;

                for (; avgRangeStart < avgRangeEnd; avgRangeStart++)
                {
                    avgX += avgRangeStart;
                    avgY += series.Points[avgRangeStart].Value;
                }
                avgX /= avgRangeLength;
                avgY /= avgRangeLength;

                // Get range for current bucket
                int rangeOffs = (int)Math.Floor((i + 0) * bucketSize) + 1;
                int rangeTo = (int)Math.Floor((i + 1) * bucketSize) + 1;

                // Point before
                double pointAX = pointIndex;
                double pointAY = series.Points[pointIndex].Value;

                double maxArea = -1;
                int maxAreaPoint = rangeOffs;

                for (; rangeOffs < rangeTo; rangeOffs++)
                {
                    // Calculate triangle area
                    double area = Math.Abs(
                        (pointAX - avgX) * (series.Points[rangeOffs].Value - pointAY) -
                        (pointAX - rangeOffs) * (avgY - pointAY)
                    ) * 0.5;

                    if (area > maxArea)
                    {
                        maxArea = area;
                        maxAreaPoint = rangeOffs;
                    }
                }

                sampled.Points.Add(series.Points[maxAreaPoint]);
                pointIndex = maxAreaPoint;
            }

            sampled.Points.Add(series.Points.Last());

            // Add metadata about sampling
            sampled.Metadata["Sampled"] = true;
            sampled.Metadata["OriginalCount"] = series.Points.Count;
            sampled.Metadata["SampledCount"] = sampled.Points.Count;

            return sampled;
        }

        /// <summary>
        /// Reports progress to callback if configured
        /// </summary>
        private void ReportProgress(int percentage, string message)
        {
            ProgressCallback?.Invoke(percentage, message);
        }

        /// <summary>
        /// Builds aggregated series
        /// </summary>
        private DataSeries? BuildAggregatedSeries(List<Client> clients, FieldSelection field, QueryConfiguration config)
        {
            if (!field.Aggregation.HasValue || field.Aggregation.Value == AggregationFunction.None)
                return null;

            var values = clients
                .Select(c => GetFieldValue(c, field.FieldName))
                .Where(v => v != null && IsNumeric(v))
                .Select(v => Convert.ToDouble(v))
                .ToList();

            if (!values.Any())
                return null;

            double aggregatedValue = field.Aggregation.Value switch
            {
                AggregationFunction.Count => values.Count,
                AggregationFunction.Sum => values.Sum(),
                AggregationFunction.Average => values.Average(),
                AggregationFunction.Min => values.Min(),
                AggregationFunction.Max => values.Max(),
                AggregationFunction.Median => CalculateMedian(values),
                AggregationFunction.StdDev => CalculateStdDev(values),
                _ => 0
            };

            var series = new DataSeries
            {
                Name = $"{field.DisplayName} ({field.Aggregation.Value})",
                SourceField = field.FieldName,
                Type = SeriesType.Bar
            };

            series.Points.Add(new DataPoint
            {
                Label = field.Aggregation.Value.ToString(),
                Value = aggregatedValue
            });

            return series;
        }

        /// <summary>
        /// Builds human-readable query description
        /// </summary>
        private string BuildQueryDescription(QueryConfiguration config)
        {
            var sb = new StringBuilder();

            sb.Append("Análisis de ");

            if (config.SelectedFields.Any())
            {
                sb.Append(string.Join(", ", config.SelectedFields.Select(f => f.DisplayName)));
            }
            else
            {
                sb.Append("todos los campos");
            }

            if (!string.IsNullOrEmpty(config.GroupByField))
            {
                sb.Append($" agrupado por {config.GroupByField}");
            }

            if (config.Filters.Any())
            {
                sb.Append($" con {config.Filters.Count} filtro(s)");
            }

            if (config.Aggregation != AggregationFunction.None)
            {
                sb.Append($" usando {config.Aggregation}");
            }

            return sb.ToString();
        }

        // Helper methods
        private bool IsNumeric(object value)
        {
            return value is int or long or float or double or decimal;
        }

        private bool CompareValues(object? a, object? b, Func<object, object, bool> comparison)
        {
            if (a == null || b == null)
                return false;

            return comparison(a, b);
        }

        private bool CompareNumeric(object? a, object? b, Func<double, double, bool> comparison)
        {
            if (a == null || b == null || !IsNumeric(a) || !IsNumeric(b))
                return false;

            return comparison(Convert.ToDouble(a), Convert.ToDouble(b));
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

        private double CalculateStdDev(List<double> values)
        {
            if (values.Count < 2)
                return 0;

            double avg = values.Average();
            double sumOfSquares = values.Sum(v => Math.Pow(v - avg, 2));
            return Math.Sqrt(sumOfSquares / values.Count);
        }
    }
}
