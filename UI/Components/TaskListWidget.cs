using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ReactCRM.Database;
using ReactCRM.Models;
using ReactCRM.Services;

namespace ReactCRM.UI.Components
{
    /// <summary>
    /// Widget to display a list of tasks with checkboxes and priorities
    /// </summary>
    public class TaskListWidget : UserControl
    {
        private Panel panelHeader;
        private Label lblTitle;
        private Button btnAddTask;
        private Panel panelTasks;
        private TodoTaskRepository taskRepo;
        private bool showGlobalTasks = true;
        private int? clientId = null;

        public event EventHandler<TodoTask> TaskClicked;
        public event EventHandler<TodoTask> TaskCompleted;
        public event EventHandler AddTaskClicked;

        public TaskListWidget()
        {
            InitializeComponent();
            taskRepo = new TodoTaskRepository();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(400, 500);
            this.BackColor = Color.White;
            this.BorderStyle = BorderStyle.FixedSingle;

            // Header
            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = UITheme.Colors.HeaderPrimary,
                Padding = new Padding(15, 8, 15, 8)
            };

            lblTitle = new Label
            {
                Text = "Team Tasks",
                Font = UITheme.Fonts.HeaderSmall,
                ForeColor = Color.White,
                Dock = DockStyle.Left,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft
            };

            btnAddTask = new Button
            {
                Text = "+ New",
                Dock = DockStyle.Right,
                Width = 80,
                FlatStyle = FlatStyle.Flat,
                BackColor = UITheme.Colors.ButtonSuccess,
                ForeColor = Color.White,
                Font = UITheme.Fonts.BodySmall,
                Cursor = Cursors.Hand
            };
            btnAddTask.FlatAppearance.BorderSize = 0;
            btnAddTask.Click += (s, e) => AddTaskClicked?.Invoke(this, EventArgs.Empty);

            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(btnAddTask);

            // Tasks container
            panelTasks = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            this.Controls.Add(panelTasks);
            this.Controls.Add(panelHeader);
        }

        public void SetTitle(string title)
        {
            lblTitle.Text = title;
        }

        public void LoadGlobalTasks()
        {
            showGlobalTasks = true;
            clientId = null;
            RefreshTasks();
        }

        public void LoadClientTasks(int clientId)
        {
            showGlobalTasks = false;
            this.clientId = clientId;
            System.Diagnostics.Debug.WriteLine($"LoadClientTasks called for client ID: {clientId}");
            RefreshTasks();
        }

        public void LoadAllTasksGrouped()
        {
            // Load both global and client tasks, grouped
            showGlobalTasks = false;
            clientId = -1; // Special flag to show all tasks grouped
            RefreshTasks();
        }

        public void RefreshTasks()
        {
            // Clear existing task items
            panelTasks.Controls.Clear();

            int yPos = 10;

            // Special case: Show all tasks grouped (global + client)
            if (clientId == -1)
            {

                // Get global tasks
                var globalTasks = taskRepo.GetPendingTasks().Where(t => !t.ClientId.HasValue).ToList();

                // Get client-specific tasks
                var clientTasks = taskRepo.GetPendingTasks().Where(t => t.ClientId.HasValue).ToList();

                System.Diagnostics.Debug.WriteLine($"RefreshTasks (Grouped): {globalTasks.Count} global tasks, {clientTasks.Count} client tasks");

                // Show global tasks section
                if (globalTasks.Count > 0)
                {
                    var lblGlobalHeader = new Label
                    {
                        Text = "🌐 Global Team Tasks",
                        Font = new Font(UITheme.Fonts.BodyRegular.FontFamily, 10, FontStyle.Bold),
                        ForeColor = UITheme.Colors.HeaderPrimary,
                        Location = new Point(10, yPos),
                        AutoSize = true
                    };
                    panelTasks.Controls.Add(lblGlobalHeader);
                    yPos += 30;

                    foreach (var task in globalTasks.OrderByDescending(t => t.Priority).ThenBy(t => t.DueDate))
                    {
                        var taskCard = CreateTaskCard(task, yPos);
                        panelTasks.Controls.Add(taskCard);
                        yPos += taskCard.Height + 10;
                    }

                    yPos += 10; // Extra spacing before next section
                }

                // Show client tasks section
                if (clientTasks.Count > 0)
                {
                    var lblClientHeader = new Label
                    {
                        Text = "👥 Client-Specific Tasks",
                        Font = new Font(UITheme.Fonts.BodyRegular.FontFamily, 10, FontStyle.Bold),
                        ForeColor = UITheme.Colors.HeaderPrimary,
                        Location = new Point(10, yPos),
                        AutoSize = true
                    };
                    panelTasks.Controls.Add(lblClientHeader);
                    yPos += 30;

                    foreach (var task in clientTasks.OrderByDescending(t => t.Priority).ThenBy(t => t.DueDate))
                    {
                        var taskCard = CreateTaskCard(task, yPos);
                        panelTasks.Controls.Add(taskCard);
                        yPos += taskCard.Height + 10;
                    }
                }

                // Show empty message if no tasks at all
                if (globalTasks.Count == 0 && clientTasks.Count == 0)
                {
                    var lblEmpty = new Label
                    {
                        Text = "No tasks yet.\nClick '+ New' to add one!",
                        Font = UITheme.Fonts.BodyRegular,
                        ForeColor = UITheme.Colors.TextSecondary,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Fill
                    };
                    panelTasks.Controls.Add(lblEmpty);
                }

                return;
            }

            // Original behavior for single-mode display
            var tasks = showGlobalTasks
                ? taskRepo.GetPendingTasks().ToList()  // Show ALL tasks in team view (global + client-specific)
                : taskRepo.GetClientTasks(clientId ?? 0);

            System.Diagnostics.Debug.WriteLine($"RefreshTasks: Found {tasks.Count} tasks (showGlobalTasks: {showGlobalTasks}, clientId: {clientId})");

            if (tasks.Count == 0)
            {
                var lblEmpty = new Label
                {
                    Text = "No tasks yet.\nClick '+ New' to add one!",
                    Font = UITheme.Fonts.BodyRegular,
                    ForeColor = UITheme.Colors.TextSecondary,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                };
                panelTasks.Controls.Add(lblEmpty);
                return;
            }

            yPos = 10;
            foreach (var task in tasks.OrderByDescending(t => t.Priority).ThenBy(t => t.DueDate))
            {
                var taskCard = CreateTaskCard(task, yPos);
                panelTasks.Controls.Add(taskCard);
                yPos += taskCard.Height + 10;
            }
        }

