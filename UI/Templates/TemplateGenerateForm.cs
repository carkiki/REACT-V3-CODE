using System;

using System.Collections.Generic;

using System.Drawing;

using System.IO;

using System.Windows.Forms;

using Microsoft.Web.WebView2.WinForms;

using ReactCRM.Database;

using ReactCRM.Models;

using ReactCRM.Services;



namespace ReactCRM.UI.Templates

{

    public class TemplateGenerateForm : Form

    {

        private readonly Client _client;

        private CrmPdfTemplate? _selectedTemplate;

        private ComboBox cmbTemplates;

        private Label lblTemplateName;

        private Label lblFieldsDetected;

        private DataGridView dgvFields;

        private Panel panelPreview;

        private WebView2 webViewPreview;

        private Button btnPreview;

        private Button btnGenerate;

        private Button btnClose;

        private SplitContainer splitContainer;

        private string? _tempPreviewPath;
        public event EventHandler? DocumentGenerated;



        public TemplateGenerateForm(Client client)

        {

            _client = client;

            InitializeComponent();

            LoadTemplates();

        }



        private void InitializeComponent()

        {

            this.Text = $"Generate Document for {_client.Name}";

            this.Size = new Size(1200, 800);

            this.StartPosition = FormStartPosition.CenterParent;

            this.BackColor = UITheme.Colors.BackgroundPrimary;



            // Header

            var lblTitle = new Label

            {

                Text = "Generate Document from Template",

                Font = UITheme.Fonts.HeaderMedium,

                ForeColor = UITheme.Colors.TextPrimary,

                Location = new Point(20, 15),

                AutoSize = true

            };

            this.Controls.Add(lblTitle);



            // Template selection

            var lblSelectTemplate = new Label

            {

                Text = "Select Template:",

                Location = new Point(20, 55),

                AutoSize = true

            };

            this.Controls.Add(lblSelectTemplate);



            cmbTemplates = new ComboBox

            {

                Location = new Point(130, 52),

                Size = new Size(300, 25),

                DropDownStyle = ComboBoxStyle.DropDownList

            };

            cmbTemplates.SelectedIndexChanged += CmbTemplates_SelectedIndexChanged;

            this.Controls.Add(cmbTemplates);



            lblTemplateName = new Label

            {

                Text = "",

                Location = new Point(450, 55),

                AutoSize = true,

                ForeColor = UITheme.Colors.TextSecondary

            };

            this.Controls.Add(lblTemplateName);



            // Split container for fields and preview

            splitContainer = new SplitContainer

            {

                Location = new Point(20, 90),

                Size = new Size(1140, 600),

                Orientation = Orientation.Vertical,

                SplitterDistance = 400,

                Panel1MinSize = 300,

                Panel2MinSize = 400

            };



            // Left panel - Fields

            var panelFields = new Panel

            {

                Dock = DockStyle.Fill,

                BackColor = UITheme.Colors.SurfaceWhite,

                Padding = new Padding(10)

            };



            lblFieldsDetected = new Label

            {

                Text = "Detected Form Fields:",

                Font = UITheme.Fonts.BodyRegular,

                Location = new Point(10, 10),

                AutoSize = true

            };

            panelFields.Controls.Add(lblFieldsDetected);



            dgvFields = new DataGridView

            {

                Location = new Point(10, 35),

                Size = new Size(370, 500),

                AllowUserToAddRows = false,

                AllowUserToDeleteRows = false,

                SelectionMode = DataGridViewSelectionMode.FullRowSelect,

                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,

                BackgroundColor = Color.White

            };

            dgvFields.Columns.Add("FieldName", "PDF Field");

            dgvFields.Columns.Add("MappedTo", "Mapped To");

            dgvFields.Columns.Add("Value", "Value");

            dgvFields.Columns["FieldName"].ReadOnly = true;

            dgvFields.Columns["MappedTo"].ReadOnly = true;

            panelFields.Controls.Add(dgvFields);



            splitContainer.Panel1.Controls.Add(panelFields);



            // Right panel - Preview

            panelPreview = new Panel

            {

                Dock = DockStyle.Fill,

                BackColor = UITheme.Colors.BackgroundSecondary,

                Padding = new Padding(10)

            };



            var lblPreview = new Label

            {

                Text = "PDF Preview:",

                Font = UITheme.Fonts.BodyRegular,

                Location = new Point(10, 10),

                AutoSize = true

            };

            panelPreview.Controls.Add(lblPreview);



            webViewPreview = new WebView2

            {

                Location = new Point(10, 35),

                Size = new Size(700, 550),

                Visible = true

            };

            panelPreview.Controls.Add(webViewPreview);



            splitContainer.Panel2.Controls.Add(panelPreview);

            this.Controls.Add(splitContainer);



            // Buttons

            btnPreview = new Button

            {

                Text = "Preview PDF",

                Location = new Point(20, 710),

                Size = new Size(120, 35)

            };

            UITheme.ApplyButtonStyle(btnPreview, ButtonType.Secondary);

            btnPreview.Click += BtnPreview_Click;

            this.Controls.Add(btnPreview);



            btnGenerate = new Button

            {

                Text = "Save to Profile",

                Location = new Point(150, 710),

                Size = new Size(140, 35)

            };

            UITheme.ApplyButtonStyle(btnGenerate, ButtonType.Primary);

            btnGenerate.Click += BtnGenerate_Click;

            this.Controls.Add(btnGenerate);



            btnClose = new Button

            {

                Text = "Close",

                Location = new Point(1040, 710),

                Size = new Size(100, 35)

            };

            UITheme.ApplyButtonStyle(btnClose, ButtonType.Secondary);

            btnClose.Click += (s, e) => this.Close();

            this.Controls.Add(btnClose);



            this.Resize += TemplateGenerateForm_Resize;

        }



