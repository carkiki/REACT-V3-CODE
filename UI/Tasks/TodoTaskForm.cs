using System;
using System.Drawing;
using System.Windows.Forms;
using ReactCRM.Database;
using ReactCRM.Models;
using ReactCRM.Services;

namespace ReactCRM.UI.Tasks
{
    /// <summary>
    /// Form for creating and editing tasks
    /// </summary>
    public class TodoTaskForm : Form
    {
        private TodoTask task;
        private TodoTaskRepository taskRepo;
        private bool isEditMode;
        private int? clientId;

        // UI Controls
        private Label lblTitle;
        private TextBox txtTitle;
        private Label lblDescription;
        private TextBox txtDescription;
        private Label lblPriority;
        private ComboBox cboPriority;
        private Label lblDueDate;
        private DateTimePicker dtpDueDate;
        private CheckBox chkNoDueDate;
        private Label lblCategory;
        private TextBox txtCategory;
        private Label lblNotes;
        private TextBox txtNotes;
        private Button btnSave;
        private Button btnCancel;
        private Panel panelHeader;
        private Label lblFormTitle;

        public TodoTaskForm(TodoTask existingTask = null, int? clientId = null)
        {
            this.task = existingTask;
            this.clientId = clientId;
            this.isEditMode = existingTask != null;
            this.taskRepo = new TodoTaskRepository();

            InitializeComponent();
            LoadTaskData();
        }

