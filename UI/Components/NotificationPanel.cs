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
    /// Notification panel that displays user notifications in a dropdown
    /// </summary>
    public class NotificationPanel : UserControl
    {
        private Panel panelHeader;
        private Label lblTitle;
        private Button btnMarkAllRead;
        private Panel panelNotifications;
        private Label lblNoNotifications;
        private NotificationRepository notificationRepo;
        private int currentUserId;
        private System.Windows.Forms.Timer refreshTimer;

        public event EventHandler NotificationClicked;
        public event EventHandler AllNotificationsRead;

        public NotificationPanel()
        {
            InitializeComponent();
            notificationRepo = new NotificationRepository();
            RefreshNotifications();

            // Auto-refresh every 30 seconds
            refreshTimer = new System.Windows.Forms.Timer { Interval = 30000 };
            refreshTimer.Tick += (s, e) => RefreshNotifications();
            refreshTimer.Start();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(380, 500);
            this.BackColor = Color.White;
            this.BorderStyle = BorderStyle.FixedSingle;

            // Header
            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = UITheme.Colors.HeaderPrimary,
                Padding = new Padding(15, 10, 15, 10)
            };

            lblTitle = new Label
            {
                Text = "Notifications",
                Font = UITheme.Fonts.HeaderSmall,
                ForeColor = Color.White,
                Dock = DockStyle.Left,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft
            };

            btnMarkAllRead = new Button
            {
                Text = "Mark all read",
                Dock = DockStyle.Right,
                Width = 100,
                FlatStyle = FlatStyle.Flat,
                BackColor = UITheme.Colors.ButtonSuccess,
                ForeColor = Color.White,
                Font = UITheme.Fonts.BodySmall,
                Cursor = Cursors.Hand
            };
            btnMarkAllRead.FlatAppearance.BorderSize = 0;
            btnMarkAllRead.Click += BtnMarkAllRead_Click;

            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(btnMarkAllRead);

            // Notifications container
            panelNotifications = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            // No notifications label
            lblNoNotifications = new Label
            {
                Text = "No new notifications",
                Font = UITheme.Fonts.BodyRegular,
                ForeColor = UITheme.Colors.TextSecondary,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Visible = false
            };

            panelNotifications.Controls.Add(lblNoNotifications);

            this.Controls.Add(panelNotifications);
            this.Controls.Add(panelHeader);
        }

        public void SetCurrentUser(int userId)
        {
            currentUserId = userId;
            RefreshNotifications();
        }

        public void RefreshNotifications()
        {
            if (currentUserId == 0)
                currentUserId = AuthService.Instance.GetCurrentUserId();

            // Clear existing notification items
            foreach (Control ctrl in panelNotifications.Controls.OfType<Panel>().ToList())
            {
                panelNotifications.Controls.Remove(ctrl);
                ctrl.Dispose();
            }

            var notifications = notificationRepo.GetUserNotifications(currentUserId, includeRead: false);

            if (notifications.Count == 0)
            {
                lblNoNotifications.Visible = true;
                lblNoNotifications.BringToFront();
                return;
            }

            lblNoNotifications.Visible = false;

            int yPos = 10;
            foreach (var notification in notifications.OrderByDescending(n => n.CreatedDate))
            {
                var notifCard = CreateNotificationCard(notification, yPos);
                panelNotifications.Controls.Add(notifCard);
                yPos += notifCard.Height + 10;
            }
        }

        private Panel CreateNotificationCard(Notification notification, int yPos)
        {
            var card = new Panel
            {
                Location = new Point(10, yPos),
                Size = new Size(panelNotifications.Width - 40, 90),
                BorderStyle = BorderStyle.None,
                BackColor = notification.IsRead ? UITheme.Colors.BackgroundSecondary : Color.White,
                Cursor = Cursors.Hand,
                Tag = notification
            };

            // Add left color indicator
            var colorBar = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(4, card.Height),
                BackColor = notification.GetTypeColor()
            };
            card.Controls.Add(colorBar);

            // Icon label
            var lblIcon = new Label
            {
                Text = notification.GetDefaultIcon(),
                Font = new Font(UITheme.Fonts.PrimaryFont, 20),
                Location = new Point(15, 10),
                Size = new Size(40, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };
            card.Controls.Add(lblIcon);

            // Title
            var lblTitle = new Label
            {
                Text = notification.Title,
                Font = UITheme.Fonts.BodyRegular,
                ForeColor = UITheme.Colors.TextPrimary,
                Location = new Point(65, 10),
                Size = new Size(card.Width - 75, 20),
                AutoEllipsis = true
            };
            card.Controls.Add(lblTitle);

            // Message
            var lblMessage = new Label
            {
                Text = notification.Message,
                Font = UITheme.Fonts.BodySmall,
                ForeColor = UITheme.Colors.TextSecondary,
                Location = new Point(65, 32),
                Size = new Size(card.Width - 75, 35),
                AutoEllipsis = true
            };
            card.Controls.Add(lblMessage);

            // Time ago
            var timeAgo = GetTimeAgo(notification.CreatedDate);
            var lblTime = new Label
            {
                Text = timeAgo,
                Font = UITheme.Fonts.BodySmall,
                ForeColor = UITheme.Colors.TextTertiary,
                Location = new Point(65, 70),
                Size = new Size(card.Width - 75, 15),
                AutoEllipsis = true
            };
            card.Controls.Add(lblTime);

            // Click event
            card.Click += (s, e) => NotificationCard_Click(notification);
            foreach (Control ctrl in card.Controls)
            {
                ctrl.Click += (s, e) => NotificationCard_Click(notification);
            }

            // Hover effect
            card.MouseEnter += (s, e) =>
            {
                card.BackColor = UITheme.Colors.BackgroundTertiary;
            };

            card.MouseLeave += (s, e) =>
            {
                card.BackColor = notification.IsRead ? UITheme.Colors.BackgroundSecondary : Color.White;
            };

            return card;
        }

        private void NotificationCard_Click(Notification notification)
        {
            // Mark as read
            if (!notification.IsRead)
            {
                notificationRepo.MarkAsRead(notification.Id);
                RefreshNotifications();
            }

            // Raise event for parent form to handle
            NotificationClicked?.Invoke(notification, EventArgs.Empty);
        }

        private void BtnMarkAllRead_Click(object sender, EventArgs e)
        {
            notificationRepo.MarkAllAsRead(currentUserId);
            RefreshNotifications();
            AllNotificationsRead?.Invoke(this, EventArgs.Empty);
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays}d ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)}w ago";

            return dateTime.ToString("MMM dd");
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible)
            {
                RefreshNotifications();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                refreshTimer?.Stop();
                refreshTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
