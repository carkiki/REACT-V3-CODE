using System;

using System.Collections.Generic;

using System.Drawing;

using System.Windows.Forms;

using ReactCRM.Models;

using ReactCRM.Services;

using Newtonsoft.Json;



namespace ReactCRM.UI.Templates

{

    public partial class TemplateEditForm : Form

    {

        private PdfTemplate _template;

        private TextBox txtName;

        private TextBox txtDescription;

        private ComboBox cmbCategory;

        private CheckBox chkActive;

        private DataGridView gridMappings;

        private Button btnSave;

        private Button btnCancel;

        private Button btnRefreshFields;

        private List<PdfFormField> _detectedFields;



        public TemplateEditForm(PdfTemplate template)

        {

            _template = template;

            _detectedFields = new List<PdfFormField>();

            InitializeComponent();

            LoadTemplateData();

            LoadFields();

        }



        private void InitializeComponent()

        {

            this.Text = "Edit PDF Template";

            this.Size = new Size(750, 650);

            this.StartPosition = FormStartPosition.CenterParent;

            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            this.MaximizeBox = false;

            this.MinimizeBox = false;



            // Header Panel

            var panelHeader = new Panel

            {

                Dock = DockStyle.Top,

                Height = 60,

                BackColor = Color.FromArgb(241, 196, 15),

                Padding = new Padding(15)

            };



            var lblTitle = new Label

            {

                Text = "Edit Template Settings & Field Mappings",

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

                Size = new Size(680, 25),

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

                Size = new Size(680, 60),

                Font = new Font("Segoe UI", 10),

                Multiline = true

            };

            panelContent.Controls.Add(txtDescription);

            yPos += 75;



            // Category and Active status on same row

            var lblCategory = new Label

            {

                Text = "Category",

                Location = new Point(10, yPos),

                AutoSize = true,

                Font = new Font("Segoe UI", 9, FontStyle.Bold)

            };

            panelContent.Controls.Add(lblCategory);



            cmbCategory = new ComboBox

            {

                Location = new Point(10, yPos + 25),

                Size = new Size(300, 25),

                Font = new Font("Segoe UI", 10),

                DropDownStyle = ComboBoxStyle.DropDown

            };

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



            chkActive = new CheckBox

            {

                Text = "Template is Active",

                Location = new Point(350, yPos + 25),

                AutoSize = true,

                Font = new Font("Segoe UI", 9)

            };

            panelContent.Controls.Add(chkActive);

            yPos += 75;



            // Field mappings section

            var lblMappings = new Label

            {

                Text = "Field Mappings (map PDF fields to client data)",

                Location = new Point(10, yPos),

                AutoSize = true,

                Font = new Font("Segoe UI", 9, FontStyle.Bold)

            };

            panelContent.Controls.Add(lblMappings);



            btnRefreshFields = new Button

            {

                Text = "Refresh Fields",

                Location = new Point(580, yPos - 5),

                Size = new Size(110, 28),

                BackColor = Color.FromArgb(52, 152, 219),

                ForeColor = Color.White,

                FlatStyle = FlatStyle.Flat,

                Font = new Font("Segoe UI", 8),

                Cursor = Cursors.Hand

            };

            btnRefreshFields.Click += BtnRefreshFields_Click;

            panelContent.Controls.Add(btnRefreshFields);

            yPos += 30;



            // Mappings grid

            gridMappings = new DataGridView

            {

                Location = new Point(10, yPos),

                Size = new Size(680, 220),

                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,

                SelectionMode = DataGridViewSelectionMode.CellSelect,

                MultiSelect = false,

                AllowUserToAddRows = false,

                AllowUserToDeleteRows = false,

                BackgroundColor = Color.White,

                BorderStyle = BorderStyle.FixedSingle,

                RowHeadersVisible = false,

                Font = new Font("Segoe UI", 9),

                EditMode = DataGridViewEditMode.EditOnEnter

            };

            panelContent.Controls.Add(gridMappings);



            // Bottom Panel

            var panelBottom = new Panel

            {

                Dock = DockStyle.Bottom,

                Height = 60,

                BackColor = Color.FromArgb(236, 240, 241),

                Padding = new Padding(10)

            };



            btnSave = new Button

            {

                Text = "Save Changes",

                Location = new Point(490, 12),

                Size = new Size(120, 35),

                BackColor = Color.FromArgb(46, 204, 113),

                ForeColor = Color.White,

                FlatStyle = FlatStyle.Flat,

                Font = new Font("Segoe UI", 9),

                Cursor = Cursors.Hand

            };

            btnSave.Click += BtnSave_Click;



            btnCancel = new Button

            {

                Text = "Cancel",

                Location = new Point(620, 12),

                Size = new Size(80, 35),

                BackColor = Color.FromArgb(127, 140, 141),

                ForeColor = Color.White,

                FlatStyle = FlatStyle.Flat,

                Font = new Font("Segoe UI", 9),

                Cursor = Cursors.Hand

            };

            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };



