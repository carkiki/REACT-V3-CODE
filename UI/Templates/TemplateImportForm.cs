using System;

using System.Collections.Generic;

using System.Drawing;

using System.IO;

using System.Windows.Forms;

using ReactCRM.Models;

using ReactCRM.Services;



namespace ReactCRM.UI.Templates

{

    public partial class TemplateImportForm : Form

    {

        private string _sourcePath;

        private TextBox txtName;

        private TextBox txtDescription;

        private ComboBox cmbCategory;

        private DataGridView gridFields;

        private Button btnImport;

        private Button btnCancel;

        private Label lblFileInfo;

        private List<PdfFormField> _detectedFields;



        public TemplateImportForm(string sourcePath)

        {

            _sourcePath = sourcePath;

            _detectedFields = new List<PdfFormField>();

            InitializeComponent();

            LoadFileInfo();

            DetectFields();

        }



        private void InitializeComponent()

        {

            this.Text = "Import PDF Template";

            this.Size = new Size(700, 600);

            this.StartPosition = FormStartPosition.CenterParent;

            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            this.MaximizeBox = false;

            this.MinimizeBox = false;



            // Header Panel

            var panelHeader = new Panel

            {

                Dock = DockStyle.Top,

                Height = 60,

                BackColor = Color.FromArgb(41, 128, 185),

                Padding = new Padding(15)

            };



            var lblTitle = new Label

            {

                Text = "Import PDF Template",

                Font = new Font("Segoe UI", 12, FontStyle.Bold),

                ForeColor = Color.White,

                Location = new Point(15, 18),

                AutoSize = true

            };



            panelHeader.Controls.Add(lblTitle);



            // Main content panel

            var panelContent = new Panel

            {

                Dock = DockStyle.Fill,

                Padding = new Padding(20),

                AutoScroll = true

            };



            int yPos = 10;



            // File info

            lblFileInfo = new Label

            {

                Location = new Point(10, yPos),

                AutoSize = true,

                Font = new Font("Segoe UI", 9, FontStyle.Italic)

            };

            panelContent.Controls.Add(lblFileInfo);

            yPos += 30;



            // Name field

            var lblName = new Label

            {

                Text = "Template Name *",

                Location = new Point(10, yPos),

                AutoSize = true,

                Font = new Font("Segoe UI", 9, FontStyle.Bold)

            };

            panelContent.Controls.Add(lblName);

            yPos += 25;



            txtName = new TextBox

            {

                Location = new Point(10, yPos),

                Size = new Size(620, 25),

                Font = new Font("Segoe UI", 10)

            };

            panelContent.Controls.Add(txtName);

            yPos += 40;



            // Description field

            var lblDescription = new Label

            {

                Text = "Description",

                Location = new Point(10, yPos),

                AutoSize = true,

                Font = new Font("Segoe UI", 9, FontStyle.Bold)

            };

            panelContent.Controls.Add(lblDescription);

            yPos += 25;



            txtDescription = new TextBox

            {

                Location = new Point(10, yPos),

                Size = new Size(620, 60),

                Font = new Font("Segoe UI", 10),

                Multiline = true

            };

            panelContent.Controls.Add(txtDescription);

            yPos += 75;



            // Category field

            var lblCategory = new Label

            {

                Text = "Category",

                Location = new Point(10, yPos),

                AutoSize = true,

                Font = new Font("Segoe UI", 9, FontStyle.Bold)

            };

            panelContent.Controls.Add(lblCategory);

            yPos += 25;



            cmbCategory = new ComboBox

            {

                Location = new Point(10, yPos),

                Size = new Size(300, 25),

                Font = new Font("Segoe UI", 10),

                DropDownStyle = ComboBoxStyle.DropDown

            };

            // Add default categories

            cmbCategory.Items.AddRange(new object[] {

                "Contracts",

                "Intake Forms",

                "Receipts",

                "Reports",

                "Letters",

                "Agreements",

                "Other"

            });

            panelContent.Controls.Add(cmbCategory);

            yPos += 45;



            // Detected fields label

            var lblFields = new Label

            {

                Text = "Detected Form Fields (auto-mapped to client data)",

                Location = new Point(10, yPos),

                AutoSize = true,

                Font = new Font("Segoe UI", 9, FontStyle.Bold)

            };

            panelContent.Controls.Add(lblFields);

            yPos += 25;



            // Fields grid

            gridFields = new DataGridView

            {

                Location = new Point(10, yPos),

                Size = new Size(620, 180),

                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,

                SelectionMode = DataGridViewSelectionMode.FullRowSelect,

                MultiSelect = false,

                AllowUserToAddRows = false,

                AllowUserToDeleteRows = false,

                BackgroundColor = Color.White,

                BorderStyle = BorderStyle.FixedSingle,

                RowHeadersVisible = false,

                Font = new Font("Segoe UI", 9)

            };

            panelContent.Controls.Add(gridFields);



            // Bottom Panel

            var panelBottom = new Panel

            {

                Dock = DockStyle.Bottom,

                Height = 60,

                BackColor = Color.FromArgb(236, 240, 241),

                Padding = new Padding(10)

            };



            btnImport = new Button

            {

                Text = "Import Template",

                Location = new Point(450, 12),

                Size = new Size(120, 35),

                BackColor = Color.FromArgb(46, 204, 113),

                ForeColor = Color.White,

                FlatStyle = FlatStyle.Flat,

                Font = new Font("Segoe UI", 9),

                Cursor = Cursors.Hand

            };

            btnImport.Click += BtnImport_Click;



            btnCancel = new Button

            {

                Text = "Cancel",

                Location = new Point(580, 12),

                Size = new Size(80, 35),

                BackColor = Color.FromArgb(127, 140, 141),

                ForeColor = Color.White,

                FlatStyle = FlatStyle.Flat,

                Font = new Font("Segoe UI", 9),

                Cursor = Cursors.Hand

            };

            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };



