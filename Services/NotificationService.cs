using System;

using System.Collections.Generic;

using System.Linq;

using ReactCRM.Database;

using ReactCRM.Models;



namespace ReactCRM.Services

{

    /// <summary>

    /// Service for creating and managing system notifications

    /// </summary>

    public static class NotificationService

    {

        private static NotificationRepository notificationRepo = new NotificationRepository();

        private static WorkerRepository workerRepo = new WorkerRepository();



        /// <summary>

        /// Creates a notification for a specific user

        /// </summary>

        public static void CreateNotification(int userId, string title, string message, string type = "Info",

            int? relatedEntityId = null, string relatedEntityType = null, string icon = null)

        {

            var notification = new Notification

            {

                UserId = userId,

                Title = title,

                Message = message,

                Type = type,

                RelatedEntityId = relatedEntityId,

                RelatedEntityType = relatedEntityType,

                Icon = icon,

                CreatedDate = DateTime.Now,

                IsRead = false,

                IsActive = true

            };



            notificationRepo.CreateNotification(notification);

        }



        /// <summary>

        /// Creates a notification for all active users

        /// </summary>

        public static void CreateNotificationForAllUsers(string title, string message, string type = "Info",

            int? relatedEntityId = null, string relatedEntityType = null, string icon = null)

        {

            var workers = workerRepo.GetAllWorkers().Where(w => w.IsActive).ToList();



            foreach (var worker in workers)

            {

                CreateNotification(worker.Id, title, message, type, relatedEntityId, relatedEntityType, icon);

            }

        }



        /// <summary>

        /// Creates a notification for all admin users

        /// </summary>

        public static void CreateNotificationForAdmins(string title, string message, string type = "Info",

            int? relatedEntityId = null, string relatedEntityType = null, string icon = null)

        {

            var admins = workerRepo.GetAllWorkers()

                .Where(w => w.IsActive && w.Role == UserRole.Admin)

                .ToList();



            foreach (var admin in admins)

            {

                CreateNotification(admin.Id, title, message, type, relatedEntityId, relatedEntityType, icon);

            }

        }



        // ========== Birthday Notifications ==========



        /// <summary>

        /// Checks for upcoming birthdays and creates notifications

        /// Should be called daily

        /// </summary>

        public static void CheckBirthdayNotifications()

        {

            var clientRepo = new ClientRepository();

            var clients = clientRepo.GetAllClients();



            var today = DateTime.Today;

            var tomorrow = today.AddDays(1);

            var nextWeek = today.AddDays(7);



            foreach (var client in clients)

            {

                if (client.DOB == null) continue;



                var nextBirthday = GetNextBirthday(client.DOB.Value);



                // Birthday is today

                if (nextBirthday.Date == today)

                {

                    CreateNotificationForAllUsers(

                        "🎂 Birthday Today!",

                        $"{client.Name}'s birthday is today! Consider sending them a message.",

                        "Birthday",

                        client.Id,

                        "Client",

                        "🎂"

                    );

                }

                // Birthday is tomorrow

                else if (nextBirthday.Date == tomorrow)

                {

                    CreateNotificationForAllUsers(

                        "🎂 Birthday Tomorrow",

                        $"{client.Name}'s birthday is tomorrow ({tomorrow:MMM dd}).",

                        "Birthday",

                        client.Id,

                        "Client",

                        "🎂"

                    );

                }

                // Birthday is within next 7 days

                else if (nextBirthday.Date > tomorrow && nextBirthday.Date <= nextWeek)

                {

                    CreateNotificationForAllUsers(

                        "🎂 Upcoming Birthday",

                        $"{client.Name}'s birthday is coming up on {nextBirthday:MMM dd}.",

                        "Birthday",

                        client.Id,

                        "Client",

                        "🎂"

                    );

                }

            }

        }



        private static DateTime GetNextBirthday(DateTime dob)

        {

            var today = DateTime.Today;

            var thisYearBirthday = new DateTime(today.Year, dob.Month, dob.Day);



            if (thisYearBirthday < today)

            {

                return new DateTime(today.Year + 1, dob.Month, dob.Day);

            }



            return thisYearBirthday;

        }



        // ========== Task Notifications ==========



        /// <summary>

        /// Notifies a user about a new task assignment

        /// </summary>

        public static void NotifyTaskAssigned(int assignedToUserId, string taskTitle, int taskId)

        {

            CreateNotification(

                assignedToUserId,

                "📋 New Task Assigned",

                $"You have been assigned a new task: {taskTitle}",

                "Task",

                taskId,

                "Task",

                "📋"

            );

        }



        /// <summary>

        /// Notifies about overdue tasks

        /// Should be called daily

        /// </summary>

        public static void CheckOverdueTasks()

