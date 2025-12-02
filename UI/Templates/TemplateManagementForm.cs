using System;

using System.Collections.Generic;

using System.Drawing;

using System.IO;

using System.Windows.Forms;

using ReactCRM.Database;

using ReactCRM.Models;

using ReactCRM.Services;



namespace ReactCRM.UI.Templates

{

    public class TemplateManagementForm : Form

    {

        private DataGridView dgvTemplates;

        private Button btnImport;

        private Button btnDelete;

        private Button btnRefresh;

        private Button btnClose;

        private Label lblTitle;

        private Panel panelButtons;



        public TemplateManagementForm()

        {

            InitializeComponent();

            LoadTemplates();

        }



        private void InitializeComponent()

        {

            this.Text = "PDF Template Management";

            this.Size = new Size(900, 600);

            this.StartPosition = FormStartPosition.CenterParent;

            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            this.MaximizeBox = false;

            this.MinimizeBox = false;



            // Title

            lblTitle = new Label

            {

                Text = "PDF Templates",

                Font = new Font("Segoe UI", 16, FontStyle.Bold),

                Location = new Point(20, 15),

                AutoSize = true

            };

            this.Controls.Add(lblTitle);



            // Button panel

            panelButtons = new Panel

            {

                Location = new Point(20, 50),

                Size = new Size(840, 40)

            };



            btnImport = new Button

            {

                Text = "Import Template",

                Size = new Size(130, 30),

                Location = new Point(0, 5)

            };

            btnImport.Click += BtnImport_Click;



            btnDelete = new Button

            {

                Text = "Delete",

                Size = new Size(100, 30),

                Location = new Point(140, 5)

            };

            btnDelete.Click += BtnDelete_Click;



            btnRefresh = new Button

            {

                Text = "Refresh",

                Size = new Size(100, 30),

                Location = new Point(250, 5)

            };

            btnRefresh.Click += BtnRefresh_Click;



            btnClose = new Button

            {

                Text = "Close",

                Size = new Size(100, 30),

                Location = new Point(740, 5)

            };

            btnClose.Click += (s, e) => this.Close();



            panelButtons.Controls.AddRange(new Control[] { btnImport, btnDelete, btnRefresh, btnClose });

            this.Controls.Add(panelButtons);



            // DataGridView

            dgvTemplates = new DataGridView

            {

                Location = new Point(20, 100),

                Size = new Size(840, 440),

                AllowUserToAddRows = false,

                AllowUserToDeleteRows = false,

                ReadOnly = true,

                SelectionMode = DataGridViewSelectionMode.FullRowSelect,

                MultiSelect = false,

                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,

                BackgroundColor = Color.White,

                BorderStyle = BorderStyle.Fixed3D

            };



            dgvTemplates.Columns.Add("Id", "ID");

            dgvTemplates.Columns.Add("Name", "Name");

            dgvTemplates.Columns.Add("Description", "Description");

            dgvTemplates.Columns.Add("Category", "Category");

            dgvTemplates.Columns.Add("FileName", "File Name");

            dgvTemplates.Columns.Add("IsActive", "Active");



            dgvTemplates.Columns["Id"].Width = 50;

            dgvTemplates.Columns["Name"].Width = 150;

            dgvTemplates.Columns["Description"].Width = 200;

            dgvTemplates.Columns["Category"].Width = 100;

            dgvTemplates.Columns["FileName"].Width = 150;

            dgvTemplates.Columns["IsActive"].Width = 60;



            dgvTemplates.CellDoubleClick += DgvTemplates_CellDoubleClick;

            this.Controls.Add(dgvTemplates);

        }



        private void LoadTemplates()

        {

            dgvTemplates.Rows.Clear();

            var templates = PdfTemplateRepository.GetAll();



            foreach (var template in templates)

            {

                dgvTemplates.Rows.Add(

                    template.Id,

                    template.Name,

                    template.Description,

                    template.Category,

                    template.FileName,

                    template.IsActive ? "Yes" : "No"

                );

            }

        }



        private void BtnImport_Click(object? sender, EventArgs e)

