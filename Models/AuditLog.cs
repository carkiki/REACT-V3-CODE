using System;

namespace ReactCRM.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string User { get; set; }
        public string ActionType { get; set; }
        public string Entity { get; set; }
        public int? EntityId { get; set; }
        public string Description { get; set; }
        public string Metadata { get; set; }
        public DateTime Timestamp { get; set; }

        public AuditLog()
        {
            Timestamp = DateTime.Now;
        }
    }
}