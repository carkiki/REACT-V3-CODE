using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using ReactCRM.Models;

namespace ReactCRM.Database
{
    public class NotificationRepository
    {
        public List<Notification> GetUserNotifications(int userId, bool includeRead = false)
        {
            var notifications = new List<Notification>();
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                SELECT * FROM Notifications
                WHERE UserId = @userId AND IsActive = 1";

            if (!includeRead)
            {
                sql += " AND IsRead = 0";
            }

            sql += " ORDER BY CreatedDate DESC LIMIT 50";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@userId", userId);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                notifications.Add(MapReaderToNotification(reader));
            }

            return notifications;
        }

        public int GetUnreadCount(int userId)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                SELECT COUNT(*) FROM Notifications
                WHERE UserId = @userId AND IsRead = 0 AND IsActive = 1";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@userId", userId);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public Notification GetNotificationById(int id)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = "SELECT * FROM Notifications WHERE Id = @id";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return MapReaderToNotification(reader);
            }

            return null;
        }

        public int CreateNotification(Notification notification)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                INSERT INTO Notifications (Title, Message, Type, UserId, IsRead,
                                           CreatedDate, RelatedEntityId, RelatedEntityType,
                                           Icon, IsActive)
                VALUES (@title, @message, @type, @userId, @isRead,
                        @createdDate, @relatedEntityId, @relatedEntityType,
                        @icon, @isActive);
                SELECT last_insert_rowid();";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@title", notification.Title);
            cmd.Parameters.AddWithValue("@message", notification.Message);
            cmd.Parameters.AddWithValue("@type", notification.Type);
            cmd.Parameters.AddWithValue("@userId", notification.UserId);
            cmd.Parameters.AddWithValue("@isRead", notification.IsRead);
            cmd.Parameters.AddWithValue("@createdDate", notification.CreatedDate);
            cmd.Parameters.AddWithValue("@relatedEntityId", notification.RelatedEntityId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@relatedEntityType", notification.RelatedEntityType ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@icon", notification.Icon ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@isActive", notification.IsActive);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void MarkAsRead(int notificationId)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                UPDATE Notifications
                SET IsRead = 1,
                    ReadDate = @readDate
                WHERE Id = @id";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", notificationId);
            cmd.Parameters.AddWithValue("@readDate", DateTime.Now);
            cmd.ExecuteNonQuery();
        }

        public void MarkAllAsRead(int userId)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                UPDATE Notifications
                SET IsRead = 1,
                    ReadDate = @readDate
                WHERE UserId = @userId AND IsRead = 0";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@readDate", DateTime.Now);
            cmd.ExecuteNonQuery();
        }

        public void DeleteNotification(int id)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = "UPDATE Notifications SET IsActive = 0 WHERE Id = @id";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        private Notification MapReaderToNotification(SqliteDataReader reader)
        {
            return new Notification
            {
                Id = reader.GetInt32("Id"),
                Title = reader.GetString("Title"),
                Message = reader.GetString("Message"),
                Type = reader.GetString("Type"),
                UserId = reader.GetInt32("UserId"),
                IsRead = reader.GetBoolean("IsRead"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                ReadDate = reader.IsDBNull("ReadDate") ? null : reader.GetDateTime("ReadDate"),
                RelatedEntityId = reader.IsDBNull("RelatedEntityId") ? null : reader.GetInt32("RelatedEntityId"),
                RelatedEntityType = reader.IsDBNull("RelatedEntityType") ? null : reader.GetString("RelatedEntityType"),
                Icon = reader.IsDBNull("Icon") ? null : reader.GetString("Icon"),
                IsActive = reader.GetBoolean("IsActive")
            };
        }
    }
}