        private void TemplateGenerateForm_Resize(object? sender, EventArgs e)

        {

            splitContainer.Size = new Size(this.ClientSize.Width - 40, this.ClientSize.Height - 200);

            webViewPreview.Size = new Size(splitContainer.Panel2.Width - 30, splitContainer.Panel2.Height - 50);

            dgvFields.Size = new Size(splitContainer.Panel1.Width - 30, splitContainer.Panel1.Height - 50);

        }



        private void LoadTemplates()

        {

            cmbTemplates.Items.Clear();

            var templates = PdfTemplateService.Instance.GetActiveTemplates();

            foreach (var template in templates)

            {

                cmbTemplates.Items.Add(new TemplateComboItem(template));

            }

        }



        private void CmbTemplates_SelectedIndexChanged(object? sender, EventArgs e)

        {

            if (cmbTemplates.SelectedItem is TemplateComboItem item)

            {

                _selectedTemplate = PdfTemplateService.Instance.GetTemplateById(item.Id);

                if (_selectedTemplate != null)

                {

                    lblTemplateName.Text = $"Category: {_selectedTemplate.Category}";

                    LoadFieldMappings();

                }

            }

        }



        private void LoadFieldMappings()

        {

            dgvFields.Rows.Clear();

            if (_selectedTemplate == null) return;



            // Detect fields from the PDF

            var fields = PdfTemplateService.Instance.DetectFormFields(_selectedTemplate.FilePath);

            lblFieldsDetected.Text = $"Detected Form Fields: {fields.Count}";



            foreach (var field in fields)

            {

                string mappedTo = field.MappedClientField ?? "(not mapped)";

                string value = GetClientValue(field.MappedClientField);

                dgvFields.Rows.Add(field.Name, mappedTo, value);

            }

        }



        private string GetClientValue(string? fieldName)

        {

            if (string.IsNullOrEmpty(fieldName)) return "";



            return fieldName switch

            {

                "Name" => _client.Name ?? "",

                "SSN" => _client.SSN ?? "",

                "DOB" => _client.DOB?.ToString("MM/dd/yyyy") ?? "",

                "Phone" => _client.Phone ?? "",

                "Email" => _client.Email ?? "",

                "CurrentDate" => DateTime.Now.ToString("MM/dd/yyyy"),

                _ => _client.GetExtraDataValue(fieldName) ?? ""

            };

        }



        private async void BtnPreview_Click(object? sender, EventArgs e)

        {

            if (_selectedTemplate == null)

            {

                MessageBox.Show("Please select a template first.", "Warning",

                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }



            try

            {

                // Generate preview PDF

                byte[] pdfData = PdfTemplateService.Instance.GenerateDocument(_selectedTemplate.Id, _client);



                // Save to temp file

                _tempPreviewPath = Path.Combine(Path.GetTempPath(), $"preview_{Guid.NewGuid()}.pdf");

                File.WriteAllBytes(_tempPreviewPath, pdfData);



                // Show in WebView2

                await webViewPreview.EnsureCoreWebView2Async(null);

                string fileUri = new Uri(_tempPreviewPath).AbsoluteUri;

                webViewPreview.CoreWebView2.Navigate(fileUri);

            }

            catch (Exception ex)

            {

                MessageBox.Show($"Error generating preview: {ex.Message}", "Error",

                    MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }



        private void BtnGenerate_Click(object? sender, EventArgs e)

        {

            if (_selectedTemplate == null)

            {

                MessageBox.Show("Please select a template first.", "Warning",

                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }



            try

            {

                // Generate PDF

                byte[] pdfData = PdfTemplateService.Instance.GenerateDocument(_selectedTemplate.Id, _client);



                // Generate filename

                string fileName = $"{_client.Name}_{_selectedTemplate.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";



                // Save to client files

                int userId = AuthService.Instance.CurrentUserId;

                PdfTemplateService.Instance.SaveGeneratedDocument(pdfData, _client.Id, fileName, userId);



                MessageBox.Show($"Document saved to client profile!\n\nFile: {fileName}", "Success",

                    MessageBoxButtons.OK, MessageBoxIcon.Information);



                DocumentGenerated?.Invoke(this, EventArgs.Empty);

            }

            catch (Exception ex)

            {

                MessageBox.Show($"Error generating document: {ex.Message}", "Error",

                    MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }



        protected override void Dispose(bool disposing)

        {

            if (disposing)

            {

                webViewPreview?.Dispose();

                // Clean up temp file

                if (!string.IsNullOrEmpty(_tempPreviewPath) && File.Exists(_tempPreviewPath))

                {

                    try { File.Delete(_tempPreviewPath); } catch { }

                }

            }

            base.Dispose(disposing);

        }



        private class TemplateComboItem

        {

            public int Id { get; }

            public string Name { get; }



            public TemplateComboItem(PdfTemplate template)

            {

                Id = template.Id;

                Name = template.Name;

            }



            public override string ToString() => Name;

        }

    }

}