using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.Sqlite;
using ReactCRM.Models;

namespace ReactCRM.Database
{
    public class ChatMessageRepository
    {
        /// <summary>
        /// Get messages for a specific channel
        /// </summary>
        public List<ChatMessage> GetChannelMessages(string channel, int limit = 100)
        {
            var messages = new List<ChatMessage>();
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                SELECT m.*, w.Username as SenderName
                FROM ChatMessages m
                LEFT JOIN Workers w ON m.SenderId = w.Id
                WHERE m.Channel = @channel AND m.IsActive = 1
                ORDER BY m.IsPinned DESC, m.SentDate DESC
                LIMIT @limit";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@channel", channel);
            cmd.Parameters.AddWithValue("@limit", limit);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                messages.Add(MapReaderToMessage(reader));
            }

            return messages.OrderBy(m => m.IsPinned ? 0 : 1).ThenBy(m => m.SentDate).ToList();
        }

        /// <summary>
        /// Get recent messages across all channels
        /// </summary>
        public List<ChatMessage> GetRecentMessages(int limit = 50)
        {
            var messages = new List<ChatMessage>();
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                SELECT m.*, w.Username as SenderName
                FROM ChatMessages m
                LEFT JOIN Workers w ON m.SenderId = w.Id
                WHERE m.IsActive = 1
                ORDER BY m.SentDate DESC
                LIMIT @limit";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@limit", limit);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                messages.Add(MapReaderToMessage(reader));
            }

            return messages.OrderBy(m => m.SentDate).ToList();
        }

        /// <summary>
        /// Get messages related to a specific entity (e.g., client)
        /// </summary>
        public List<ChatMessage> GetEntityMessages(string entityType, int entityId)
        {
            var messages = new List<ChatMessage>();
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                SELECT m.*, w.Username as SenderName
                FROM ChatMessages m
                LEFT JOIN Workers w ON m.SenderId = w.Id
                WHERE m.RelatedEntityType = @entityType
                  AND m.RelatedEntityId = @entityId
                  AND m.IsActive = 1
                ORDER BY m.SentDate ASC";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@entityType", entityType);
            cmd.Parameters.AddWithValue("@entityId", entityId);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                messages.Add(MapReaderToMessage(reader));
            }

            return messages;
        }

        /// <summary>
        /// Send a new message
        /// </summary>
        public int SendMessage(ChatMessage message)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                INSERT INTO ChatMessages (SenderId, Message, SentDate, Channel, MessageType,
                                         RelatedEntityId, RelatedEntityType, IsPinned,
                                         AttachmentPath, ReplyToMessageId, IsActive)
                VALUES (@senderId, @message, @sentDate, @channel, @messageType,
                        @relatedEntityId, @relatedEntityType, @isPinned,
                        @attachmentPath, @replyToMessageId, @isActive);
                SELECT last_insert_rowid();";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@senderId", message.SenderId);
            cmd.Parameters.AddWithValue("@message", message.Message);
            cmd.Parameters.AddWithValue("@sentDate", message.SentDate);
            cmd.Parameters.AddWithValue("@channel", message.Channel);
            cmd.Parameters.AddWithValue("@messageType", message.MessageType);
            cmd.Parameters.AddWithValue("@relatedEntityId", message.RelatedEntityId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@relatedEntityType", message.RelatedEntityType ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@isPinned", message.IsPinned);
            cmd.Parameters.AddWithValue("@attachmentPath", message.AttachmentPath ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@replyToMessageId", message.ReplyToMessageId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@isActive", message.IsActive);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        /// <summary>
        /// Edit an existing message
        /// </summary>
        public void EditMessage(int messageId, string newContent)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                UPDATE ChatMessages
                SET Message = @message,
                    IsEdited = 1,
                    EditedDate = @editedDate
                WHERE Id = @id";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", messageId);
            cmd.Parameters.AddWithValue("@message", newContent);
            cmd.Parameters.AddWithValue("@editedDate", DateTime.Now);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Delete a message (soft delete)
        /// </summary>
        public void DeleteMessage(int messageId)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = "UPDATE ChatMessages SET IsActive = 0 WHERE Id = @id";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", messageId);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Pin/unpin a message
        /// </summary>
        public void TogglePin(int messageId)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                UPDATE ChatMessages
                SET IsPinned = NOT IsPinned
                WHERE Id = @id";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", messageId);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Search messages by content
        /// </summary>
        public List<ChatMessage> SearchMessages(string searchTerm, string channel = null)
        {
            var messages = new List<ChatMessage>();
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                SELECT m.*, w.Username as SenderName
                FROM ChatMessages m
                LEFT JOIN Workers w ON m.SenderId = w.Id
                WHERE m.Message LIKE @searchTerm AND m.IsActive = 1";

            if (!string.IsNullOrEmpty(channel))
            {
                sql += " AND m.Channel = @channel";
            }

            sql += " ORDER BY m.SentDate DESC LIMIT 50";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@searchTerm", $"%{searchTerm}%");
            if (!string.IsNullOrEmpty(channel))
            {
                cmd.Parameters.AddWithValue("@channel", channel);
            }

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                messages.Add(MapReaderToMessage(reader));
            }

            return messages;
        }

        /// <summary>
        /// Get count of unread messages for a user (last 24h as example)
        /// </summary>
        public int GetUnreadCount(int userId, string channel = "general")
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                SELECT COUNT(*)
                FROM ChatMessages
                WHERE Channel = @channel
                  AND SenderId != @userId
                  AND SentDate > @since
                  AND IsActive = 1";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@channel", channel);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@since", DateTime.Now.AddHours(-24));

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private ChatMessage MapReaderToMessage(SqliteDataReader reader)
        {
            return new ChatMessage
            {
                Id = reader.GetInt32("Id"),
                SenderId = reader.GetInt32("SenderId"),
                SenderName = reader.IsDBNull("SenderName") ? "Unknown" : reader.GetString("SenderName"),
                Message = reader.GetString("Message"),
                SentDate = reader.GetDateTime("SentDate"),
                Channel = reader.GetString("Channel"),
                MessageType = reader.GetString("MessageType"),
                RelatedEntityId = reader.IsDBNull("RelatedEntityId") ? null : reader.GetInt32("RelatedEntityId"),
                RelatedEntityType = reader.IsDBNull("RelatedEntityType") ? null : reader.GetString("RelatedEntityType"),
                IsPinned = reader.GetBoolean("IsPinned"),
                IsEdited = reader.GetBoolean("IsEdited"),
                EditedDate = reader.IsDBNull("EditedDate") ? null : reader.GetDateTime("EditedDate"),
                AttachmentPath = reader.IsDBNull("AttachmentPath") ? null : reader.GetString("AttachmentPath"),
                ReplyToMessageId = reader.IsDBNull("ReplyToMessageId") ? null : reader.GetInt32("ReplyToMessageId"),
                IsActive = reader.GetBoolean("IsActive")
            };
        }
    }
}
