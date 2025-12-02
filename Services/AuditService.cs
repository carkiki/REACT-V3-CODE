using System;
using Microsoft.Data.Sqlite;
using System.Text;
using ReactCRM.Database;
using Newtonsoft.Json;
using File = System.IO.File;
using Directory = System.IO.Directory;
using StreamWriter = System.IO.StreamWriter;

namespace ReactCRM.Services
{
    public static class AuditService
    {
        private const string LOG_FILE = "Logs/activity-log.csv";
        private static readonly object _lock = new object();

        public static void LogAction(string actionType, string entity, int? entityId,
            string description, object metadata = null)
        {
            lock (_lock)
            {
                try
                {
                    // Get current user
                    string user = AuthService.Instance.CurrentUser ?? "System";

                    // Log to database
                    LogToDatabase(user, actionType, entity, entityId, description, metadata);

                    // Log to CSV file
                    LogToCsv(user, actionType, entity, entityId, description);
                }
                catch (Exception ex)
                {
                    // If logging fails, don't crash the application
                    Console.WriteLine($"Logging error: {ex.Message}");
                }
            }
        }

        private static void LogToDatabase(string user, string actionType, string entity,
            int? entityId, string description, object metadata)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();

                string sql = @"
                    INSERT INTO AuditLog (User, ActionType, Entity, EntityId, Description, Metadata)
                    VALUES (@user, @actionType, @entity, @entityId, @description, @metadata)";

                using (var cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@user", user);
                    cmd.Parameters.AddWithValue("@actionType", actionType);
                    cmd.Parameters.AddWithValue("@entity", entity ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@entityId", entityId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@description", description);

                    if (metadata != null)
                    {
                        string metadataJson = JsonConvert.SerializeObject(metadata);
                        cmd.Parameters.AddWithValue("@metadata", metadataJson);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@metadata", DBNull.Value);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void LogToCsv(string user, string actionType, string entity,
            int? entityId, string description)
        {
            // Ensure directory exists
            Directory.CreateDirectory("Logs");

            // Check if file exists to write header
            bool fileExists = File.Exists(LOG_FILE);

            using (StreamWriter sw = new StreamWriter(LOG_FILE, true, Encoding.UTF8))
            {
                if (!fileExists)
                {
                    // Write CSV header
                    sw.WriteLine("Timestamp,User,ActionType,Entity,EntityId,Description");
                }

                // Write log entry
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string entityIdStr = entityId?.ToString() ?? "";

                // Escape fields for CSV
                description = EscapeCsvField(description);

                sw.WriteLine($"{timestamp},{user},{actionType},{entity ?? ""},{entityIdStr},{description}");
            }
        }

        private static string EscapeCsvField(string field)
        {
            if (field == null) return "";

            // If field contains comma, quotes, or newline, wrap in quotes
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                // Escape quotes by doubling them
                field = field.Replace("\"", "\"\"");
                return $"\"{field}\"";
            }

            return field;
        }

        public static void LogImport(string fileName, int recordsProcessed, int recordsCreated,
            int recordsUpdated, int errors)
        {
            var metadata = new
            {
                FileName = fileName,
                RecordsProcessed = recordsProcessed,
                RecordsCreated = recordsCreated,
                RecordsUpdated = recordsUpdated,
                Errors = errors
            };

            LogAction("Import", "CSV", null,
                $"Imported {fileName}: {recordsCreated} created, {recordsUpdated} updated, {errors} errors",
                metadata);
        }

        public static void LogExport(string fileName, int recordsExported)

        {

            var metadata = new

            {

                FileName = fileName,

                RecordsExported = recordsExported

            };



            LogAction("Export", "CSV", null,

                $"Exported {fileName}: {recordsExported} records",

                metadata);

        }

        public static void LogClientAction(string actionType, int clientId, string clientName,
            string details = null)
        {
            LogAction(actionType, "Client", clientId,
                $"{actionType} client: {clientName}" + (details != null ? $" - {details}" : ""));
        }

        public static void LogDocumentAction(string actionType, int clientId, string fileName)
        {
            LogAction(actionType, "Document", clientId,
                $"{actionType} document: {fileName} for client ID {clientId}");
        }

        public static void LogBackup(string backupFile, bool success)
        {
            LogAction("Backup", "System", null,
                success ? $"Backup created: {backupFile}" : $"Backup failed: {backupFile}");
        }

        public static System.Collections.Generic.List<Models.AuditLog> GetAuditLogs(DateTime startDate, DateTime endDate)
        {
            var logs = new System.Collections.Generic.List<Models.AuditLog>();

            using (var conn = Database.DbConnection.GetConnection())
            {
                conn.Open();

                string sql = @"
                    SELECT Id, User, ActionType, Entity, EntityId, Description, Metadata, Timestamp
                    FROM AuditLog
                    WHERE Timestamp >= @startDate AND Timestamp < @endDate
                    ORDER BY Timestamp DESC";

                using (var cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@startDate", startDate);
                    cmd.Parameters.AddWithValue("@endDate", endDate);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var log = new Models.AuditLog
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                User = reader["User"].ToString(),
                                ActionType = reader["ActionType"].ToString(),
                                Entity = reader["Entity"] != System.DBNull.Value ? reader["Entity"].ToString() : null,
                                EntityId = reader["EntityId"] != System.DBNull.Value ? Convert.ToInt32(reader["EntityId"]) : (int?)null,
                                Description = reader["Description"].ToString(),
                                Metadata = reader["Metadata"] != System.DBNull.Value ? reader["Metadata"].ToString() : null,
                                Timestamp = Convert.ToDateTime(reader["Timestamp"])
                            };

                            logs.Add(log);
                        }
                    }
                }
            }

            return logs;
        }
    }
}