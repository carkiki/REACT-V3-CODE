using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using ReactCRM.Database;

namespace ReactCRM.Services
{
    /// <summary>
    /// Service for generating and managing file paths for client files
    /// </summary>
    public static class FilePathService
    {
        private const string CLIENT_FILES_BASE_DIR = "clientfiles";

        /// <summary>
        /// Get the folder path for a specific client
        /// Format: clientfiles/{clientId}-{sanitizedClientName}
        /// Example: clientfiles/1-John_Doe
        /// </summary>
        public static string GetClientFolderPath(int clientId, string clientName)
        {
            string sanitizedName = SanitizeFileName(clientName);
            return Path.Combine(CLIENT_FILES_BASE_DIR, $"{clientId}-{sanitizedName}");
        }

        /// <summary>
        /// Sanitize a string to be used as a file or folder name
        /// Removes invalid characters and replaces spaces with underscores
        /// </summary>
        public static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "Unknown";

            // Replace spaces with underscores
            fileName = fileName.Trim().Replace(" ", "_");

            // Remove invalid file name characters
            var invalidChars = Path.GetInvalidFileNameChars();
            fileName = string.Join("", fileName.Split(invalidChars));

            // Remove any additional problematic characters
            fileName = Regex.Replace(fileName, @"[<>:""/\\|?*]", "");

            // Limit length to 50 characters
            if (fileName.Length > 50)
                fileName = fileName.Substring(0, 50);

            // Ensure we have something left
            if (string.IsNullOrWhiteSpace(fileName))
                return "Unknown";

            return fileName;
        }

        /// <summary>
        /// Ensure the client folder exists, creating it if necessary
        /// </summary>
        public static void EnsureClientFolderExists(int clientId, string clientName)
        {
            string folderPath = GetClientFolderPath(clientId, clientName);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        /// <summary>
        /// Get the full file path for a client file
        /// </summary>
        public static string GetClientFilePath(int clientId, string clientName, string fileName)
        {
            string folderPath = GetClientFolderPath(clientId, clientName);
            return Path.Combine(folderPath, fileName);
        }

        /// <summary>
        /// Generate a unique timestamped filename to avoid collisions
        /// </summary>
        public static string GenerateUniqueFileName(string originalFileName)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            return $"{timestamp}_{originalFileName}";
        }

        /// <summary>
        /// Check if a folder path is in the old format (just client ID)
        /// </summary>
        public static bool IsOldFormatFolder(string folderPath)
        {
            string folderName = Path.GetFileName(folderPath);
            // Old format is just a number
            return int.TryParse(folderName, out _);
        }

        /// <summary>
        /// Migrate an old-format client folder to the new format
        /// Returns true if migration was successful
        /// </summary>
        public static bool MigrateClientFolder(int clientId, string clientName)
        {
            try
            {
                string oldPath = Path.Combine(CLIENT_FILES_BASE_DIR, clientId.ToString());
                string newPath = GetClientFolderPath(clientId, clientName);

                // If old path doesn't exist, nothing to migrate
                if (!Directory.Exists(oldPath))
                    return false;

                // If new path already exists, skip migration
                if (Directory.Exists(newPath))
                    return false;

                // Rename the directory
                Directory.Move(oldPath, newPath);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error migrating folder for client {clientId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get the client folder path, checking both old and new formats
        /// If old format exists, it will be migrated to the new format
        /// </summary>
        public static string GetOrMigrateClientFolderPath(int clientId, string clientName)
        {
            string newPath = GetClientFolderPath(clientId, clientName);
            string oldPath = Path.Combine(CLIENT_FILES_BASE_DIR, clientId.ToString());

            // If new path exists, use it
            if (Directory.Exists(newPath))
                return newPath;

            // If old path exists, migrate it
            if (Directory.Exists(oldPath))
            {
                if (MigrateClientFolder(clientId, clientName))
                {
                    return newPath;
                }
                // If migration failed, fall back to old path
                return oldPath;
            }

            // Neither exists, return new path
            return newPath;
        }

        /// <summary>
        /// Get the client folder path by client ID only (retrieves name from database)
        /// This is a convenience method for cases where the client name is not readily available
        /// </summary>
        public static string GetOrMigrateClientFolderPathById(int clientId)
        {
            string clientName = GetClientNameById(clientId);
            return GetOrMigrateClientFolderPath(clientId, clientName);
        }

        /// <summary>
        /// Retrieve client name from database by ID
        /// </summary>
        private static string GetClientNameById(int clientId)
        {
            try
            {
                using var connection = DbConnection.GetConnection();
                connection.Open();

                string sql = "SELECT Name FROM Clients WHERE Id = @id";
                using var cmd = new SqliteCommand(sql, connection);
                cmd.Parameters.AddWithValue("@id", clientId);

                var result = cmd.ExecuteScalar();
                return result?.ToString() ?? "Unknown";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting client name for ID {clientId}: {ex.Message}");
                return "Unknown";
            }
        }
    }
}
