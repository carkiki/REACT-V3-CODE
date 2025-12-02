using Newtonsoft.Json;

using ReactCRM.Database;

using ReactCRM.Models;

using iText.Kernel.Pdf;

using iText.Forms;

using iText.Forms.Fields;

using iText.Kernel.Colors;

using System;

using System.Collections.Generic;

using System.IO;

using PdfTemplateModel = ReactCRM.Models.PdfTemplate;

using PdfFormFieldModel = ReactCRM.Models.PdfFormField;

using iTextRectangle = iText.Kernel.Geom.Rectangle;



namespace ReactCRM.Services

{

    public class PdfTemplateService

    {

        private static PdfTemplateService? _instance;

        private static readonly object _lock = new();

        private readonly PdfTemplateRepository _repository;

        private const string TEMPLATES_FOLDER = "Templates";



        public static PdfTemplateService Instance

        {

            get

            {

                if (_instance == null)

                {

                    lock (_lock)

                    {

                        _instance ??= new PdfTemplateService();

                    }

                }

                return _instance;

            }

        }



        private PdfTemplateService()

        {

            _repository = new PdfTemplateRepository();

            EnsureTemplatesFolderExists();

        }



        private void EnsureTemplatesFolderExists()

        {

            if (!Directory.Exists(TEMPLATES_FOLDER))

            {

                Directory.CreateDirectory(TEMPLATES_FOLDER);

            }

        }



        #region Template Management



        public List<PdfTemplateModel> GetAllTemplates() => _repository.GetAllTemplates();

        public List<PdfTemplateModel> GetActiveTemplates() => _repository.GetActiveTemplates();

        public List<string> GetCategories() => _repository.GetCategories();

        public CrmPdfTemplate? GetTemplateById(int id) => _repository.GetTemplateById(id);



        public int ImportTemplate(string sourcePath, string name, string? description, string? category, int createdByUserId)

        {

            if (!File.Exists(sourcePath))

                throw new FileNotFoundException("Source PDF file not found.", sourcePath);



            // Generate unique file name

            string fileName = $"{Guid.NewGuid()}_{System.IO.Path.GetFileName(sourcePath)}";

            string destPath = System.IO.Path.Combine(TEMPLATES_FOLDER, fileName);



            // Copy file to templates folder

            File.Copy(sourcePath, destPath, true);



            var fileInfo = new FileInfo(destPath);



            // Detect form fields in the PDF

            var fieldMappings = DetectFormFields(destPath);



            var template = new PdfTemplateModel

            {

                Name = name,

                Description = description ?? string.Empty,

                Category = category ?? string.Empty,

                FileName = System.IO.Path.GetFileName(sourcePath),

                FilePath = destPath,

                FileSize = fileInfo.Length,

                FieldMappings = JsonConvert.SerializeObject(fieldMappings),

                IsActive = true

            };



            return _repository.CreateTemplate(template, createdByUserId);

        }



        public bool UpdateTemplate(CrmPdfTemplate template, int updatedByUserId)

        {

            return _repository.UpdateTemplate(template, updatedByUserId);

        }



        public bool DeleteTemplate(int templateId, int deletedByUserId)

        {

            var template = _repository.GetTemplateById(templateId);

            if (template == null) return false;



            // Delete the physical file

            if (File.Exists(template.FilePath))

            {

                File.Delete(template.FilePath);

            }



            return _repository.DeleteTemplate(templateId, deletedByUserId);

        }



        #endregion



        #region PDF Form Fields Detection



        public List<PdfFormFieldModel> DetectFormFields(string pdfPath)

