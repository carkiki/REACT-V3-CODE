using System;

namespace ReactCRM.Models
{
    /// <summary>
    /// Represents a task/todo item in the CRM system
    /// Can be assigned to the team (global) or to specific clients
    /// </summary>
    public class TodoTask
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// Priority level: 1=Low, 2=Medium, 3=High, 4=Urgent
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Status: Pending, InProgress, Completed, Cancelled
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Due date for the task
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Date when task was created
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Date when task was completed
        /// </summary>
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// User ID who created the task
        /// </summary>
        public int CreatedByUserId { get; set; }

        /// <summary>
        /// User ID assigned to complete the task
        /// </summary>
        public int? AssignedToUserId { get; set; }

        /// <summary>
        /// Client ID if this task is client-specific, null for global team tasks
        /// </summary>
        public int? ClientId { get; set; }

        /// <summary>
        /// Task category/type
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Additional notes or comments
        /// </summary>
        public string Notes { get; set; }

        public bool IsActive { get; set; }
        public string ClientName { get; set; }

        public TodoTask()
        {
            CreatedDate = DateTime.Now;
            Status = "Pending";
            Priority = 2; // Medium by default
            IsActive = true;
        }

        /// <summary>
        /// Gets the priority color based on priority level
        /// </summary>
        public System.Drawing.Color GetPriorityColor()
        {
            return Priority switch
            {
                1 => System.Drawing.Color.FromArgb(76, 175, 80),   // Green - Low
                2 => System.Drawing.Color.FromArgb(33, 150, 243),  // Blue - Medium
                3 => System.Drawing.Color.FromArgb(255, 152, 0),   // Orange - High
                4 => System.Drawing.Color.FromArgb(244, 67, 54),   // Red - Urgent
                _ => System.Drawing.Color.Gray
            };
        }

        /// <summary>
        /// Gets the priority label
        /// </summary>
        public string GetPriorityLabel()
        {
            return Priority switch
            {
                1 => "Low",
                2 => "Medium",
                3 => "High",
                4 => "Urgent",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Check if task is overdue
        /// </summary>
        public bool IsOverdue()
        {
            return DueDate.HasValue &&
                   DueDate.Value < DateTime.Now &&
                   Status != "Completed" &&
                   Status != "Cancelled";
        }
    }
}
