using System;
using ReactCRM.Database;
using File = System.IO.File;
using Directory = System.IO.Directory;
using Path = System.IO.Path;

namespace ReactCRM.Services
{
    public static class BackupService
    {
        public static string CreateBackup()
        {
            try
            {
                // Ensure Backups directory exists
                Directory.CreateDirectory("Backups");

                // Create backup filename with timestamp
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd-HHmmss");
                string backupFile = Path.Combine("Backups", $"backup-{timestamp}.db");

                // Copy database file
                File.Copy("crm.db", backupFile, true);

                // Log the backup
                AuditService.LogBackup(backupFile, true);

                return backupFile;
            }
            catch (Exception ex)
            {
                AuditService.LogBackup("Failed", false);
                throw new Exception($"Backup failed: {ex.Message}", ex);
            }
        }

        public static void RestoreBackup(string backupFile)
        {
            try
            {
                if (!File.Exists(backupFile))
                {
                    throw new FileNotFoundException($"Backup file not found: {backupFile}");
                }

                // Create a safety backup of current database
                string safetyBackup = Path.Combine("Backups", $"safety-{DateTime.Now:yyyy-MM-dd-HHmmss}.db");
                File.Copy("crm.db", safetyBackup, true);

                // Restore the backup
                File.Copy(backupFile, "crm.db", true);

                AuditService.LogAction("RestoreBackup", "System", null,
                    $"Database restored from {backupFile}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Restore failed: {ex.Message}", ex);
            }
        }

        public static string[] GetAvailableBackups()
        {
            if (!Directory.Exists("Backups"))
            {
                return new string[0];
            }

            return Directory.GetFiles("Backups", "backup-*.db");
        }

        public static void DeleteOldBackups(int daysToKeep = 30)
        {
            if (!Directory.Exists("Backups"))
            {
                return;
            }

            DateTime cutoffDate = DateTime.Now.AddDays(-daysToKeep);
            string[] backups = GetAvailableBackups();

            foreach (string backup in backups)
            {
                FileInfo fileInfo = new FileInfo(backup);
                if (fileInfo.CreationTime < cutoffDate)
                {
                    try
                    {
                        File.Delete(backup);
                        AuditService.LogAction("DeleteBackup", "System", null,
                            $"Deleted old backup: {backup}");
                    }
                    catch (Exception)
                    {
                        // Continue if deletion fails
                    }
                }
            }
        }
    }
}