        {

            var fields = new List<PdfFormFieldModel>();



            try

            {

                using var stream = new FileStream(pdfPath, FileMode.Open, FileAccess.Read, FileShare.Read);

                using var reader = new PdfReader(stream);

                using var pdfDoc = new PdfDocument(reader);

                var form = PdfAcroForm.GetAcroForm(pdfDoc, false);



                if (form != null)

                {

                    var formFields = form.GetAllFormFields();

                    foreach (var kvp in formFields)

                    {

                        try

                        {

                            var field = kvp.Value;

                            var pdfField = new PdfFormFieldModel

                            {

                                Name = kvp.Key,

                                Type = GetFieldType(field),

                                MappedClientField = GetAutoMappedField(kvp.Key)

                            };



                            // Get options for choice fields

                            if (field is PdfChoiceFormField choiceField)

                            {

                                var options = choiceField.GetOptions();

                                if (options != null)

                                {

                                    pdfField.Options = new List<string>();

                                    foreach (var option in options)

                                    {

                                        pdfField.Options.Add(option?.ToString() ?? string.Empty);

                                    }

                                }

                            }



                            // Get default value if available

                            var value = field.GetValueAsString();

                            if (!string.IsNullOrEmpty(value))

                            {

                                pdfField.DefaultValue = value;

                            }



                            fields.Add(pdfField);

                        }

                        catch (Exception fieldEx)

                        {

                            System.Diagnostics.Debug.WriteLine($"Error reading field {kvp.Key}: {fieldEx.Message}");

                        }

                    }

                }

            }

            catch (Exception ex)

            {

                System.Diagnostics.Debug.WriteLine($"Error detecting form fields: {ex.Message}");

                // Return empty list - PDF might not have form fields or is invalid

            }



            return fields;

        }



        private string GetFieldType(iText.Forms.Fields.PdfFormField field)

        {

            return field switch

            {

                PdfTextFormField => "text",

                PdfButtonFormField btn => IsCheckBox(btn) ? "checkbox" : "radio",

                PdfChoiceFormField choice => IsComboBox(choice) ? "dropdown" : "listbox",

                PdfSignatureFormField => "signature",

                _ => "unknown"

            };

        }



        private bool IsCheckBox(PdfButtonFormField field)

        {

            var flags = field.GetFieldFlags();

            // Check if it's NOT a radio button (radio buttons have flag 32768)

            return (flags & 32768) == 0;

        }



        private bool IsComboBox(PdfChoiceFormField field)

        {

            var flags = field.GetFieldFlags();

            // Combo box has flag 131072

            return (flags & 131072) != 0;

        }



        private string? GetAutoMappedField(string fieldName)

        {

            // Try to auto-map based on field name

            string normalizedName = fieldName.Replace("_", "").Replace("-", "").Replace(" ", "");



            if (ClientFieldMappings.StandardFields.TryGetValue(normalizedName, out string? mappedField))

            {

                return mappedField;

            }



            // Try case-insensitive match

            foreach (var kvp in ClientFieldMappings.StandardFields)

            {

                if (kvp.Key.Equals(normalizedName, StringComparison.OrdinalIgnoreCase))

                {

                    return kvp.Value;

                }

            }



            return null;

        }



        #endregion



        #region Document Generation



        public byte[] GenerateDocument(int templateId, Client client, Dictionary<string, string>? additionalData = null)

        {

            var template = _repository.GetTemplateById(templateId);

            if (template == null)

                throw new ArgumentException("Template not found.", nameof(templateId));



            if (!File.Exists(template.FilePath))

                throw new FileNotFoundException("Template PDF file not found.", template.FilePath);
            try

            {

                return GenerateDocumentFromTemplate(template.FilePath, client, template.FieldMappings, additionalData);

            }

            catch (Exception ex)

            {

                System.Diagnostics.Debug.WriteLine($"Error generating document: {ex.Message}");

                // If iText fails, just return a copy of the original template

                return File.ReadAllBytes(template.FilePath);

            }

        }



        public byte[] GenerateDocumentFromTemplate(string templatePath, Client client,

            Dictionary<string, string> fieldMappings, Dictionary<string, string>? additionalData = null)

