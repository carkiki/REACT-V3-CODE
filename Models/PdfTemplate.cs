using System;

using System.Collections.Generic;

using System.Text.Json;



namespace ReactCRM.Models

{

    public class PdfTemplate

    {

        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public string FieldMappings { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }



        public PdfTemplate()

        {

        }



        public PdfTemplate(int id, string name, string description, string category,

            string fileName, string filePath, long fileSize, string fieldMappings, bool isActive)

        {

            Id = id;

            Name = name;

            Description = description;

            Category = category;

            FileName = fileName;

            FilePath = filePath;

            FileSize = fileSize;

            FieldMappings = fieldMappings;

            IsActive = isActive;

        }



        public Dictionary<string, string> GetFieldMappings()

        {

            if (string.IsNullOrEmpty(FieldMappings))

                return new Dictionary<string, string>();



            try

            {

                return JsonSerializer.Deserialize<Dictionary<string, string>>(FieldMappings)

                    ?? new Dictionary<string, string>();

            }

            catch

            {

                return new Dictionary<string, string>();

            }

        }



        public void SetFieldMappings(Dictionary<string, string> mappings)

        {

            FieldMappings = JsonSerializer.Serialize(mappings);

        }

    }

}
