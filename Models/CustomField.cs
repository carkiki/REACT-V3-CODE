using System;

namespace ReactCRM.Models
{
    public class CustomField
    {
        public int Id { get; set; }
        public string FieldName { get; set; }
        public string Label { get; set; }
        public string FieldType { get; set; }
        public string Options { get; set; } // JSON array for dropdown options
        public bool IsRequired { get; set; }
        public string DefaultValue { get; set; }
        public DateTime CreatedAt { get; set; }

        // Field type constants - must match database CHECK constraint (lowercase)
        public const string TYPE_TEXT = "text";
        public const string TYPE_NUMBER = "number";
        public const string TYPE_DATE = "date";
        public const string TYPE_DROPDOWN = "dropdown";
        public const string TYPE_CHECKBOX = "checkbox";
    }
}