            panelBottom.Controls.Add(btnSave);

            panelBottom.Controls.Add(btnCancel);



            // Add controls to form

            this.Controls.Add(panelContent);

            this.Controls.Add(panelHeader);

            this.Controls.Add(panelBottom);

        }



        private void LoadTemplateData()

        {

            txtName.Text = _template.Name;

            txtDescription.Text = _template.Description ?? "";

            chkActive.Checked = _template.IsActive;



            if (!string.IsNullOrEmpty(_template.Category))

            {

                int index = cmbCategory.Items.IndexOf(_template.Category);

                if (index >= 0)

                    cmbCategory.SelectedIndex = index;

                else

                    cmbCategory.Text = _template.Category;

            }

        }



        private void LoadFields()

        {

            try

            {

                _detectedFields = PdfTemplateService.Instance.DetectFormFields(_template.FilePath);

                DisplayMappings();

            }

            catch (Exception ex)

            {

                MessageBox.Show($"Error loading fields: {ex.Message}", "Warning",

                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }

        }



        private void DisplayMappings()

        {

            gridMappings.Columns.Clear();



            // Field name column (read-only)

            var colFieldName = new DataGridViewTextBoxColumn

            {

                Name = "FieldName",

                HeaderText = "PDF Field",

                ReadOnly = true

            };

            gridMappings.Columns.Add(colFieldName);



            // Field type column (read-only)

            var colFieldType = new DataGridViewTextBoxColumn

            {

                Name = "FieldType",

                HeaderText = "Type",

                ReadOnly = true,

                Width = 80

            };

            gridMappings.Columns.Add(colFieldType);



            // Mapping dropdown column

            var colMapping = new DataGridViewComboBoxColumn

            {

                Name = "MappedTo",

                HeaderText = "Map to Client Field",

                DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton

            };

            colMapping.Items.Add("(none)");

            colMapping.Items.AddRange(ClientFieldMappings.GetAvailableFields().ToArray());

            gridMappings.Columns.Add(colMapping);



            // Get existing mappings

            var existingMappings = _template.GetFieldMappings();



            // Add rows

            foreach (var field in _detectedFields)

            {

                int rowIndex = gridMappings.Rows.Add(field.Name, field.Type);



                // Set mapped value

                string mappedValue = "(none)";

                if (existingMappings.TryGetValue(field.Name, out string? mapped) && !string.IsNullOrEmpty(mapped))

                {

                    mappedValue = mapped;

                }

                else if (!string.IsNullOrEmpty(field.MappedClientField))

                {

                    mappedValue = field.MappedClientField;

                }



                var cell = gridMappings.Rows[rowIndex].Cells["MappedTo"] as DataGridViewComboBoxCell;

                if (cell != null)

                {

                    if (cell.Items.Contains(mappedValue))

                        cell.Value = mappedValue;

                    else

                        cell.Value = "(none)";

                }

            }



            if (_detectedFields.Count == 0)

            {

                gridMappings.Rows.Add("(No form fields detected)", "", "(none)");

            }

        }



        private void BtnRefreshFields_Click(object? sender, EventArgs e)

        {

            LoadFields();

            MessageBox.Show("Fields refreshed from PDF.", "Info",

                MessageBoxButtons.OK, MessageBoxIcon.Information);

        }



        private void BtnSave_Click(object? sender, EventArgs e)

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

                // Collect field mappings

                var mappings = new Dictionary<string, string>();

                foreach (DataGridViewRow row in gridMappings.Rows)

                {

                    string? fieldName = row.Cells["FieldName"].Value?.ToString();

                    string? mappedTo = row.Cells["MappedTo"].Value?.ToString();



                    if (!string.IsNullOrEmpty(fieldName) && !string.IsNullOrEmpty(mappedTo) && mappedTo != "(none)")

                    {

                        mappings[fieldName] = mappedTo;

                    }

                }



                // Update template

                _template.Name = txtName.Text.Trim();

                _template.Description = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim();

                _template.Category = string.IsNullOrWhiteSpace(cmbCategory.Text) ? null : cmbCategory.Text;

                _template.IsActive = chkActive.Checked;

                _template.SetFieldMappings(mappings);



                int userId = AuthService.Instance.GetCurrentUserId();



                if (PdfTemplateService.Instance.UpdateTemplate(_template, userId))

                {

                    MessageBox.Show("Template updated successfully.", "Success",

                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;

                    this.Close();

                }

            }

            catch (Exception ex)

            {

                MessageBox.Show($"Error saving template: {ex.Message}", "Error",

                    MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }

    }

}