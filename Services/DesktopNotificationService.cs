using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ReactCRM.Database;
using ReactCRM.Models;

namespace ReactCRM.Services
{
    /// <summary>
    /// Service for displaying Windows desktop notifications (balloon tips)
    /// Monitors for new notifications and shows them automatically
    /// </summary>
    public class DesktopNotificationService : IDisposable
    {
        private static DesktopNotificationService _instance;
        private static readonly object _lock = new object();

        private NotifyIcon notifyIcon;
        private NotificationRepository notificationRepo;
        private System.Windows.Forms.Timer checkTimer;
        private DateTime lastCheckTime;
        private int currentUserId;
        private bool isInitialized = false;
        private bool isDisposed = false;

        private DesktopNotificationService()
        {
            notificationRepo = new NotificationRepository();
            lastCheckTime = DateTime.Now;
        }

        public static DesktopNotificationService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DesktopNotificationService();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Initializes the desktop notification service with NotifyIcon
        /// Must be called from the main form
        /// </summary>
        public void Initialize(int userId, Form parentForm)
        {
            if (isInitialized)
                return;

            currentUserId = userId;

            // Create NotifyIcon for system tray
            notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Information,
                Visible = true,
                Text = "REACT CRM - Notifications"
            };

            // Handle balloon tip click
            notifyIcon.BalloonTipClicked += NotifyIcon_BalloonTipClicked;
            notifyIcon.Click += NotifyIcon_Click;

            // Create context menu for tray icon
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Show REACT CRM", null, (s, e) =>
            {
                parentForm.Show();
                parentForm.WindowState = FormWindowState.Normal;
                parentForm.BringToFront();
            });
            contextMenu.Items.Add("-"); // Separator
            contextMenu.Items.Add("Check for Notifications", null, (s, e) => CheckForNewNotifications(true));
            contextMenu.Items.Add("-"); // Separator
            contextMenu.Items.Add("Exit", null, (s, e) => Application.Exit());

            notifyIcon.ContextMenuStrip = contextMenu;

            // Setup timer to check for new notifications every 30 seconds
            checkTimer = new System.Windows.Forms.Timer
            {
                Interval = 30000 // 30 seconds
            };
            checkTimer.Tick += (s, e) => CheckForNewNotifications();
            checkTimer.Start();

            isInitialized = true;

            System.Diagnostics.Debug.WriteLine("[DesktopNotificationService] Initialized successfully");
        }

        /// <summary>
        /// Checks for new notifications and shows desktop balloon tips
        /// </summary>
        private void CheckForNewNotifications(bool forceShow = false)
        {
            try
            {
                if (!isInitialized || notifyIcon == null)
                    return;

                // Get notifications created since last check
                var newNotifications = notificationRepo.GetUserNotifications(currentUserId)
                    .Where(n => !n.IsRead && n.CreatedDate > lastCheckTime)
                    .OrderByDescending(n => n.CreatedDate)
                    .ToList();

                if (newNotifications.Any() || forceShow)
                {
                    var notification = newNotifications.FirstOrDefault();

                    if (notification != null)
                    {
                        ShowDesktopNotification(notification);

                        // If there are more notifications, show count
                        if (newNotifications.Count > 1)
                        {
                            System.Threading.Tasks.Task.Delay(3000).ContinueWith(_ =>
                            {
                                if (notifyIcon != null && !isDisposed)
                                {
                                    notifyIcon.ShowBalloonTip(
                                        3000,
                                        $"{newNotifications.Count - 1} More Notifications",
                                        $"You have {newNotifications.Count - 1} more unread notification(s).",
                                        ToolTipIcon.Info
                                    );
                                }
                            });
                        }
                    }
                    else if (forceShow)
                    {
                        // Manual check, show status
                        var unreadCount = notificationRepo.GetUnreadCount(currentUserId);
                        if (unreadCount > 0)
                        {
                            notifyIcon.ShowBalloonTip(
                                3000,
                                "REACT CRM Notifications",
                                $"You have {unreadCount} unread notification(s).",
                                ToolTipIcon.Info
                            );
                        }
                        else
                        {
                            notifyIcon.ShowBalloonTip(
                                2000,
                                "REACT CRM",
                                "No new notifications.",
                                ToolTipIcon.Info
                            );
                        }
                    }

                    lastCheckTime = DateTime.Now;
                }

                System.Diagnostics.Debug.WriteLine($"[DesktopNotificationService] Checked for notifications. Found {newNotifications.Count} new.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DesktopNotificationService] Error checking notifications: {ex.Message}");
            }
        }

        /// <summary>
        /// Shows a desktop notification for a specific notification
        /// </summary>
        public void ShowDesktopNotification(Notification notification)
        {
            if (!isInitialized || notifyIcon == null || notification == null)
                return;

            try
            {
                // Determine icon based on notification type
                ToolTipIcon icon = notification.Type switch
                {
                    "Error" => ToolTipIcon.Error,
                    "Warning" => ToolTipIcon.Warning,
                    "Success" => ToolTipIcon.Info,
                    "Birthday" => ToolTipIcon.Info,
                    _ => ToolTipIcon.Info
                };

                // Show balloon tip (Windows will display this as a toast notification on Win10/11)
                notifyIcon.ShowBalloonTip(
                    5000, // Duration in milliseconds
                    notification.Title ?? "REACT CRM",
                    notification.Message ?? "",
                    icon
                );

                System.Diagnostics.Debug.WriteLine($"[DesktopNotificationService] Showed notification: {notification.Title}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DesktopNotificationService] Error showing notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Manually trigger notification check
        /// </summary>
        public void ForceCheckNotifications()
        {
            CheckForNewNotifications(true);
        }

        private void NotifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            // When user clicks the balloon tip, we could navigate to notifications panel
            // This would require a reference to the main form
            System.Diagnostics.Debug.WriteLine("[DesktopNotificationService] Balloon tip clicked");
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            // Single click on tray icon - could show a quick status
            var mouseEvent = e as MouseEventArgs;
            if (mouseEvent != null && mouseEvent.Button == MouseButtons.Left)
            {
                CheckForNewNotifications(true);
            }
        }

        /// <summary>
        /// Updates the tray icon based on notification status
        /// </summary>
        public void UpdateTrayIcon(bool hasUnread)
        {
            if (!isInitialized || notifyIcon == null)
                return;

            try
            {
                // Change icon color/style based on unread status
                notifyIcon.Icon = hasUnread ? SystemIcons.Exclamation : SystemIcons.Information;

                var unreadCount = notificationRepo.GetUnreadCount(currentUserId);
                notifyIcon.Text = unreadCount > 0
                    ? $"REACT CRM - {unreadCount} unread notification(s)"
                    : "REACT CRM - No new notifications";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DesktopNotificationService] Error updating tray icon: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;

            checkTimer?.Stop();
            checkTimer?.Dispose();

            if (notifyIcon != null)
            {
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
            }

            isInitialized = false;
        }
    }
}