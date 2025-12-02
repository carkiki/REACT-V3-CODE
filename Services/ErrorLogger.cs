using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ReactCRM.Services
{
    /// <summary>
    /// Centralized error logging service for the application
    /// Provides thread-safe logging with automatic log rotation
    /// </summary>
    public class ErrorLogger
    {
        private static ErrorLogger? _instance;
        private static readonly object _lock = new object();
        private readonly string _logDirectory;
        private readonly string _logFileName;
        private readonly object _fileLock = new object();
        private const long MAX_LOG_SIZE = 10 * 1024 * 1024; // 10MB

        public static ErrorLogger Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ErrorLogger();
                        }
                    }
                }
                return _instance;
            }
        }

        private ErrorLogger()
        {
            // Initialize log directory
            _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            _logFileName = Path.Combine(_logDirectory, "application.log");

            // Create logs directory if it doesn't exist
            try
            {
                if (!Directory.Exists(_logDirectory))
                {
                    Directory.CreateDirectory(_logDirectory);
                }
            }
            catch (Exception ex)
            {
                // If we can't create the log directory, write to debug output
                System.Diagnostics.Debug.WriteLine($"Failed to create log directory: {ex.Message}");
            }
        }

        /// <summary>
        /// Logs an error message with exception details
        /// </summary>
        public void LogError(string message, Exception? exception = null, string? context = null)
        {
            try
            {
                var logEntry = BuildLogEntry("ERROR", message, exception, context);
                WriteToLog(logEntry);
            }
            catch (Exception ex)
            {
                // Fallback to debug output if logging fails
                System.Diagnostics.Debug.WriteLine($"Logging failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Logs a warning message
        /// </summary>
        public void LogWarning(string message, string? context = null)
        {
            try
            {
                var logEntry = BuildLogEntry("WARNING", message, null, context);
                WriteToLog(logEntry);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logging failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Logs an informational message
        /// </summary>
        public void LogInfo(string message, string? context = null)
        {
            try
            {
                var logEntry = BuildLogEntry("INFO", message, null, context);
                WriteToLog(logEntry);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logging failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Logs an error and shows a user-friendly message box
        /// </summary>
        public void LogAndShowError(string userMessage, Exception exception, string? context = null)
        {
            LogError(userMessage, exception, context);

            MessageBox.Show(
                $"{userMessage}\n\n" +
                $"Error: {exception.Message}\n\n" +
                $"Please check the application logs for more details.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }

        /// <summary>
        /// Logs a critical error and shows a detailed message box
        /// </summary>
        public void LogCriticalError(string userMessage, Exception exception, string? context = null)
        {
            LogError(userMessage, exception, context);

            MessageBox.Show(
                $"{userMessage}\n\n" +
                $"Error: {exception.Message}\n\n" +
                $"Stack Trace:\n{exception.StackTrace}\n\n" +
                $"Please contact support if this error persists.",
                "Critical Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }

        /// <summary>
        /// Builds a formatted log entry
        /// </summary>
        private string BuildLogEntry(string level, string message, Exception? exception, string? context)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}");

            if (!string.IsNullOrEmpty(context))
            {
                sb.AppendLine($"Context: {context}");
            }

            if (exception != null)
            {
                sb.AppendLine($"Exception Type: {exception.GetType().FullName}");
                sb.AppendLine($"Exception Message: {exception.Message}");
                sb.AppendLine($"Stack Trace:");
                sb.AppendLine(exception.StackTrace);

                // Log inner exceptions
                var innerEx = exception.InnerException;
                int innerLevel = 1;
                while (innerEx != null)
                {
                    sb.AppendLine($"Inner Exception {innerLevel}:");
                    sb.AppendLine($"  Type: {innerEx.GetType().FullName}");
                    sb.AppendLine($"  Message: {innerEx.Message}");
                    sb.AppendLine($"  Stack Trace: {innerEx.StackTrace}");
                    innerEx = innerEx.InnerException;
                    innerLevel++;
                }
            }

            sb.AppendLine(new string('-', 80));
            return sb.ToString();
        }

        /// <summary>
        /// Writes log entry to file with thread-safety
        /// </summary>
        private void WriteToLog(string logEntry)
        {
            lock (_fileLock)
            {
                try
                {
                    // Check if log rotation is needed
                    if (File.Exists(_logFileName))
                    {
                        var fileInfo = new FileInfo(_logFileName);
                        if (fileInfo.Length > MAX_LOG_SIZE)
                        {
                            RotateLog();
                        }
                    }

                    // Append to log file
                    File.AppendAllText(_logFileName, logEntry);

                    // Also write to debug output for development
                    System.Diagnostics.Debug.Write(logEntry);
                }
                catch (Exception ex)
                {
                    // If file writing fails, at least output to debug
                    System.Diagnostics.Debug.WriteLine($"Failed to write to log file: {ex.Message}");
                    System.Diagnostics.Debug.Write(logEntry);
                }
            }
        }

        /// <summary>
        /// Rotates the log file when it exceeds maximum size
        /// </summary>
        private void RotateLog()
        {
            try
            {
                // Keep the last 5 log files
                for (int i = 4; i >= 1; i--)
                {
                    string oldFile = Path.Combine(_logDirectory, $"application.{i}.log");
                    string newFile = Path.Combine(_logDirectory, $"application.{i + 1}.log");

                    if (File.Exists(newFile))
                    {
                        File.Delete(newFile);
                    }

                    if (File.Exists(oldFile))
                    {
                        File.Move(oldFile, newFile);
                    }
                }

                // Move current log to .1
                string archiveFile = Path.Combine(_logDirectory, "application.1.log");
                if (File.Exists(archiveFile))
                {
                    File.Delete(archiveFile);
                }
                File.Move(_logFileName, archiveFile);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to rotate log: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the path to the current log file
        /// </summary>
        public string GetLogFilePath()
        {
            return _logFileName;
        }

        /// <summary>
        /// Clears all log files
        /// </summary>
        public void ClearLogs()
        {
            lock (_fileLock)
            {
                try
                {
                    if (Directory.Exists(_logDirectory))
                    {
                        foreach (var file in Directory.GetFiles(_logDirectory, "*.log"))
                        {
                            File.Delete(file);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to clear logs: {ex.Message}");
                }
            }
        }
    }
}