            panelBottom.Controls.Add(btnImport);

            panelBottom.Controls.Add(btnCancel);



            // Add controls to form

            this.Controls.Add(panelContent);

            this.Controls.Add(panelHeader);

            this.Controls.Add(panelBottom);

        }



        private void LoadFileInfo()

        {

            var fileInfo = new FileInfo(_sourcePath);

            string sizeStr = FormatFileSize(fileInfo.Length);

            lblFileInfo.Text = $"File: {fileInfo.Name} ({sizeStr})";

            txtName.Text = Path.GetFileNameWithoutExtension(_sourcePath);

        }



        private void DetectFields()

        {

            try

            {

                _detectedFields = PdfTemplateService.Instance.DetectFormFields(_sourcePath);

                DisplayFields();

            }

            catch (Exception ex)

            {

                MessageBox.Show($"Error detecting form fields: {ex.Message}", "Warning",

                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }

        }



        private void DisplayFields()

        {

            gridFields.Columns.Clear();

            gridFields.Columns.Add("FieldName", "Field Name");

            gridFields.Columns.Add("FieldType", "Type");

            gridFields.Columns.Add("MappedTo", "Auto-Mapped To");



            foreach (var field in _detectedFields)

            {

                gridFields.Rows.Add(

                    field.Name,

                    field.Type,

                    field.MappedClientField ?? "(not mapped)"

                );

            }



            if (_detectedFields.Count == 0)

            {

                gridFields.Rows.Add("(No form fields detected)", "", "");

            }

        }



        private string FormatFileSize(long bytes)

        {

            string[] sizes = { "B", "KB", "MB", "GB" };

            int order = 0;

            double size = bytes;

            while (size >= 1024 && order < sizes.Length - 1)

            {

                order++;

                size /= 1024;

            }

            return $"{size:0.##} {sizes[order]}";

        }



        private void BtnImport_Click(object? sender, EventArgs e)

        {

            if (string.IsNullOrWhiteSpace(txtName.Text))

            {

                MessageBox.Show("Please enter a template name.", "Validation Error",

                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                txtName.Focus();

                return;

            }



            try

            {

                int userId = AuthService.Instance.GetCurrentUserId();

                string? category = cmbCategory.SelectedItem?.ToString() ?? cmbCategory.Text;

                if (string.IsNullOrWhiteSpace(category)) category = null;



                PdfTemplateService.Instance.ImportTemplate(

                    _sourcePath,

                    txtName.Text.Trim(),

                    string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim(),

                    category,

                    userId

                );



                this.DialogResult = DialogResult.OK;

                this.Close();

            }

            catch (Exception ex)

            {

                MessageBox.Show($"Error importing template: {ex.Message}", "Error",

                    MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }

    }

}