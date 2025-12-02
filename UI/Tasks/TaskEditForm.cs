using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using ReactCRM.Database;
using ReactCRM.Models;
using ReactCRM.Services;

namespace ReactCRM.UI.Tasks
{
    // Helper class for combo box items

    public class ComboBoxItem

    {

        public string Text { get; set; }

        public int Value { get; set; }



        public override string ToString()

        {

            return Text;

        }

    }
    public partial class TaskEditForm : Form
    {
        private TodoTask task;
        private TodoTaskRepository repository;
        private WorkerRepository workerRepository;
        private ClientRepository clientRepository;
        private bool isEditMode;

        private TextBox txtTitle;
        private TextBox txtDescription;
        private ComboBox cmbPriority;
        private ComboBox cmbStatus;
        private DateTimePicker dtpDueDate;
        private CheckBox chkHasDueDate;
        private ComboBox cmbAssignedTo;
        private ComboBox cmbClient;
        private CheckBox chkIsGlobalTask;
        private TextBox txtNotes;
        private Button btnSave;
        private Button btnCancel;

        public TaskEditForm()
        {
            this.repository = new TodoTaskRepository();
            this.workerRepository = new WorkerRepository();
            this.clientRepository = new ClientRepository();
            this.task = new TodoTask
            {
                CreatedByUserId = AuthService.Instance.GetCurrentUserId(),
                Priority = 2, // Medium
                Status = "Pending",
                CreatedDate = DateTime.Now
            };
            this.isEditMode = false;

            InitializeComponent();
            LoadTaskData();
        }

        public TaskEditForm(TodoTask task)
        {
            this.repository = new TodoTaskRepository();
            this.workerRepository = new WorkerRepository();
            this.clientRepository = new ClientRepository();
            this.task = task;
            this.isEditMode = true;

            InitializeComponent();
            LoadTaskData();
        }

        public TaskEditForm(int clientId)
        {
            this.repository = new TodoTaskRepository();
            this.workerRepository = new WorkerRepository();
            this.clientRepository = new ClientRepository();
            this.task = new TodoTask
            {
                CreatedByUserId = AuthService.Instance.GetCurrentUserId(),
                Priority = 2, // Medium
                Status = "Pending",
                CreatedDate = DateTime.Now,
                ClientId = clientId
            };
            this.isEditMode = false;

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

            // Main panel with scroll
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20)
            };

            int yPos = 20;
            int labelWidth = 120;
            int fieldWidth = 420;
            int fieldHeight = 25;
            int spacing = 15;

            // Title
            var lblTitle = new Label
            {
                Text = "Title:",
                Location = new Point(20, yPos),
                Size = new Size(labelWidth, fieldHeight),
                Font = UITheme.Fonts.BodyRegular,
                TextAlign = ContentAlignment.MiddleLeft
            };
            mainPanel.Controls.Add(lblTitle);

            txtTitle = new TextBox
            {
                Location = new Point(150, yPos),
                Size = new Size(fieldWidth, fieldHeight),
                Font = UITheme.Fonts.BodyRegular
            };
            mainPanel.Controls.Add(txtTitle);
            yPos += fieldHeight + spacing;

            // Description
            var lblDescription = new Label
            {
                Text = "Description:",
                Location = new Point(20, yPos),
                Size = new Size(labelWidth, fieldHeight),
                Font = UITheme.Fonts.BodyRegular,
                TextAlign = ContentAlignment.MiddleLeft
            };
            mainPanel.Controls.Add(lblDescription);

