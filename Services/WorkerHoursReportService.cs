using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using ReactCRM.Models;
using ReactCRM.Database;
using iTextColor = iText.Kernel.Colors.Color;

namespace ReactCRM.Services
{
    /// <summary>
    /// Service for generating professional PDF reports of worker hours
    /// </summary>
    public class WorkerHoursReportService
    {
        public enum ReportPeriod
        {
            Weekly,
            Biweekly,
            Monthly,
            Custom
        }

        /// <summary>
        /// Generate a worker hours PDF report
        /// </summary>
        /// <param name="workerId">Worker ID to generate report for</param>
        /// <param name="startDate">Start date of report period</param>
        /// <param name="endDate">End date of report period</param>
        /// <param name="hourlyRate">Hourly pay rate (optional)</param>
        /// <param name="period">Report period type</param>
        /// <returns>PDF byte array</returns>
        public byte[] GenerateReport(int workerId, DateTime startDate, DateTime endDate, decimal? hourlyRate, ReportPeriod period)
        {
            try
            {
                var workerRepo = new WorkerRepository();
                var timeEntryRepo = new TimeEntryRepository();

                var worker = workerRepo.GetWorkerById(workerId);
                if (worker == null)
                    throw new ArgumentException($"Worker with ID {workerId} not found.");

                // Get time entries for the period
                var entries = timeEntryRepo.GetWorkerEntries(workerId, startDate, endDate);

                // Use temporary file to avoid SmartMode MemoryStream issues
                string tempFile = Path.GetTempFileName();
                try
                {
                    using (var writer = new PdfWriter(tempFile))
                    using (var pdfDoc = new PdfDocument(writer))
                    using (var document = new Document(pdfDoc))
                    {

                        // Fonts
                        PdfFont boldFont;
                        PdfFont regularFont;

                        try
                        {
                            boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                            regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                        }
                        catch (Exception fontEx)
                        {
                            throw new Exception($"Error creating fonts: {fontEx.Message}", fontEx);
                        }

                        // === HEADER ===
                        var header = new Paragraph("WORKER HOURS REPORT")
                            .SetFont(boldFont)
                            .SetFontSize(20)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetMarginBottom(5);
                        document.Add(header);

                        // Horizontal line
                        var line = new Paragraph(new string('_', 100))
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFontSize(8)
                            .SetMarginBottom(15);
                        document.Add(line);

                        // === WORKER INFORMATION ===
                        var infoTable = new Table(UnitValue.CreatePercentArray(new float[] { 30, 70 }))
                            .UseAllAvailableWidth()
                            .SetMarginBottom(15);

                        AddInfoRow(infoTable, "Worker Name:", worker.Username, boldFont, regularFont);
                        AddInfoRow(infoTable, "Report Period:", GetPeriodLabel(period), boldFont, regularFont);
                        AddInfoRow(infoTable, "Date Range:", $"{startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}", boldFont, regularFont);
                        AddInfoRow(infoTable, "Report Generated:", DateTime.Now.ToString("MMM dd, yyyy hh:mm tt"), boldFont, regularFont);
                        if (hourlyRate.HasValue)
                        {
                            AddInfoRow(infoTable, "Hourly Rate:", $"${hourlyRate.Value:F2}/hour", boldFont, regularFont);
                        }

                        document.Add(infoTable);

                        // === TIME ENTRIES TABLE ===
                        var entriesHeader = new Paragraph("Time Entries")
                            .SetFont(boldFont)
                            .SetFontSize(14)
                            .SetMarginTop(10)
                            .SetMarginBottom(5);
                        document.Add(entriesHeader);

                        if (entries.Count == 0)
                        {
                            var noEntries = new Paragraph("No time entries found for this period.")
                                .SetFont(regularFont)
                                .SetFontSize(10)
                                .SetMarginBottom(20);
                            document.Add(noEntries);
                        }
                        else
                        {
                            // Create table for entries
                            var entriesTable = new Table(UnitValue.CreatePercentArray(new float[] { 15, 15, 15, 10, 10, 15, 20 }))
                                .UseAllAvailableWidth()
                                .SetMarginBottom(15);

                            // Header row
                            var headerBg = new DeviceRgb(52, 73, 94); // Dark blue-gray
                            AddHeaderCell(entriesTable, "Date", boldFont, headerBg);
                            AddHeaderCell(entriesTable, "Clock In", boldFont, headerBg);
                            AddHeaderCell(entriesTable, "Clock Out", boldFont, headerBg);
                            AddHeaderCell(entriesTable, "Break (min)", boldFont, headerBg);
                            AddHeaderCell(entriesTable, "Net Hours", boldFont, headerBg);
                            AddHeaderCell(entriesTable, "Status", boldFont, headerBg);
                            if (hourlyRate.HasValue)
                            {
                                AddHeaderCell(entriesTable, "Pay", boldFont, headerBg);
                            }
                            else
                            {
                                AddHeaderCell(entriesTable, "Notes", boldFont, headerBg);
                            }

                            // Data rows
                            decimal totalHours = 0;
                            decimal totalPay = 0;
                            bool alternateRow = false;

                            foreach (var entry in entries.OrderBy(e => e.Date))
                            {
                                var rowBg = alternateRow ? new DeviceRgb(245, 245, 245) : DeviceRgb.WHITE;
                                alternateRow = !alternateRow;

                                AddDataCell(entriesTable, entry.Date.ToString("ddd, MMM dd"), regularFont, rowBg);
                                AddDataCell(entriesTable, entry.ClockIn.ToString("hh:mm tt"), regularFont, rowBg);

                                string clockOut = entry.ClockOut.HasValue ? entry.ClockOut.Value.ToString("hh:mm tt") : "---";
                                AddDataCell(entriesTable, clockOut, regularFont, rowBg);

                                AddDataCell(entriesTable, entry.BreakMinutes.ToString(), regularFont, rowBg, TextAlignment.CENTER);

                                // Calculate hours worked
                                decimal hoursWorked = 0;
                                if (entry.ClockOut.HasValue)
                                {
                                    var timeSpan = entry.ClockOut.Value - entry.ClockIn;
                                    hoursWorked = (decimal)timeSpan.TotalHours - (entry.BreakMinutes / 60m);
                                    totalHours += hoursWorked;
                                }

                                AddDataCell(entriesTable, hoursWorked > 0 ? $"{hoursWorked:F2}" : "---", regularFont, rowBg, TextAlignment.RIGHT);

                                // Status
                                string status = entry.ClockOut.HasValue ? "Complete" : "Open";
                                AddDataCell(entriesTable, status, regularFont, rowBg, TextAlignment.CENTER);

                                // Pay or Notes
                                if (hourlyRate.HasValue && hoursWorked > 0)
                                {
                                    decimal pay = hoursWorked * hourlyRate.Value;
                                    totalPay += pay;
                                    AddDataCell(entriesTable, $"${pay:F2}", regularFont, rowBg, TextAlignment.RIGHT);
                                }
                                else
                                {
                                    AddDataCell(entriesTable, entry.Notes ?? "", regularFont, rowBg);
                                }
                            }

                            document.Add(entriesTable);

                            // === TOTALS SECTION ===
                            var totalsTable = new Table(UnitValue.CreatePercentArray(new float[] { 70, 30 }))
                                .UseAllAvailableWidth()
                                .SetMarginTop(10)
                                .SetMarginBottom(20);

                            var totalsBg = new DeviceRgb(46, 204, 113); // Green

                            // Total hours
                            var totalHoursLabelCell = new Cell()
                                .Add(new Paragraph("TOTAL HOURS WORKED:")
                                    .SetFont(boldFont)
                                    .SetFontSize(12)
                                    .SetTextAlignment(TextAlignment.RIGHT))
                                .SetBackgroundColor(totalsBg)
                                .SetFontColor(DeviceRgb.WHITE)
                                .SetPadding(10)
                                .SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                            totalsTable.AddCell(totalHoursLabelCell);

                            var totalHoursValueCell = new Cell()
                                .Add(new Paragraph($"{totalHours:F2} hrs")
                                    .SetFont(boldFont)
                                    .SetFontSize(12)
                                    .SetTextAlignment(TextAlignment.CENTER))
                                .SetBackgroundColor(totalsBg)
                                .SetFontColor(DeviceRgb.WHITE)
                                .SetPadding(10)
                                .SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                            totalsTable.AddCell(totalHoursValueCell);

                            // Total pay if applicable
                            if (hourlyRate.HasValue)
                            {
                                var totalPayLabelCell = new Cell()
                                    .Add(new Paragraph("TOTAL PAY:")
                                        .SetFont(boldFont)
                                        .SetFontSize(12)
                                        .SetTextAlignment(TextAlignment.RIGHT))
                                    .SetBackgroundColor(new DeviceRgb(52, 152, 219)) // Blue
                                    .SetFontColor(DeviceRgb.WHITE)
                                    .SetPadding(10)
                                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                                totalsTable.AddCell(totalPayLabelCell);

                                var totalPayValueCell = new Cell()
                                    .Add(new Paragraph($"${totalPay:F2}")
                                        .SetFont(boldFont)
                                        .SetFontSize(12)
                                        .SetTextAlignment(TextAlignment.CENTER))
                                    .SetBackgroundColor(new DeviceRgb(52, 152, 219))
                                    .SetFontColor(DeviceRgb.WHITE)
                                    .SetPadding(10)
                                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                                totalsTable.AddCell(totalPayValueCell);

                                // Average hours per day
                                int daysInPeriod = (endDate - startDate).Days + 1;
                                decimal avgHoursPerDay = totalHours / daysInPeriod;

                                var avgLabelCell = new Cell()
                                    .Add(new Paragraph("AVERAGE HOURS PER DAY:")
                                        .SetFont(boldFont)
                                        .SetFontSize(10)
                                        .SetTextAlignment(TextAlignment.RIGHT))
                                    .SetBackgroundColor(new DeviceRgb(149, 165, 166)) // Gray
                                    .SetFontColor(DeviceRgb.WHITE)
                                    .SetPadding(8)
                                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                                totalsTable.AddCell(avgLabelCell);

                                var avgValueCell = new Cell()
                                    .Add(new Paragraph($"{avgHoursPerDay:F2} hrs/day")
                                        .SetFont(regularFont)
                                        .SetFontSize(10)
                                        .SetTextAlignment(TextAlignment.CENTER))
                                    .SetBackgroundColor(new DeviceRgb(149, 165, 166))
                                    .SetFontColor(DeviceRgb.WHITE)
                                    .SetPadding(8)
                                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                                totalsTable.AddCell(avgValueCell);
                            }

                            document.Add(totalsTable);
                        }

                        // === FOOTER ===
                        var footer = new Paragraph($"Report generated on {DateTime.Now:MMMM dd, yyyy} at {DateTime.Now:hh:mm tt}")
                            .SetFont(regularFont)
                            .SetFontSize(8)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFontColor(new DeviceRgb(127, 140, 141))
                            .SetMarginTop(20);
                        document.Add(footer);
                    }

                    // Read the temp file into memory
                    byte[] pdfBytes = File.ReadAllBytes(tempFile);

                    // Clean up temp file with retry logic
                    try
                    {
                        if (File.Exists(tempFile))
                        {
                            File.Delete(tempFile);
                        }
                    }
                    catch
                    {
                        // If delete fails, try again after a short delay
                        System.Threading.Thread.Sleep(100);
                        try
                        {
                            if (File.Exists(tempFile))
                            {
                                File.Delete(tempFile);
                            }
                        }
                        catch
                        {
                            // If still fails, just ignore - OS will clean up temp files eventually
                        }
                    }

                    return pdfBytes;
                }
                catch (Exception innerEx)
                {
                    // Provide very detailed error information
                    string detailedError = $"Error in PDF generation: {innerEx.GetType().Name}\n" +
                                          $"Message: {innerEx.Message}\n" +
                                          $"Stack Trace: {innerEx.StackTrace}";

                    if (innerEx.InnerException != null)
                    {
                        detailedError += $"\nInner Exception: {innerEx.InnerException.GetType().Name}\n" +
                                       $"Inner Message: {innerEx.InnerException.Message}";
                    }

                    throw new Exception(detailedError, innerEx);
                }
            }
            catch (Exception ex)
            {
                // Provide detailed error information
                string errorMsg = $"PDF Generation Error: {ex.GetType().Name}\n" +
                                 $"Message: {ex.Message}\n" +
                                 $"Stack Trace: {ex.StackTrace}";

                if (ex.InnerException != null)
                {
                    errorMsg += $"\nInner Exception: {ex.InnerException.GetType().Name}\n" +
                               $"Inner Message: {ex.InnerException.Message}";
                }

                throw new Exception(errorMsg, ex);
            }
        }