        private void InitializeComponent()
        {
            this.Text = isEditMode ? "Edit Task" : "New Task";
            this.Size = new Size(600, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = UITheme.Colors.BackgroundPrimary;

            // Header Panel
            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = UITheme.Colors.HeaderPrimary,
                Padding = new Padding(20, 15, 20, 15)
            };

            lblFormTitle = new Label
            {
                Text = isEditMode ? "✏️ Edit Task" : "➕ New Task",
                Font = UITheme.Fonts.HeaderMedium,
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            panelHeader.Controls.Add(lblFormTitle);

            // Form content
            int yPos = 80;
            int leftMargin = 30;
            int labelWidth = 120;
            int controlWidth = 500;

            // Title
            lblTitle = new Label
            {
                Text = "Title *",
                Location = new Point(leftMargin, yPos),
                Size = new Size(labelWidth, 25),
                Font = UITheme.Fonts.BodyRegular,
                ForeColor = UITheme.Colors.TextPrimary
            };

            txtTitle = new TextBox
            {
                Location = new Point(leftMargin, yPos + 25),
                Size = new Size(controlWidth, 30),
                Font = UITheme.Fonts.BodyRegular
            };

            yPos += 70;

            // Description
            lblDescription = new Label
            {
                Text = "Description",
                Location = new Point(leftMargin, yPos),
                Size = new Size(labelWidth, 25),
                Font = UITheme.Fonts.BodyRegular,
                ForeColor = UITheme.Colors.TextPrimary
            };

            txtDescription = new TextBox
            {
                Location = new Point(leftMargin, yPos + 25),
                Size = new Size(controlWidth, 60),
                Font = UITheme.Fonts.BodyRegular,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            yPos += 100;

            // Priority
            lblPriority = new Label
            {
                Text = "Priority *",
                Location = new Point(leftMargin, yPos),
                Size = new Size(labelWidth, 25),
                Font = UITheme.Fonts.BodyRegular,
                ForeColor = UITheme.Colors.TextPrimary
            };

            cboPriority = new ComboBox
            {
                Location = new Point(leftMargin, yPos + 25),
                Size = new Size(200, 30),
                Font = UITheme.Fonts.BodyRegular,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboPriority.Items.AddRange(new object[] { "Low", "Medium", "High", "Urgent" });
            cboPriority.SelectedIndex = 1; // Default to Medium

            yPos += 70;

            // Due Date
            lblDueDate = new Label
            {
                Text = "Due Date",
                Location = new Point(leftMargin, yPos),
                Size = new Size(labelWidth, 25),
                Font = UITheme.Fonts.BodyRegular,
                ForeColor = UITheme.Colors.TextPrimary
            };

            dtpDueDate = new DateTimePicker
            {
                Location = new Point(leftMargin, yPos + 25),
                Size = new Size(300, 30),
                Font = UITheme.Fonts.BodyRegular,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now.AddDays(7) // Default to 1 week from now
            };

            chkNoDueDate = new CheckBox
            {
                Text = "No due date",
                Location = new Point(leftMargin + 320, yPos + 28),
                Size = new Size(150, 25),
                Font = UITheme.Fonts.BodyRegular,
                ForeColor = UITheme.Colors.TextPrimary
            };
            chkNoDueDate.CheckedChanged += (s, e) =>
            {
                dtpDueDate.Enabled = !chkNoDueDate.Checked;
            };

            yPos += 70;

            // Category
            lblCategory = new Label
            {
                Text = "Category",
                Location = new Point(leftMargin, yPos),
                Size = new Size(labelWidth, 25),
                Font = UITheme.Fonts.BodyRegular,
                ForeColor = UITheme.Colors.TextPrimary
            };

            txtCategory = new TextBox
            {
                Location = new Point(leftMargin, yPos + 25),
                Size = new Size(controlWidth, 30),
                Font = UITheme.Fonts.BodyRegular,
                PlaceholderText = "e.g., Follow-up, Meeting, Documentation"
            };

            yPos += 70;

            // Notes
            lblNotes = new Label
            {
                Text = "Notes",
                Location = new Point(leftMargin, yPos),
                Size = new Size(labelWidth, 25),
                Font = UITheme.Fonts.BodyRegular,
                ForeColor = UITheme.Colors.TextPrimary
            };

            txtNotes = new TextBox
            {
                Location = new Point(leftMargin, yPos + 25),
                Size = new Size(controlWidth, 60),
                Font = UITheme.Fonts.BodyRegular,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            yPos += 100;

            // Buttons
            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(leftMargin + controlWidth - 200, yPos),
                Size = new Size(90, 35),
                Font = UITheme.Fonts.BodyRegular,
                BackColor = UITheme.Colors.ButtonSecondary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            btnSave = new Button
            {
                Text = isEditMode ? "Update" : "Create",
                Location = new Point(leftMargin + controlWidth - 100, yPos),
                Size = new Size(100, 35),
                Font = UITheme.Fonts.BodyRegular,
                BackColor = UITheme.Colors.ButtonSuccess,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            // Add controls to form
            this.Controls.Add(panelHeader);
            this.Controls.Add(lblTitle);
            this.Controls.Add(txtTitle);
            this.Controls.Add(lblDescription);
            this.Controls.Add(txtDescription);
            this.Controls.Add(lblPriority);
            this.Controls.Add(cboPriority);
            this.Controls.Add(lblDueDate);
            this.Controls.Add(dtpDueDate);
            this.Controls.Add(chkNoDueDate);
            this.Controls.Add(lblCategory);
            this.Controls.Add(txtCategory);
            this.Controls.Add(lblNotes);
            this.Controls.Add(txtNotes);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
        }

        private void LoadTaskData()
        {
            if (isEditMode && task != null)
            {
                txtTitle.Text = task.Title;
                txtDescription.Text = task.Description;
                cboPriority.SelectedIndex = task.Priority - 1; // Priority is 1-4, index is 0-3
                txtCategory.Text = task.Category;
                txtNotes.Text = task.Notes;

                if (task.DueDate.HasValue)
                {
                    dtpDueDate.Value = task.DueDate.Value;
                    chkNoDueDate.Checked = false;
                }
                else
                {
                    chkNoDueDate.Checked = true;
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Please enter a task title.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTitle.Focus();
                return;
            }

            try
            {
                if (isEditMode)
                {
                    // Update existing task
                    task.Title = txtTitle.Text.Trim();
                    task.Description = txtDescription.Text.Trim();
                    task.Priority = cboPriority.SelectedIndex + 1;
                    task.DueDate = chkNoDueDate.Checked ? null : dtpDueDate.Value;
                    task.Category = txtCategory.Text.Trim();
                    task.Notes = txtNotes.Text.Trim();

                    taskRepo.UpdateTask(task);
                    MessageBox.Show("Task updated successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Create new task
                    var newTask = new TodoTask
                    {
                        Title = txtTitle.Text.Trim(),
                        Description = txtDescription.Text.Trim(),
                        Priority = cboPriority.SelectedIndex + 1,
                        DueDate = chkNoDueDate.Checked ? null : dtpDueDate.Value,
                        Category = txtCategory.Text.Trim(),
                        Notes = txtNotes.Text.Trim(),
                        CreatedByUserId = AuthService.Instance.GetCurrentUserId(),
                        ClientId = clientId,
                        Status = "Pending",
                        CreatedDate = DateTime.Now,
                        IsActive = true
                    };

                    taskRepo.CreateTask(newTask);
                    MessageBox.Show("Task created successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving task: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
