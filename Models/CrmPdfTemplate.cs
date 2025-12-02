using System;

using System.Collections.Generic;



namespace ReactCRM.Models

{

    /// <summary>

    /// Wrapper class for PDF template data used during PDF generation

    /// </summary>

    public class CrmPdfTemplate

    {

        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public Dictionary<string, string> FieldMappings { get; set; } = new Dictionary<string, string>();

        public List<PdfFormField> FormFields { get; set; } = new List<PdfFormField>();

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }



        public CrmPdfTemplate()

        {

        }



        public CrmPdfTemplate(PdfTemplate template)

        {

            if (template != null)

            {

                Id = template.Id;

                Name = template.Name;

                Description = template.Description;

                Category = template.Category;

                FileName = template.FileName;

                FilePath = template.FilePath;

                FileSize = template.FileSize;

                FieldMappings = template.GetFieldMappings();

                IsActive = template.IsActive;

                CreatedAt = template.CreatedAt;

                UpdatedAt = template.UpdatedAt;

            }

        }



        public PdfTemplate ToPdfTemplate()

        {

            var template = new PdfTemplate

            {

                Id = this.Id,

                Name = this.Name,

                Description = this.Description,

                Category = this.Category,

                FileName = this.FileName,

                FilePath = this.FilePath,

                FileSize = this.FileSize,

                IsActive = this.IsActive,

                CreatedAt = this.CreatedAt,

                UpdatedAt = this.UpdatedAt

            };

            template.SetFieldMappings(this.FieldMappings);

            return template;
        }


        // Implicit conversion from PdfTemplate to CrmPdfTemplate

        public static implicit operator CrmPdfTemplate(PdfTemplate template)

        {

            return new CrmPdfTemplate(template);

        }



        // Implicit conversion from CrmPdfTemplate to PdfTemplate

        public static implicit operator PdfTemplate(CrmPdfTemplate crmTemplate)

        {

            return crmTemplate.ToPdfTemplate();

        }

    }

    }