        private void AddInfoRow(Table table, string label, string value, PdfFont boldFont, PdfFont regularFont)
        {
            var labelCell = new Cell()
                .Add(new Paragraph(label).SetFont(boldFont).SetFontSize(10))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetPaddingBottom(3);
            table.AddCell(labelCell);

            var valueCell = new Cell()
                .Add(new Paragraph(value).SetFont(regularFont).SetFontSize(10))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetPaddingBottom(3);
            table.AddCell(valueCell);
        }

        private void AddHeaderCell(Table table, string text, PdfFont font, iTextColor bgColor)
        {
            var cell = new Cell()
                .Add(new Paragraph(text)
                    .SetFont(font)
                    .SetFontSize(9)
                    .SetTextAlignment(TextAlignment.CENTER))
                .SetBackgroundColor(bgColor)
                .SetFontColor(DeviceRgb.WHITE)
                .SetPadding(5);
            table.AddCell(cell);
        }

        private void AddDataCell(Table table, string text, PdfFont font, iTextColor bgColor, TextAlignment alignment = TextAlignment.LEFT)
        {
            var cell = new Cell()
                .Add(new Paragraph(text)
                    .SetFont(font)
                    .SetFontSize(8)
                    .SetTextAlignment(alignment))
                .SetBackgroundColor(bgColor)
                .SetPadding(4);
            table.AddCell(cell);
        }

        private string GetPeriodLabel(ReportPeriod period)
        {
            return period switch
            {
                ReportPeriod.Weekly => "Weekly",
                ReportPeriod.Biweekly => "Bi-weekly (2 weeks)",
                ReportPeriod.Monthly => "Monthly",
                ReportPeriod.Custom => "Custom Date Range",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Calculate date range for a specific period ending on a given date
        /// </summary>
        public static (DateTime startDate, DateTime endDate) CalculatePeriodRange(ReportPeriod period, DateTime endDate)
        {
            DateTime startDate = period switch
            {
                ReportPeriod.Weekly => endDate.AddDays(-6),
                ReportPeriod.Biweekly => endDate.AddDays(-13),
                ReportPeriod.Monthly => endDate.AddMonths(-1).AddDays(1),
                _ => endDate
            };

            return (startDate, endDate);
        }
    }
}