        {

            using var outputStream = new MemoryStream();

            using var inputStream = new FileStream(templatePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            using var reader = new PdfReader(inputStream);

            using var writer = new PdfWriter(outputStream);

            using var pdfDoc = new PdfDocument(reader, writer);



            try

            {

                var form = PdfAcroForm.GetAcroForm(pdfDoc, false);

                if (form != null)

                {

                    var formFields = form.GetAllFormFields();

                    foreach (var kvp in formFields)

                    {

                        try

                        {

                            string? valueToFill = GetFieldValue(kvp.Key, client, fieldMappings, additionalData);



                            if (valueToFill != null)

                            {

                                FillField(kvp.Value, valueToFill);

                            }

                        }

                        catch (Exception fieldEx)

                        {

                            System.Diagnostics.Debug.WriteLine($"Error filling field {kvp.Key}: {fieldEx.Message}");

                        }

                    }



                    // Flatten form to make fields non-editable (optional)

                    // form.FlattenFields();

                }

            }

            catch (Exception ex)

            {

                System.Diagnostics.Debug.WriteLine($"Error processing form: {ex.Message}");

            }



            pdfDoc.Close();

            return outputStream.ToArray();

        }



        private string? GetFieldValue(string fieldName, Client client,

            Dictionary<string, string> fieldMappings, Dictionary<string, string>? additionalData)

        {

            // First check if there's a specific mapping for this field

            if (fieldMappings.TryGetValue(fieldName, out string? mappedField))

            {

                return GetClientFieldValue(mappedField, client);

            }



            // Try auto-mapping

            string? autoMapped = GetAutoMappedField(fieldName);

            if (autoMapped != null)

            {

                return GetClientFieldValue(autoMapped, client);

            }



            // Check additional data

            if (additionalData != null && additionalData.TryGetValue(fieldName, out string? additionalValue))

            {

                return additionalValue;

            }



            // Check if field name matches a client extra data field

            string? extraDataValue = client.GetExtraDataValue(fieldName);

            if (extraDataValue != null)

            {

                return extraDataValue;

            }



            return null;

        }



        private string? GetClientFieldValue(string fieldName, Client client)

        {

            return fieldName switch

            {

                "Name" => client.Name,

                "SSN" => client.SSN,

                "DOB" => client.DOB?.ToString("MM/dd/yyyy"),

                "Phone" => client.Phone,

                "Email" => client.Email,

                "CurrentDate" => DateTime.Now.ToString("MM/dd/yyyy"),

                _ => client.GetExtraDataValue(fieldName)

            };

        }



        private void FillField(iText.Forms.Fields.PdfFormField field, string value)

        {

            try

            {

                switch (field)

                {

                    case PdfTextFormField textField:

                        textField.SetValue(value);

                        break;



                    case PdfButtonFormField buttonField:

                        if (IsCheckBox(buttonField))

                        {

                            bool isChecked = value.Equals("true", StringComparison.OrdinalIgnoreCase) ||

                                            value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||

                                            value == "1";

                            buttonField.SetValue(isChecked ? "Yes" : "Off");

                        }

                        break;



                    case PdfChoiceFormField choiceField:

                        choiceField.SetValue(value);

                        break;



                    default:

                        field.SetValue(value);

                        break;

                }

            }

            catch (Exception ex)

            {

                System.Diagnostics.Debug.WriteLine($"Error filling field {field.GetFieldName()}: {ex.Message}");

            }

        }



        #endregion



        #region PDF Creation (New Templates)



        public string CreateNewTemplate(string name, string? description, string? category, int createdByUserId)

        {

            // Generate unique file name

            string fileName = $"{Guid.NewGuid()}_template.pdf";

            string filePath = System.IO.Path.Combine(TEMPLATES_FOLDER, fileName);



            // Create a blank PDF with form capability

            using var writer = new PdfWriter(filePath);

            using var pdfDoc = new PdfDocument(writer);



            // Add a blank page (A4 size)

            pdfDoc.AddNewPage(iText.Kernel.Geom.PageSize.A4);



            // Initialize AcroForm

            PdfAcroForm.GetAcroForm(pdfDoc, true);



            pdfDoc.Close();



            var fileInfo = new FileInfo(filePath);



            var template = new PdfTemplateModel

            {

                Name = name,

                Description = description ?? string.Empty,

                Category = category ?? string.Empty,

                FileName = $"{name}.pdf",

                FilePath = filePath,

                FileSize = fileInfo.Length,

                FieldMappings = "{}",

                IsActive = true

            };



            _repository.CreateTemplate(template, createdByUserId);



            return filePath;

        }



        public void AddTextField(string pdfPath, string fieldName, float x, float y, float width, float height, int pageIndex = 0)

        {

            string tempPath = pdfPath + ".tmp";



            using (var reader = new PdfReader(pdfPath))

            using (var writer = new PdfWriter(tempPath))

            using (var pdfDoc = new PdfDocument(reader, writer))

            {

                var form = PdfAcroForm.GetAcroForm(pdfDoc, true);

                var page = pdfDoc.GetPage(pageIndex + 1);



                var rect = new iTextRectangle(x, y, width, height);

                var textField = new TextFormFieldBuilder(pdfDoc, fieldName)

                    .SetWidgetRectangle(rect)

                    .SetPage(page)

                    .CreateText();



                form.AddField(textField);

            }



            File.Delete(pdfPath);

            File.Move(tempPath, pdfPath);

        }



        public void AddCheckBoxField(string pdfPath, string fieldName, float x, float y, float size, int pageIndex = 0)

        {

            string tempPath = pdfPath + ".tmp";



            using (var reader = new PdfReader(pdfPath))

            using (var writer = new PdfWriter(tempPath))

            using (var pdfDoc = new PdfDocument(reader, writer))

            {

                var form = PdfAcroForm.GetAcroForm(pdfDoc, true);

                var page = pdfDoc.GetPage(pageIndex + 1);



                var rect = new iTextRectangle(x, y, size, size);

                var checkBox = new CheckBoxFormFieldBuilder(pdfDoc, fieldName)

                    .SetWidgetRectangle(rect)

                    .SetPage(page)

                    .CreateCheckBox();



                form.AddField(checkBox);

            }



            File.Delete(pdfPath);

            File.Move(tempPath, pdfPath);

        }



        #endregion



        #region Save Generated Document



        public string SaveGeneratedDocument(byte[] pdfData, int clientId, string fileName, int savedByUserId)

        {

            // Create client folder if it doesn't exist (with client name)

            string clientFolder = FilePathService.GetOrMigrateClientFolderPathById(clientId);

            if (!Directory.Exists(clientFolder))

            {

                Directory.CreateDirectory(clientFolder);

            }



            // Generate unique file name

            string uniqueFileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{fileName}";

            string filePath = System.IO.Path.Combine(clientFolder, uniqueFileName);



            // Save the PDF to disk

            File.WriteAllBytes(filePath, pdfData);



            // Save record to database

            using var connection = DbConnection.GetConnection();

            connection.Open();



            string sql = @"

                INSERT INTO ClientFiles (ClientId, FileName, FilePath, FileSize, Description, UploadedBy, UploadedAt)

                VALUES (@clientId, @fileName, @filePath, @fileSize, @description, @uploadedBy, @uploadedAt)";



            using var cmd = new Microsoft.Data.Sqlite.SqliteCommand(sql, connection);

            cmd.Parameters.AddWithValue("@clientId", clientId);

            cmd.Parameters.AddWithValue("@fileName", fileName);

            cmd.Parameters.AddWithValue("@filePath", filePath);

            cmd.Parameters.AddWithValue("@fileSize", pdfData.Length);

            cmd.Parameters.AddWithValue("@description", "Generated from PDF template");

            cmd.Parameters.AddWithValue("@uploadedBy", savedByUserId);

            cmd.Parameters.AddWithValue("@uploadedAt", DateTime.Now);



            cmd.ExecuteNonQuery();



            return filePath;

        }



        #endregion

    }

}