        {

            using var openFileDialog = new OpenFileDialog

            {

                Filter = "PDF Files (*.pdf)|*.pdf",

                Title = "Select PDF Template"

            };



            if (openFileDialog.ShowDialog() == DialogResult.OK)

            {

                string sourcePath = openFileDialog.FileName;

                string fileName = Path.GetFileNameWithoutExtension(sourcePath);



                // Show input dialog for template details

                using var inputForm = new TemplateImportDialog(fileName);

                if (inputForm.ShowDialog() == DialogResult.OK)

                {

                    try

                    {

                        int userId = AuthService.Instance.CurrentUserId;

                        PdfTemplateService.Instance.ImportTemplate(

                            sourcePath,

                            inputForm.TemplateName,

                            inputForm.Description,

                            inputForm.Category,

                            userId

                        );



                        MessageBox.Show("Template imported successfully!", "Success",

                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LoadTemplates();

                    }

                    catch (Exception ex)

                    {

                        MessageBox.Show($"Error importing template: {ex.Message}", "Error",

                            MessageBoxButtons.OK, MessageBoxIcon.Error);

                    }

                }

            }

        }



        private void BtnDelete_Click(object? sender, EventArgs e)

        {

            if (dgvTemplates.SelectedRows.Count == 0)

            {

                MessageBox.Show("Please select a template to delete.", "Warning",

                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }



            var result = MessageBox.Show("Are you sure you want to delete this template?",

                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);



            if (result == DialogResult.Yes)

            {

                int templateId = Convert.ToInt32(dgvTemplates.SelectedRows[0].Cells["Id"].Value);

                int userId = AuthService.Instance.CurrentUserId;



                if (PdfTemplateService.Instance.DeleteTemplate(templateId, userId))

                {

                    MessageBox.Show("Template deleted successfully!", "Success",

                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadTemplates();

                }

                else

                {

                    MessageBox.Show("Error deleting template.", "Error",

                        MessageBoxButtons.OK, MessageBoxIcon.Error);

                }

            }

        }



        private void BtnRefresh_Click(object? sender, EventArgs e)

        {

            LoadTemplates();

        }



        private void DgvTemplates_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)

        {

            if (e.RowIndex >= 0)

            {

                int templateId = Convert.ToInt32(dgvTemplates.Rows[e.RowIndex].Cells["Id"].Value);

                // TODO: Open template edit form

                MessageBox.Show($"Template ID: {templateId}\nDouble-click to edit coming soon!",

                    "Template Details", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }

        }

    }



    // Simple dialog for template import

    public class TemplateImportDialog : Form

    {

        private TextBox txtName;

        private TextBox txtDescription;

        private TextBox txtCategory;

        private Button btnOk;

        private Button btnCancel;



        public string TemplateName => txtName.Text;

        public string Description => txtDescription.Text;

        public string Category => txtCategory.Text;



        public TemplateImportDialog(string defaultName)

        {

            this.Text = "Import Template";

            this.Size = new Size(400, 250);

            this.StartPosition = FormStartPosition.CenterParent;

            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            this.MaximizeBox = false;

            this.MinimizeBox = false;



            var lblName = new Label { Text = "Template Name:", Location = new Point(20, 20), AutoSize = true };

            txtName = new TextBox { Location = new Point(20, 45), Size = new Size(340, 25), Text = defaultName };



            var lblDesc = new Label { Text = "Description:", Location = new Point(20, 80), AutoSize = true };

            txtDescription = new TextBox { Location = new Point(20, 105), Size = new Size(340, 25) };



            var lblCat = new Label { Text = "Category:", Location = new Point(20, 140), AutoSize = true };

            txtCategory = new TextBox { Location = new Point(20, 165), Size = new Size(340, 25) };



            btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(200, 200), Size = new Size(75, 30) };

            btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(285, 200), Size = new Size(75, 30) };



            this.AcceptButton = btnOk;

            this.CancelButton = btnCancel;



            this.Controls.AddRange(new Control[] { lblName, txtName, lblDesc, txtDescription, lblCat, txtCategory, btnOk, btnCancel });

        }

    }

}