using System;

namespace ReactCRM.Models
{
    /// <summary>
    /// Represents a time tracking entry for workers
    /// Supports multiple clock-in/clock-out entries per day
    /// </summary>
    public class TimeEntry
    {
        public int Id { get; set; }

        /// <summary>
        /// Worker/User ID who clocked in
        /// </summary>
        public int WorkerId { get; set; }

        /// <summary>
        /// Date of the time entry
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Clock-in time
        /// </summary>
        public DateTime ClockIn { get; set; }

        /// <summary>
        /// Clock-out time (null if still clocked in)
        /// </summary>
        public DateTime? ClockOut { get; set; }

        /// <summary>
        /// Break duration in minutes
        /// </summary>
        public int BreakMinutes { get; set; }

        /// <summary>
        /// Entry type: Regular, Overtime, Break
        /// </summary>
        public string EntryType { get; set; }

        /// <summary>
        /// Notes or description for this entry
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Location or department
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Whether entry has been approved by supervisor
        /// </summary>
        public bool IsApproved { get; set; }

        /// <summary>
        /// User ID who approved the entry
        /// </summary>
        public int? ApprovedByUserId { get; set; }

        /// <summary>
        /// Date when entry was approved
        /// </summary>
        public DateTime? ApprovedDate { get; set; }

        public bool IsActive { get; set; }

        public TimeEntry()
        {
            Date = DateTime.Today;
            ClockIn = DateTime.Now;
            EntryType = "Regular";
            IsActive = true;
            IsApproved = false;
            BreakMinutes = 0;
        }

        /// <summary>
        /// Calculate total hours worked (excluding breaks)
        /// </summary>
        public double GetTotalHours()
        {
            if (!ClockOut.HasValue)
                return 0;

            TimeSpan duration = ClockOut.Value - ClockIn;
            double totalMinutes = duration.TotalMinutes - BreakMinutes;
            return totalMinutes / 60.0;
        }

        /// <summary>
        /// Get formatted duration string
        /// </summary>
        public string GetDurationString()
        {
            if (!ClockOut.HasValue)
                return "In Progress";

            double hours = GetTotalHours();
            int wholeHours = (int)hours;
            int minutes = (int)((hours - wholeHours) * 60);

            return $"{wholeHours}h {minutes}m";
        }

        /// <summary>
        /// Check if worker is currently clocked in
        /// </summary>
        public bool IsClockedIn()
        {
            return !ClockOut.HasValue;
        }
    }
}