        {

            var taskRepo = new TodoTaskRepository();

            var tasks = taskRepo.GetAllTasks();



            var today = DateTime.Today;



            foreach (var task in tasks)

            {

                if (task.Status == "Completed" || task.DueDate == null)

                    continue;



                if (task.DueDate.Value.Date < today && task.AssignedToUserId.HasValue)

                {

                    var daysPastDue = (today - task.DueDate.Value.Date).Days;

                    CreateNotification(

                        task.AssignedToUserId.Value,

                        "⚠️ Overdue Task",

                        $"Task '{task.Title}' is {daysPastDue} day(s) overdue!",

                        "Warning",

                        task.Id,

                        "Task",

                        "⚠️"

                    );

                }

            }

        }



        // ========== Import Notifications ==========



        /// <summary>

        /// Notifies about successful CSV import

        /// </summary>

        public static void NotifyImportSuccess(int userId, string fileName, int recordsImported)

        {

            CreateNotification(

                userId,

                "✅ Import Successful",

                $"Successfully imported {recordsImported} records from {fileName}",

                "Success",

                null,

                "Import",

                "✅"

            );

        }



        /// <summary>

        /// Notifies about failed CSV import

        /// </summary>

        public static void NotifyImportError(int userId, string fileName, string errorMessage)

        {

            CreateNotification(

                userId,

                "❌ Import Failed",

                $"Failed to import {fileName}: {errorMessage}",

                "Error",

                null,

                "Import",

                "❌"

            );

        }



        // ========== Worker Notifications ==========



        /// <summary>

        /// Notifies admins when a worker is deactivated

        /// </summary>

        public static void NotifyWorkerDeactivated(string workerUsername, int deactivatedByUserId)

        {

            CreateNotificationForAdmins(

                "⚠️ Worker Deactivated",

                $"User '{workerUsername}' has been deactivated.",

                "Warning",

                null,

                "Worker",

                "⚠️"

            );

        }



        /// <summary>

        /// Notifies admins when a worker is deleted

        /// </summary>

        public static void NotifyWorkerDeleted(string workerUsername, int deletedByUserId)

        {

            CreateNotificationForAdmins(

                "❌ Worker Deleted",

                $"User '{workerUsername}' has been permanently deleted.",

                "Error",

                null,

                "Worker",

                "🗑️"

            );

        }



        /// <summary>

        /// Notifies admins when a new worker is created

        /// </summary>

        public static void NotifyWorkerCreated(string workerUsername, string role)

        {

            CreateNotificationForAdmins(

                "✅ New Worker Created",

                $"New user '{workerUsername}' created with role: {role}",

                "Success",

                null,

                "Worker",

                "👤"

            );

        }



        // ========== Client Notifications ==========



        /// <summary>

        /// Notifies about a new client being added

        /// </summary>

        public static void NotifyNewClient(string clientName, int clientId, int createdByUserId)

        {

            CreateNotificationForAllUsers(

                "✅ New Client Added",

                $"New client '{clientName}' has been added to the system.",

                "Success",

                clientId,

                "Client",

                "👤"

            );

        }



        // ========== Time Clock Notifications ==========



        /// <summary>

        /// Notifies admin when a worker clocks in

        /// </summary>

        public static void NotifyClockIn(string workerUsername, int workerId)

        {

            CreateNotificationForAdmins(

                "⏰ Clock In",

                $"{workerUsername} has clocked in.",

                "Info",

                workerId,

                "TimeEntry",

                "⏰"

            );

        }



        /// <summary>

        /// Notifies admin when a worker clocks out

        /// </summary>

        public static void NotifyClockOut(string workerUsername, int workerId, TimeSpan duration)

        {

            CreateNotificationForAdmins(

                "⏰ Clock Out",

                $"{workerUsername} has clocked out. Duration: {duration.Hours}h {duration.Minutes}m",

                "Info",

                workerId,

                "TimeEntry",

                "⏰"

            );

        }



        // ========== System Notifications ==========



        /// <summary>

        /// Notifies about system backup completion

        /// </summary>

        public static void NotifyBackupComplete(string backupFileName)

        {

            CreateNotificationForAdmins(

                "💾 Backup Complete",

                $"Database backup created: {backupFileName}",

                "Success",

                null,

                "System",

                "💾"

            );

        }



        /// <summary>

        /// Sends a welcome notification to a new user

        /// </summary>

        public static void SendWelcomeNotification(int userId, string username)

        {

            CreateNotification(

                userId,

                "👋 Welcome to REACT CRM!",

                $"Welcome, {username}! We're glad to have you on board. Explore the features and let us know if you need help.",

                "Info",

                null,

                "System",

                "👋"

            );

        }

    }

}