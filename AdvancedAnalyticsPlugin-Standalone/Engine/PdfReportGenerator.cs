using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ReactCRM.Plugins.AdvancedAnalytics.Models;

namespace ReactCRM.Plugins.AdvancedAnalytics.Engine
{
    /// <summary>
    /// PDF report generator with embedded charts and analytics
    /// </summary>
    public class PdfReportGenerator
    {
        private readonly AnalyticsResult _result;
        private readonly List<Bitmap> _chartImages;
        private readonly ChartConfiguration _config;

        public PdfReportGenerator(AnalyticsResult result, ChartConfiguration config)
        {
            _result = result;
            _config = config;
            _chartImages = new List<Bitmap>();

            // Configure QuestPDF license (Community license for free use)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        /// <summary>
        /// Adds a chart image to the report
        /// </summary>
        public void AddChartImage(Bitmap chartImage)
        {
            _chartImages.Add(chartImage);
        }

        /// <summary>
        /// Generates the PDF report
        /// </summary>
        public void GenerateReport(string outputPath)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(40);
                    page.PageColor(QuestPDF.Helpers.Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().Element(ComposeFooter);
                });
            });

            document.GeneratePdf(outputPath);
        }

        /// <summary>
        /// Composes the report header
        /// </summary>
        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("REACT CRM")
                        .FontSize(20)
                        .Bold()
                        .FontColor(QuestPDF.Helpers.Colors.Blue.Darken2);

                    column.Item().Text("Reporte de Análisis Avanzado")
                        .FontSize(14)
                        .SemiBold()
                        .FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);

                    column.Item().Text($"{_config.Title}")
                        .FontSize(12)
                        .FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                });

                row.ConstantItem(100).Height(50).Placeholder();
            });
        }

        /// <summary>
        /// Composes the main content
        /// </summary>
        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(10).Column(column =>
            {
                column.Spacing(15);

                // Executive Summary
                column.Item().Element(ComposeExecutiveSummary);

                // Statistics Overview
                column.Item().Element(ComposeStatistics);

                // Charts
                if (_chartImages.Any())
                {
                    column.Item().Element(ComposeCharts);
                }

                // Insights
                if (_result.Insights.Any())
                {
                    column.Item().Element(ComposeInsights);
                }

                // Detailed Data Tables
                column.Item().Element(ComposeDataTables);
            });
        }

        /// <summary>
        /// Composes executive summary section
        /// </summary>
        private void ComposeExecutiveSummary(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text("Resumen Ejecutivo")
                    .FontSize(16)
                    .Bold()
                    .FontColor(QuestPDF.Helpers.Colors.Blue.Darken2);

                column.Item().PaddingTop(8).PaddingBottom(4).LineHorizontal(1)
                    .LineColor(QuestPDF.Helpers.Colors.Grey.Lighten2);

                column.Item().PaddingTop(8).Text(text =>
                {
                    text.Span("Fecha de generación: ").SemiBold();
                    text.Span(_result.GeneratedAt.ToString("dd/MM/yyyy HH:mm:ss"));
                    text.Line("");

                    text.Span("Consulta: ").SemiBold();
                    text.Span(_result.QueryDescription);
                    text.Line("");

                    text.Span("Registros analizados: ").SemiBold();
                    text.Span(_result.TotalRecordsAnalyzed.ToString("N0"));
                    text.Line("");

                    text.Span("Tiempo de ejecución: ").SemiBold();
                    text.Span($"{_result.ExecutionTime.TotalMilliseconds:F2} ms");
                    text.Line("");

                    text.Span("Series de datos: ").SemiBold();
                    text.Span(_result.Series.Count.ToString());
                });
            });
        }

        /// <summary>
        /// Composes statistics overview
        /// </summary>
        private void ComposeStatistics(IContainer container)
        {
            var stats = _result.GetOverallStatistics();

            container.Column(column =>
            {
                column.Item().Text("Estadísticas Generales")
                    .FontSize(16)
                    .Bold()
                    .FontColor(QuestPDF.Helpers.Colors.Blue.Darken2);

                column.Item().PaddingTop(8).PaddingBottom(4).LineHorizontal(1)
                    .LineColor(QuestPDF.Helpers.Colors.Grey.Lighten2);

                column.Item().PaddingTop(8).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    // Header
                    table.Cell().Element(CellStyle).Text("Métrica").Bold();
                    table.Cell().Element(CellStyle).Text("Valor").Bold();
                    table.Cell().Element(CellStyle).Text("Descripción").Bold();

                    // Data rows
                    table.Cell().Element(CellStyle).Text("Total de puntos");
                    table.Cell().Element(CellStyle).Text(stats.TotalDataPoints.ToString("N0"));
                    table.Cell().Element(CellStyle).Text("Cantidad total de datos");

                    table.Cell().Element(CellStyle).Text("Promedio global");
                    table.Cell().Element(CellStyle).Text(stats.GlobalAverage.ToString("N2"));
                    table.Cell().Element(CellStyle).Text("Media de todos los valores");

                    table.Cell().Element(CellStyle).Text("Valor mínimo");
                    table.Cell().Element(CellStyle).Text(stats.GlobalMin?.ToString("N2") ?? "N/A");
                    table.Cell().Element(CellStyle).Text("Menor valor registrado");

                    table.Cell().Element(CellStyle).Text("Valor máximo");
                    table.Cell().Element(CellStyle).Text(stats.GlobalMax?.ToString("N2") ?? "N/A");
                    table.Cell().Element(CellStyle).Text("Mayor valor registrado");

                    table.Cell().Element(CellStyle).Text("Rango");
                    table.Cell().Element(CellStyle).Text(stats.GlobalRange.ToString("N2"));
                    table.Cell().Element(CellStyle).Text("Diferencia entre máx y mín");
                });

                // Per-series statistics
                column.Item().PaddingTop(15).Text("Estadísticas por Serie")
                    .FontSize(14)
                    .SemiBold();

                foreach (var series in _result.Series)
                {
                    var seriesStats = series.GetStatistics();

                    column.Item().PaddingTop(10).Column(seriesColumn =>
                    {
                        seriesColumn.Item().Text(series.Name)
                            .FontSize(12)
                            .SemiBold()
                            .FontColor(QuestPDF.Helpers.Colors.Blue.Medium);

                        seriesColumn.Item().PaddingTop(5).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Cell().Element(CellStyleSmall).Text("Conteo").Bold();
                            table.Cell().Element(CellStyleSmall).Text("Promedio").Bold();
                            table.Cell().Element(CellStyleSmall).Text("Desv. Est.").Bold();
                            table.Cell().Element(CellStyleSmall).Text("Mediana").Bold();

                            table.Cell().Element(CellStyleSmall).Text(seriesStats.Count.ToString("N0"));
                            table.Cell().Element(CellStyleSmall).Text(seriesStats.Average.ToString("N2"));
                            table.Cell().Element(CellStyleSmall).Text(seriesStats.StdDev.ToString("N2"));
                            table.Cell().Element(CellStyleSmall).Text(seriesStats.Median.ToString("N2"));
                        });
                    });
                }
            });
        }

        /// <summary>
        /// Composes charts section
        /// </summary>
        private void ComposeCharts(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().PageBreak();

                column.Item().Text("Visualizaciones")
                    .FontSize(16)
                    .Bold()
                    .FontColor(QuestPDF.Helpers.Colors.Blue.Darken2);

                column.Item().PaddingTop(8).PaddingBottom(4).LineHorizontal(1)
                    .LineColor(QuestPDF.Helpers.Colors.Grey.Lighten2);

                foreach (var chartImage in _chartImages)
                {
                    column.Item().PaddingTop(15).Image(ConvertBitmapToBytes(chartImage))
                        .FitArea();
                }
            });
        }

        /// <summary>
        /// Composes insights section
        /// </summary>
        private void ComposeInsights(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().PageBreak();

                column.Item().Text("Insights y Recomendaciones")
                    .FontSize(16)
                    .Bold()
                    .FontColor(QuestPDF.Helpers.Colors.Blue.Darken2);

                column.Item().PaddingTop(8).PaddingBottom(4).LineHorizontal(1)
                    .LineColor(QuestPDF.Helpers.Colors.Grey.Lighten2);

                foreach (var insight in _result.Insights)
                {
                    column.Item().PaddingTop(10).Border(1)
                        .BorderColor(GetInsightColor(insight.Severity))
                        .Padding(10)
                        .Column(insightColumn =>
                        {
                            insightColumn.Item().Row(row =>
                            {
                                row.RelativeItem().Text(insight.Title)
                                    .FontSize(12)
                                    .SemiBold()
                                    .FontColor(GetInsightColor(insight.Severity));

                                row.ConstantItem(80).Text(insight.Type.ToString())
                                    .FontSize(9)
                                    .Italic()
                                    .FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                            });

                            insightColumn.Item().PaddingTop(5).Text(insight.Description)
                                .FontSize(10);

                            if (insight.Data.Any())
                            {
                                insightColumn.Item().PaddingTop(5).Text(text =>
                                {
                                    text.Span("Datos: ").FontSize(9).SemiBold();
                                    text.Span(string.Join(", ", insight.Data.Select(kvp => $"{kvp.Key}: {kvp.Value}")))
                                        .FontSize(9)
                                        .Italic();
                                });
                            }
                        });
                }
            });
        }

        /// <summary>
        /// Composes data tables section
        /// </summary>
        private void ComposeDataTables(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().PageBreak();

                column.Item().Text("Datos Detallados")
                    .FontSize(16)
                    .Bold()
                    .FontColor(QuestPDF.Helpers.Colors.Blue.Darken2);

                column.Item().PaddingTop(8).PaddingBottom(4).LineHorizontal(1)
                    .LineColor(QuestPDF.Helpers.Colors.Grey.Lighten2);

                foreach (var series in _result.Series)
                {
                    if (series.Points.Count > 100)
                    {
                        column.Item().PaddingTop(10).Text($"{series.Name} (mostrando primeros 100 registros)")
                            .FontSize(12)
                            .SemiBold();
                    }
                    else
                    {
                        column.Item().PaddingTop(10).Text(series.Name)
                            .FontSize(12)
                            .SemiBold();
                    }

                    column.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Cell().Element(CellStyle).Text("#").Bold();
                        table.Cell().Element(CellStyle).Text("Etiqueta").Bold();
                        table.Cell().Element(CellStyle).Text("Valor").Bold();

                        int count = 0;
                        foreach (var point in series.Points.Take(100))
                        {
                            count++;
                            table.Cell().Element(CellStyle).Text(count.ToString());
                            table.Cell().Element(CellStyle).Text(point.Label);
                            table.Cell().Element(CellStyle).Text(point.Value.ToString("N2"));
                        }
                    });
                }
            });
        }

        /// <summary>
        /// Composes the footer
        /// </summary>
        private void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(text =>
            {
                text.Span("Página ");
                text.CurrentPageNumber();
                text.Span(" de ");
                text.TotalPages();
                text.Span(" | Generado por REACT CRM Advanced Analytics Plugin");
            }).FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
        }

        // Helper methods

        private IContainer CellStyle(IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2)
                .Padding(5);
        }

        private IContainer CellStyleSmall(IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2)
                .Padding(3);
        }

        private string GetInsightColor(InsightSeverity severity)
        {
            return severity switch
            {
                InsightSeverity.Critical => "#e74c3c",
                InsightSeverity.Warning => "#f39c12",
                InsightSeverity.Positive => "#27ae60",
                _ => "#3498db"
            };
        }

        private byte[] ConvertBitmapToBytes(Bitmap bitmap)
        {
            using var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }
    }
}
