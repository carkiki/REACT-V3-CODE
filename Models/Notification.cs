using System;

namespace ReactCRM.Models
{
    /// <summary>
    /// Represents a system notification
    /// </summary>
    public class Notification
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        /// <summary>
        /// Type: Info, Success, Warning, Error, Task, Birthday, System
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// User ID this notification is for
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Whether the notification has been read
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// Date when notification was created
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Date when notification was read
        /// </summary>
        public DateTime? ReadDate { get; set; }

        /// <summary>
        /// Related entity ID (e.g., Task ID, Client ID)
        /// </summary>
        public int? RelatedEntityId { get; set; }

        /// <summary>
        /// Related entity type (e.g., "Task", "Client", "Worker")
        /// </summary>
        public string RelatedEntityType { get; set; }

        /// <summary>
        /// Icon to display (Unicode emoji)
        /// </summary>
        public string Icon { get; set; }

        public bool IsActive { get; set; }

        public Notification()
        {
            CreatedDate = DateTime.Now;
            IsRead = false;
            IsActive = true;
            Type = "Info";
        }

        /// <summary>
        /// Gets the notification color based on type
        /// </summary>
        public System.Drawing.Color GetTypeColor()
        {
            return Type switch
            {
                "Success" => System.Drawing.Color.FromArgb(76, 175, 80),
                "Warning" => System.Drawing.Color.FromArgb(255, 152, 0),
                "Error" => System.Drawing.Color.FromArgb(244, 67, 54),
                "Task" => System.Drawing.Color.FromArgb(142, 84, 186),
                "Birthday" => System.Drawing.Color.FromArgb(233, 30, 99),
                "Info" => System.Drawing.Color.FromArgb(33, 150, 243),
                _ => System.Drawing.Color.Gray
            };
        }

        /// <summary>
        /// Gets default icon for notification type if none specified
        /// </summary>
        public string GetDefaultIcon()
        {
            if (!string.IsNullOrEmpty(Icon))
                return Icon;

            return Type switch
            {
                "Success" => "✅",
                "Warning" => "⚠️",
                "Error" => "❌",
                "Task" => "📋",
                "Birthday" => "🎂",
                "Info" => "ℹ️",
                "System" => "⚙️",
                _ => "🔔"
            };
        }
    }
}
