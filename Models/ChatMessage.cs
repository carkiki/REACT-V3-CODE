using System;

namespace ReactCRM.Models
{
    /// <summary>
    /// Represents a chat message in the team chat system
    /// </summary>
    public class ChatMessage
    {
        public int Id { get; set; }

        /// <summary>
        /// User ID who sent the message
        /// </summary>
        public int SenderId { get; set; }

        /// <summary>
        /// Username of sender (populated from join)
        /// </summary>
        public string SenderName { get; set; }

        /// <summary>
        /// Message content
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// When the message was sent
        /// </summary>
        public DateTime SentDate { get; set; }

        /// <summary>
        /// Chat channel/room (e.g., "general", "support", "sales")
        /// </summary>
        public string Channel { get; set; }

        /// <summary>
        /// Message type: Text, System, File, Image
        /// </summary>
        public string MessageType { get; set; }

        /// <summary>
        /// Related entity ID (e.g., Client ID, Task ID)
        /// </summary>
        public int? RelatedEntityId { get; set; }

        /// <summary>
        /// Related entity type (e.g., "Client", "Task")
        /// </summary>
        public string RelatedEntityType { get; set; }

        /// <summary>
        /// If message is pinned to top of channel
        /// </summary>
        public bool IsPinned { get; set; }

        /// <summary>
        /// If message has been edited
        /// </summary>
        public bool IsEdited { get; set; }

        /// <summary>
        /// When message was last edited
        /// </summary>
        public DateTime? EditedDate { get; set; }

        /// <summary>
        /// File path if message contains attachment
        /// </summary>
        public string AttachmentPath { get; set; }

        /// <summary>
        /// Reply to another message ID
        /// </summary>
        public int? ReplyToMessageId { get; set; }

        public bool IsActive { get; set; }

        public ChatMessage()
        {
            SentDate = DateTime.Now;
            Channel = "general";
            MessageType = "Text";
            IsActive = true;
            IsPinned = false;
            IsEdited = false;
        }

        /// <summary>
        /// Get formatted time for display
        /// </summary>
        public string GetFormattedTime()
        {
            var now = DateTime.Now;
            var timeSpan = now - SentDate;

            if (SentDate.Date == now.Date)
            {
                return SentDate.ToString("HH:mm"); // Today: show time
            }
            else if (SentDate.Date == now.Date.AddDays(-1))
            {
                return "Yesterday " + SentDate.ToString("HH:mm");
            }
            else if (timeSpan.TotalDays < 7)
            {
                return SentDate.ToString("ddd HH:mm"); // This week: show day and time
            }
            else
            {
                return SentDate.ToString("MMM dd"); // Older: show date
            }
        }

        /// <summary>
        /// Check if message is from today
        /// </summary>
        public bool IsToday()
        {
            return SentDate.Date == DateTime.Today;
        }

        /// <summary>
        /// Get message type icon
        /// </summary>
        public string GetMessageIcon()
        {
            return MessageType switch
            {
                "System" => "⚙️",
                "File" => "📎",
                "Image" => "🖼️",
                _ => ""
            };
        }
    }
}
