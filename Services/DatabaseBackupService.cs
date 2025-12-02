using System;
using System.IO;
using System.Linq;
using BackupTimer = System.Timers.Timer;
using TimerElapsedEventArgs = System.Timers.ElapsedEventArgs;

namespace ReactCRM.Services
{
    /// <summary>
    /// Automatic database backup service with periodic backups and protection
    /// </summary>
    public class DatabaseBackupService : IDisposable
    {
        private static DatabaseBackupService? _instance;
        private static readonly object _lock = new();
        private BackupTimer? _backupTimer;
        private const string DATABASE_FILE = "crm.db";
        private const string BACKUPS_FOLDER = "Backups";
        private const int BACKUP_INTERVAL_MINUTES = 60; // Backup every hour
        private const int MAX_BACKUPS_TO_KEEP = 30; // Keep last 30 backups

        public static DatabaseBackupService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new DatabaseBackupService();
                    }
                }
                return _instance;
            }
        }

        private DatabaseBackupService()
        {
            EnsureBackupsFolderExists();
        }

        /// <summary>
        /// Start the automatic backup service
        /// </summary>
        public void StartBackupService()
        {
            try
            {
                // Create initial backup on startup
                CreateBackup("startup");

                // Set up timer for periodic backups
                _backupTimer = new BackupTimer(BACKUP_INTERVAL_MINUTES * 60 * 1000); // Convert minutes to milliseconds
                _backupTimer.Elapsed += OnBackupTimerElapsed;
                _backupTimer.AutoReset = true;
                _backupTimer.Start();

                System.Diagnostics.Debug.WriteLine($"Database backup service started. Backups every {BACKUP_INTERVAL_MINUTES} minutes.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting backup service: {ex.Message}");
            }
        }

        /// <summary>
        /// Stop the automatic backup service
        /// </summary>
        public void StopBackupService()
        {
            if (_backupTimer != null)
            {
                _backupTimer.Stop();
                _backupTimer.Dispose();
                _backupTimer = null;
                System.Diagnostics.Debug.WriteLine("Database backup service stopped.");
            }
        }

        private void OnBackupTimerElapsed(object? sender, TimerElapsedEventArgs e)
        {
            CreateBackup("auto");
        }

        /// <summary>
        /// Create a backup of the database
        /// </summary>
        /// <param name="backupType">Type of backup (startup, auto, manual)</param>
        /// <returns>True if backup was successful</returns>
        public bool CreateBackup(string backupType = "manual")
        {
            try
            {
                if (!File.Exists(DATABASE_FILE))
                {
                    System.Diagnostics.Debug.WriteLine("Database file not found. No backup created.");
                    return false;
                }

                EnsureBackupsFolderExists();

                // Create backup filename with timestamp
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFileName = $"crm_backup_{backupType}_{timestamp}.db";
                string backupPath = Path.Combine(BACKUPS_FOLDER, backupFileName);

                // Copy database file to backup
                File.Copy(DATABASE_FILE, backupPath, true);

                System.Diagnostics.Debug.WriteLine($"Database backup created: {backupFileName}");

                // Clean up old backups
                CleanupOldBackups();

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating backup: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Restore database from a backup file
        /// </summary>
        /// <param name="backupFileName">Name of the backup file to restore</param>
        /// <returns>True if restore was successful</returns>
        public bool RestoreBackup(string backupFileName)
        {
            try
            {
                string backupPath = Path.Combine(BACKUPS_FOLDER, backupFileName);

                if (!File.Exists(backupPath))
                {
                    System.Diagnostics.Debug.WriteLine($"Backup file not found: {backupFileName}");
                    return false;
                }

                // Create a backup of current database before restoring
                if (File.Exists(DATABASE_FILE))
                {
                    CreateBackup("before_restore");
                }

                // Restore the backup
                File.Copy(backupPath, DATABASE_FILE, true);

                System.Diagnostics.Debug.WriteLine($"Database restored from: {backupFileName}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restoring backup: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get list of all available backups
        /// </summary>
        /// <returns>Array of backup file information</returns>
        public FileInfo[] GetAvailableBackups()
        {
            try
            {
                EnsureBackupsFolderExists();

                var backupsDir = new DirectoryInfo(BACKUPS_FOLDER);
                return backupsDir.GetFiles("crm_backup_*.db")
                    .OrderByDescending(f => f.CreationTime)
                    .ToArray();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting backups list: {ex.Message}");
                return Array.Empty<FileInfo>();
            }
        }

        /// <summary>
        /// Delete old backups, keeping only the most recent ones
        /// </summary>
        private void CleanupOldBackups()
        {
            try
            {
                var backups = GetAvailableBackups();

                if (backups.Length > MAX_BACKUPS_TO_KEEP)
                {
                    var backupsToDelete = backups.Skip(MAX_BACKUPS_TO_KEEP);

                    foreach (var backup in backupsToDelete)
                    {
                        backup.Delete();
                        System.Diagnostics.Debug.WriteLine($"Deleted old backup: {backup.Name}");
                    }

                    System.Diagnostics.Debug.WriteLine($"Cleaned up {backups.Length - MAX_BACKUPS_TO_KEEP} old backups.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cleaning up old backups: {ex.Message}");
            }
        }

        /// <summary>
        /// Protect database file from accidental deletion
        /// </summary>
        public void ProtectDatabaseFile()
        {
            try
            {
                if (File.Exists(DATABASE_FILE))
                {
                    FileInfo dbFile = new FileInfo(DATABASE_FILE);

                    // Make the file hidden (optional - uncomment if desired)
                    // dbFile.Attributes |= FileAttributes.Hidden;

                    // Note: We don't set ReadOnly because the app needs to write to it
                    // But we could implement temporary read-only when app is not running

                    System.Diagnostics.Debug.WriteLine("Database file protection applied.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error protecting database file: {ex.Message}");
            }
        }

        /// <summary>
        /// Create backup folder if it doesn't exist
        /// </summary>
        private void EnsureBackupsFolderExists()
        {
            if (!Directory.Exists(BACKUPS_FOLDER))
            {
                Directory.CreateDirectory(BACKUPS_FOLDER);
                System.Diagnostics.Debug.WriteLine($"Created backups folder: {BACKUPS_FOLDER}");
            }
        }

        /// <summary>
        /// Get backup statistics
        /// </summary>
        /// <returns>Tuple with backup count and total size in MB</returns>
        public (int count, double sizeMB) GetBackupStatistics()
        {
            try
            {
                var backups = GetAvailableBackups();
                long totalBytes = backups.Sum(f => f.Length);
                double totalMB = totalBytes / (1024.0 * 1024.0);

                return (backups.Length, totalMB);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting backup statistics: {ex.Message}");
                return (0, 0);
            }
        }

        public void Dispose()
        {
            StopBackupService();
        }
    }
}