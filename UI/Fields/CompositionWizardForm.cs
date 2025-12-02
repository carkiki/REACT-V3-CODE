using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using ReactCRM.Database;
using ReactCRM.Models;
using ReactCRM.Services;

namespace ReactCRM.UI.Fields
{
    public partial class CompositionWizardForm : Form
    {
        private DataGridView gridFields;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnClose;
        private Label lblInfo;
        private CustomFieldRepository repository;

        public CompositionWizardForm()
        {
            repository = new CustomFieldRepository();
            InitializeComponent();
            LoadFields();
        }

        private void InitializeComponent()
        {
            this.Text = "Custom Field Manager";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(700, 400);

            // Header Panel
            var panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(52, 73, 94),
                Padding = new Padding(15)
            };

            var lblTitle = new Label
            {
                Text = "Custom Field Composition Wizard",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 10),
                AutoSize = true
            };

            lblInfo = new Label
            {
                Text = "Create and manage custom fields for client records",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(189, 195, 199),
                Location = new Point(15, 40),
                AutoSize = true
            };

            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(lblInfo);

            // DataGridView
            gridFields = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9)
            };
            gridFields.DoubleClick += BtnEdit_Click;

            // Bottom Panel
            var panelBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(236, 240, 241),
                Padding = new Padding(10)
            };

            btnAdd = new Button
            {
                Text = "Add Field",
                Location = new Point(10, 12),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnAdd.Click += BtnAdd_Click;

            btnEdit = new Button
            {
                Text = "Edit Field",
                Location = new Point(140, 12),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(241, 196, 15),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnEdit.Click += BtnEdit_Click;

            btnDelete = new Button
            {
                Text = "Delete Field",
                Location = new Point(270, 12),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnDelete.Click += BtnDelete_Click;

            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(400, 12),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(127, 140, 141),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnClose.Click += (s, e) => this.Close();

            panelBottom.Controls.Add(btnAdd);
            panelBottom.Controls.Add(btnEdit);
            panelBottom.Controls.Add(btnDelete);
            panelBottom.Controls.Add(btnClose);

            // Add controls to form
            this.Controls.Add(gridFields);
            this.Controls.Add(panelHeader);
            this.Controls.Add(panelBottom);
        }

        private void LoadFields()
        {
            try
            {
                var fields = repository.GetAll();
                gridFields.DataSource = null;
                gridFields.Columns.Clear();

                var displayData = fields.Select(f => new
                {
                    f.Id,
                    f.FieldName,
                    f.Label,
                    f.FieldType,
                    Required = f.IsRequired ? "Yes" : "No",
                    f.DefaultValue,
                    Created = f.CreatedAt.ToString("yyyy-MM-dd")
                }).ToList();

                gridFields.DataSource = displayData;

                // Format columns - with null checks
                try
                {
                    if (gridFields?.Columns != null)
                    {
                        if (gridFields.Columns.Contains("Id"))
                        {
                            try { gridFields.Columns["Id"].Width = 50; } catch { }
                        }

                        if (gridFields.Columns.Contains("FieldName"))
                        {
                            try
                            {
                                gridFields.Columns["FieldName"].HeaderText = "Field Name";
                                gridFields.Columns["FieldName"].Width = 150;
                            }
                            catch { }
                        }

                        if (gridFields.Columns.Contains("Label"))
                        {
                            try { gridFields.Columns["Label"].Width = 200; } catch { }
                        }

                        if (gridFields.Columns.Contains("FieldType"))
                        {
                            try
                            {
                                gridFields.Columns["FieldType"].HeaderText = "Type";
                                gridFields.Columns["FieldType"].Width = 100;
                            }
                            catch { }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Column formatting error: {ex.Message}");
                }

                lblInfo.Text = $"Manage custom fields for client records - Total: {fields.Count}";

                AuditService.LogAction("View", "CustomFields", null,
                    $"Viewed custom fields list ({fields.Count} fields)");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading custom fields: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var form = new FieldEditorForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadFields();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (gridFields.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a field to edit.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int fieldId = (int)gridFields.SelectedRows[0].Cells["Id"].Value;
            var field = repository.GetAll().FirstOrDefault(f => f.Id == fieldId);

            if (field != null)
            {
                var form = new FieldEditorForm(field);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadFields();
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (gridFields.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a field to delete.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int fieldId = (int)gridFields.SelectedRows[0].Cells["Id"].Value;
            string fieldLabel = gridFields.SelectedRows[0].Cells["Label"].Value.ToString();

            var result = MessageBox.Show(
                $"Are you sure you want to delete the field '{fieldLabel}'?\n\nThis will not delete the data already stored in this field.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    repository.Delete(fieldId);
                    MessageBox.Show("Field deleted successfully.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadFields();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting field: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    // Field Editor Form
    public partial class FieldEditorForm : Form
    {
        private CustomField field;
        private bool isNewField;
        private CustomFieldRepository repository;

        private TextBox txtFieldName;
        private TextBox txtLabel;
        private ComboBox cboFieldType;
        private TextBox txtOptions;
        private TextBox txtDefaultValue;
        private CheckBox chkRequired;
        private Button btnSave;
        private Button btnCancel;
        private Label lblOptionsNote;

        public FieldEditorForm(CustomField existingField = null)
        {
            repository = new CustomFieldRepository();
            field = existingField ?? new CustomField();
            isNewField = existingField == null;

            InitializeComponent();
            LoadFieldData();
        }

        private void InitializeComponent()
        {
            this.Text = isNewField ? "Add Custom Field" : "Edit Custom Field";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int yPos = 20;
            int labelWidth = 120;
            int fieldX = 140;
            int fieldWidth = 320;
            int spacing = 50;

            // Field Name
            var lblFieldName = new Label
            {
                Text = "Field Name:",
                Location = new Point(20, yPos + 3),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10)
            };

            txtFieldName = new TextBox
            {
                Location = new Point(fieldX, yPos),
                Width = fieldWidth,
                Font = new Font("Segoe UI", 10)
            };
            yPos += spacing;

            // Label
            var lblLabel = new Label
            {
                Text = "Display Label:",
                Location = new Point(20, yPos + 3),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10)
            };

            txtLabel = new TextBox
            {
                Location = new Point(fieldX, yPos),
                Width = fieldWidth,
                Font = new Font("Segoe UI", 10)
            };
            yPos += spacing;

            // Field Type
            var lblFieldType = new Label
            {
                Text = "Field Type:",
                Location = new Point(20, yPos + 3),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10)
            };

            cboFieldType = new ComboBox
            {
                Location = new Point(fieldX, yPos),
                Width = fieldWidth,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cboFieldType.Items.AddRange(new object[] {
                CustomField.TYPE_TEXT,
                CustomField.TYPE_NUMBER,
                CustomField.TYPE_DATE,
                CustomField.TYPE_DROPDOWN
            });
            cboFieldType.SelectedIndex = 0;
            cboFieldType.SelectedIndexChanged += (s, e) => UpdateOptionsVisibility();
            yPos += spacing;

            // Options (for dropdown)
            var lblOptions = new Label
            {
                Text = "Options:",
                Location = new Point(20, yPos + 3),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10)
            };

            txtOptions = new TextBox
            {
                Location = new Point(fieldX, yPos),
                Width = fieldWidth,
                Height = 60,
                Multiline = true,
                Font = new Font("Segoe UI", 9),
                Visible = false
            };

            lblOptionsNote = new Label
            {
                Text = "Enter one option per line",
                Location = new Point(fieldX, yPos + 65),
                AutoSize = true,
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Color.Gray,
                Visible = false
            };
            yPos += 90;

            // Default Value
            var lblDefaultValue = new Label
            {
                Text = "Default Value:",
                Location = new Point(20, yPos + 3),
                Width = labelWidth,
                Font = new Font("Segoe UI", 10)
            };

            txtDefaultValue = new TextBox
            {
                Location = new Point(fieldX, yPos),
                Width = fieldWidth,
                Font = new Font("Segoe UI", 10)
            };
            yPos += spacing;

            // Is Required
            chkRequired = new CheckBox
            {
                Text = "This field is required",
                Location = new Point(fieldX, yPos),
                AutoSize = true,
                Font = new Font("Segoe UI", 10)
            };
            yPos += spacing;

            // Buttons
            btnSave = new Button
            {
                Text = "Save",
                Location = new Point(fieldX, yPos),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(fieldX + 110, yPos),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(127, 140, 141),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnCancel.Click += (s, e) => this.Close();

            // Add controls
            this.Controls.Add(lblFieldName);
            this.Controls.Add(txtFieldName);
            this.Controls.Add(lblLabel);
            this.Controls.Add(txtLabel);
            this.Controls.Add(lblFieldType);
            this.Controls.Add(cboFieldType);
            this.Controls.Add(lblOptions);
            this.Controls.Add(txtOptions);
            this.Controls.Add(lblOptionsNote);
            this.Controls.Add(lblDefaultValue);
            this.Controls.Add(txtDefaultValue);
            this.Controls.Add(chkRequired);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
        }

        private void UpdateOptionsVisibility()
        {
            bool isDropdown = cboFieldType.SelectedItem?.ToString() == CustomField.TYPE_DROPDOWN;
            txtOptions.Visible = isDropdown;
            lblOptionsNote.Visible = isDropdown;
        }

        private void LoadFieldData()
        {
            if (!isNewField)
            {
                txtFieldName.Text = field.FieldName;
                txtFieldName.ReadOnly = true; // Field name shouldn't be editable
                txtLabel.Text = field.Label;
                cboFieldType.SelectedItem = field.FieldType;
                txtDefaultValue.Text = field.DefaultValue;
                chkRequired.Checked = field.IsRequired;

                if (field.FieldType == CustomField.TYPE_DROPDOWN && !string.IsNullOrEmpty(field.Options))
                {
                    var options = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(field.Options);
                    txtOptions.Text = string.Join(Environment.NewLine, options);
                }
            }

            UpdateOptionsVisibility();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(txtFieldName.Text))
            {
                MessageBox.Show("Field Name is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFieldName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtLabel.Text))
            {
                MessageBox.Show("Display Label is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLabel.Focus();
                return;
            }

            // Validate field name format (alphanumeric and underscore only)
            if (!System.Text.RegularExpressions.Regex.IsMatch(txtFieldName.Text, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
            {
                MessageBox.Show("Field Name must start with a letter or underscore and contain only letters, numbers, and underscores.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate dropdown options
            if (cboFieldType.SelectedItem?.ToString() == CustomField.TYPE_DROPDOWN)
            {
                if (string.IsNullOrWhiteSpace(txtOptions.Text))
                {
                    MessageBox.Show("Dropdown options are required.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtOptions.Focus();
                    return;
                }
            }

            try
            {
                // Check for duplicate field name
                if (isNewField)
                {
                    var existing = repository.GetByFieldName(txtFieldName.Text.Trim());
                    if (existing != null)
                    {
                        MessageBox.Show("A field with this name already exists.", "Duplicate Field",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Update field object
                field.FieldName = txtFieldName.Text.Trim();
                field.Label = txtLabel.Text.Trim();
                field.FieldType = cboFieldType.SelectedItem.ToString();
                field.DefaultValue = txtDefaultValue.Text.Trim();
                field.IsRequired = chkRequired.Checked;

                // Process options for dropdown
                if (field.FieldType == CustomField.TYPE_DROPDOWN)
                {
                    var options = txtOptions.Text
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(o => o.Trim())
                        .Where(o => !string.IsNullOrEmpty(o))
                        .ToArray();

                    field.Options = Newtonsoft.Json.JsonConvert.SerializeObject(options);
                }
                else
                {
                    field.Options = null;
                }

                if (isNewField)
                {
                    repository.Create(field);
                    MessageBox.Show("Custom field created successfully.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    repository.Update(field);
                    MessageBox.Show("Custom field updated successfully.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving field: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}