        private Panel CreateTaskCard(TodoTask task, int yPos)
        {
            var card = new Panel
            {
                Location = new Point(10, yPos),
                Size = new Size(panelTasks.Width - 40, 70),
                BorderStyle = BorderStyle.None,
                BackColor = task.Status == "Completed" ? UITheme.Colors.BackgroundSecondary : Color.White,
                Cursor = Cursors.Hand,
                Tag = task
            };

            // Priority color bar
            var colorBar = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(4, card.Height),
                BackColor = task.GetPriorityColor()
            };
            card.Controls.Add(colorBar);

            // Checkbox
            var checkbox = new CheckBox
            {
                Location = new Point(15, 25),
                Size = new Size(20, 20),
                Checked = task.Status == "Completed",
                Tag = task
            };
            checkbox.CheckedChanged += (s, e) =>
            {
                if (checkbox.Checked && task.Status != "Completed")
                {
                    taskRepo.CompleteTask(task.Id);
                    TaskCompleted?.Invoke(this, task);
                    RefreshTasks();
                }
            };
            card.Controls.Add(checkbox);

            // Task title
            var lblTitle = new Label
            {
                Text = task.Title,
                Font = task.Status == "Completed"
                    ? new Font(UITheme.Fonts.BodyRegular, FontStyle.Strikeout)
                    : UITheme.Fonts.BodyRegular,
                ForeColor = task.Status == "Completed" ? UITheme.Colors.TextSecondary : UITheme.Colors.TextPrimary,
                Location = new Point(45, 12),
                Size = new Size(card.Width - 120, 20),
                AutoEllipsis = true
            };
            card.Controls.Add(lblTitle);

            // Priority badge
            var lblPriority = new Label
            {
                Text = task.GetPriorityLabel(),
                Font = UITheme.Fonts.BodySmall,
                ForeColor = Color.White,
                BackColor = task.GetPriorityColor(),
                Location = new Point(card.Width - 70, 10),
                Size = new Size(60, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            card.Controls.Add(lblPriority);

            // Due date
            string dueText = task.DueDate.HasValue
                ? task.DueDate.Value.ToString("MMM dd")
                : "No due date";

            if (task.IsOverdue())
                dueText += " (Overdue!)";

            var lblDue = new Label
            {
                Text = dueText,
                Font = UITheme.Fonts.BodySmall,
                ForeColor = task.IsOverdue() ? UITheme.Colors.StatusError : UITheme.Colors.TextTertiary,
                Location = new Point(45, 35),
                Size = new Size(card.Width - 55, 20),
                AutoEllipsis = true
            };
            card.Controls.Add(lblDue);

            // Description (if available)
            if (!string.IsNullOrEmpty(task.Description))
            {
                var lblDesc = new Label
                {
                    Text = task.Description,
                    Font = UITheme.Fonts.BodySmall,
                    ForeColor = UITheme.Colors.TextSecondary,
                    Location = new Point(45, 52),
                    Size = new Size(card.Width - 55, 15),
                    AutoEllipsis = true
                };
                card.Controls.Add(lblDesc);
            }

            // Click event (excluding checkbox)
            card.Click += (s, e) => TaskClicked?.Invoke(this, task);
            foreach (Control ctrl in card.Controls.OfType<Label>())
            {
                ctrl.Click += (s, e) => TaskClicked?.Invoke(this, task);
            }

            // Hover effect
            card.MouseEnter += (s, e) =>
            {
                card.BackColor = UITheme.Colors.BackgroundTertiary;
            };

            card.MouseLeave += (s, e) =>
            {
                card.BackColor = task.Status == "Completed" ? UITheme.Colors.BackgroundSecondary : Color.White;
            };

            return card;
        }
    }
}