            txtDescription = new TextBox
            {
                Location = new Point(150, yPos),
                Size = new Size(fieldWidth, 80),
                Font = UITheme.Fonts.BodyRegular,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            mainPanel.Controls.Add(txtDescription);
            yPos += 80 + spacing;

            // Priority
            var lblPriority = new Label
            {
                Text = "Priority:",
                Location = new Point(20, yPos),
                Size = new Size(labelWidth, fieldHeight),
                Font = UITheme.Fonts.BodyRegular,
                TextAlign = ContentAlignment.MiddleLeft
            };
            mainPanel.Controls.Add(lblPriority);

            cmbPriority = new ComboBox
            {
                Location = new Point(150, yPos),
                Size = new Size(200, fieldHeight),
                Font = UITheme.Fonts.BodyRegular,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbPriority.Items.AddRange(new object[] { "1 - Low", "2 - Medium", "3 - High", "4 - Urgent" });
            cmbPriority.SelectedIndex = 1; // Default to Medium
            mainPanel.Controls.Add(cmbPriority);
            yPos += fieldHeight + spacing;

            // Status
            var lblStatus = new Label
            {
                Text = "Status:",
                Location = new Point(20, yPos),
                Size = new Size(labelWidth, fieldHeight),
                Font = UITheme.Fonts.BodyRegular,
                TextAlign = ContentAlignment.MiddleLeft
            };
            mainPanel.Controls.Add(lblStatus);

            cmbStatus = new ComboBox
            {
                Location = new Point(150, yPos),
                Size = new Size(200, fieldHeight),
                Font = UITheme.Fonts.BodyRegular,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatus.Items.AddRange(new object[] { "Pending", "InProgress", "Completed", "Cancelled" });
            cmbStatus.SelectedIndex = 0; // Default to Pending
            mainPanel.Controls.Add(cmbStatus);
            yPos += fieldHeight + spacing;

            // Due Date checkbox
            chkHasDueDate = new CheckBox
            {
                Text = "Set Due Date:",
                Location = new Point(20, yPos),
                Size = new Size(labelWidth, fieldHeight),
                Font = UITheme.Fonts.BodyRegular,
                Checked = false
            };
            chkHasDueDate.CheckedChanged += (s, e) =>
            {
                dtpDueDate.Enabled = chkHasDueDate.Checked;
            };
            mainPanel.Controls.Add(chkHasDueDate);

            dtpDueDate = new DateTimePicker
            {
                Location = new Point(150, yPos),
                Size = new Size(200, fieldHeight),
                Font = UITheme.Fonts.BodyRegular,
                Format = DateTimePickerFormat.Short,
                Enabled = false
            };
            mainPanel.Controls.Add(dtpDueDate);
            yPos += fieldHeight + spacing;

            // Assigned To
            var lblAssignedTo = new Label
            {
                Text = "Assigned To:",
                Location = new Point(20, yPos),
                Size = new Size(labelWidth, fieldHeight),
                Font = UITheme.Fonts.BodyRegular,
                TextAlign = ContentAlignment.MiddleLeft
            };
            mainPanel.Controls.Add(lblAssignedTo);

            cmbAssignedTo = new ComboBox
            {
                Location = new Point(150, yPos),
                Size = new Size(200, fieldHeight),
                Font = UITheme.Fonts.BodyRegular,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Load workers
            var workers = workerRepository.GetAllWorkers().Where(w => w.IsActive).ToList();
            cmbAssignedTo.Items.Add("(Unassigned)");
            foreach (var worker in workers)
            {
                cmbAssignedTo.Items.Add(new ComboBoxItem { Text = worker.Username, Value = worker.Id });
            }
            cmbAssignedTo.SelectedIndex = 0;
            mainPanel.Controls.Add(cmbAssignedTo);
            yPos += fieldHeight + spacing;

            // Global Task checkbox
            chkIsGlobalTask = new CheckBox
            {
                Text = "Global Team Task (not client-specific)",
                Location = new Point(20, yPos),
                Size = new Size(fieldWidth, fieldHeight),
                Font = UITheme.Fonts.BodyRegular,
                Checked = true
            };
            chkIsGlobalTask.CheckedChanged += (s, e) =>
            {
                cmbClient.Enabled = !chkIsGlobalTask.Checked;
            };
            mainPanel.Controls.Add(chkIsGlobalTask);
            yPos += fieldHeight + spacing;

            // Client
            var lblClient = new Label
            {
                Text = "Client:",
                Location = new Point(20, yPos),
                Size = new Size(labelWidth, fieldHeight),
                Font = UITheme.Fonts.BodyRegular,
                TextAlign = ContentAlignment.MiddleLeft
            };
            mainPanel.Controls.Add(lblClient);

            cmbClient = new ComboBox
            {
                Location = new Point(150, yPos),
                Size = new Size(fieldWidth, fieldHeight),
                Font = UITheme.Fonts.BodyRegular,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Enabled = false
            };

            // Load clients
            var clients = clientRepository.GetAllClients().OrderBy(c => c.Name).ToList();
            cmbClient.Items.Add("(No client)");
            foreach (var client in clients)
            {
                // Use GetFullName to include last name from ExtraData if available
                cmbClient.Items.Add(new ComboBoxItem { Text = client.GetFullName(), Value = client.Id });
            }
            cmbClient.SelectedIndex = 0;
            mainPanel.Controls.Add(cmbClient);
            yPos += fieldHeight + spacing;

            // Notes
            var lblNotes = new Label
            {
                Text = "Notes:",
                Location = new Point(20, yPos),
                Size = new Size(labelWidth, fieldHeight),
                Font = UITheme.Fonts.BodyRegular,
                TextAlign = ContentAlignment.MiddleLeft
            };
            mainPanel.Controls.Add(lblNotes);

            txtNotes = new TextBox
            {
                Location = new Point(150, yPos),
                Size = new Size(fieldWidth, 80),
                Font = UITheme.Fonts.BodyRegular,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            mainPanel.Controls.Add(txtNotes);
            yPos += 80 + spacing;

            // Button panel
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(20, 10, 20, 10)
            };

            btnCancel = new Button
            {
                Text = "Cancel",
                Width = 100,
                Height = 35,
                DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(127, 140, 141),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = UITheme.Fonts.BodyRegular,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Location = new Point(buttonPanel.Width - 230, 10);
            buttonPanel.Controls.Add(btnCancel);

            btnSave = new Button
            {
                Text = "Save",
                Width = 100,
                Height = 35,
                BackColor = UITheme.Colors.ButtonSuccess,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = UITheme.Fonts.BodyRegular,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Location = new Point(buttonPanel.Width - 120, 10);
            btnSave.Click += BtnSave_Click;
            buttonPanel.Controls.Add(btnSave);

            this.Controls.Add(mainPanel);
            this.Controls.Add(buttonPanel);
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void LoadTaskData()
        {
            if (task != null)
            {
                // Load all data if in edit mode
                if (isEditMode)
                {
                    txtTitle.Text = task.Title;
                    txtDescription.Text = task.Description;
                    cmbPriority.SelectedIndex = task.Priority - 1; // Priority is 1-4, index is 0-3
                    cmbStatus.SelectedText = task.Status;

                    if (task.DueDate.HasValue)
                    {
                        chkHasDueDate.Checked = true;
                        dtpDueDate.Value = task.DueDate.Value;
                    }

                    // Set assigned to
                    if (task.AssignedToUserId.HasValue)
                    {
                        for (int i = 1; i < cmbAssignedTo.Items.Count; i++)
                        {
                            if (cmbAssignedTo.Items[i] is ComboBoxItem item && item.Value == task.AssignedToUserId.Value)

                            {

                                cmbAssignedTo.SelectedIndex = i;

                                break;

                            }

                        }

                    }



                    txtNotes.Text = task.Notes;

                }



                // Set client (works for both edit mode and new task with pre-selected client)

                if (task.ClientId.HasValue)

                {

                    chkIsGlobalTask.Checked = false;

                    for (int i = 1; i < cmbClient.Items.Count; i++)

                    {

                        if (cmbClient.Items[i] is ComboBoxItem item && item.Value == task.ClientId.Value)

                        {

                            cmbClient.SelectedIndex = i;

                            break;
                        }
                    }
                }
                else
                {
                    chkIsGlobalTask.Checked = true;
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

            // Update task object
            task.Title = txtTitle.Text.Trim();
            task.Description = txtDescription.Text.Trim();
            task.Priority = cmbPriority.SelectedIndex + 1; // Index is 0-3, priority is 1-4
            task.Status = cmbStatus.SelectedItem.ToString();
            task.DueDate = chkHasDueDate.Checked ? (DateTime?)dtpDueDate.Value : null;
            task.Notes = txtNotes.Text.Trim();

            // Assigned to
            if (cmbAssignedTo.SelectedIndex > 0 && cmbAssignedTo.SelectedItem is ComboBoxItem selectedWorker)

            {

                task.AssignedToUserId = selectedWorker.Value;

            }

            else

            {

                task.AssignedToUserId = null;

            }



            // Client

            if (chkIsGlobalTask.Checked)

            {

                // Global task - no client

                task.ClientId = null;

            }

            else if (cmbClient.SelectedIndex > 0 && cmbClient.SelectedItem is ComboBoxItem selectedClient)

            {

                // Client selected in dropdown

                task.ClientId = selectedClient.Value;

            }

            // If neither condition is met, keep the existing ClientId (don't overwrite to null)

            try
            {
                if (isEditMode)
                {
                    repository.UpdateTask(task);
                    AuditService.LogAction("Update", "Task", task.Id,
                        $"Updated task: {task.Title}");
                }
                else
                {
                    int newId = repository.CreateTask(task);
                    task.Id = newId;
                    AuditService.LogAction("Create", "Task", newId,
                        $"Created task: {task.Title}");
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