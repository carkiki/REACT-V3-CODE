using System;

namespace ReactCRM.Models
{
    /// <summary>
    /// Represents a tab/group for organizing clients
    /// </summary>
    public class ClientTab
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
    }
}
