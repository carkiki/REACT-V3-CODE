using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ReactCRM.Services
{
    /// <summary>
    /// Debug console for production builds - shows messages in a console window
    /// </summary>
    public static class DebugConsole
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        private static bool consoleVisible = false;
        private static StreamWriter logWriter;
        private static string logFilePath;

        /// <summary>
        /// Shows the debug console window
        /// </summary>
        public static void Show()
        {
            if (!consoleVisible)
            {
                AllocConsole();
                consoleVisible = true;

                // Redirect Console.WriteLine to the console
                Console.OutputEncoding = Encoding.UTF8;
                Console.Title = "REACT CRM - Debug Console";

                // Initialize file logging
                InitializeFileLogging();

                // Write header
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
                Console.WriteLine("║         REACT CRM - Debug Console                        ║");
                Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
                Console.ResetColor();
                Console.WriteLine($"Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"Log file: {logFilePath}");
                Console.WriteLine(new string('─', 60));
                Console.WriteLine();

                WriteToLog($"[CONSOLE] Debug console initialized at {DateTime.Now:HH:mm:ss}");
            }
            else
            {
                // Console already exists, just show it
                IntPtr consoleWindow = GetConsoleWindow();
                if (consoleWindow != IntPtr.Zero)
                {
                    ShowWindow(consoleWindow, SW_SHOW);
                }
            }
        }

        /// <summary>
        /// Hides the debug console window (doesn't close it)
        /// </summary>
        public static void Hide()
        {
            if (consoleVisible)
            {
                IntPtr consoleWindow = GetConsoleWindow();
                if (consoleWindow != IntPtr.Zero)
                {
                    ShowWindow(consoleWindow, SW_HIDE);
                }
            }
        }

        /// <summary>
        /// Closes the debug console window
        /// </summary>
        public static void Close()
        {
            if (consoleVisible)
            {
                WriteToLog($"[CONSOLE] Debug console closed at {DateTime.Now:HH:mm:ss}");

                // Close log file
                if (logWriter != null)
                {
                    logWriter.Close();
                    logWriter.Dispose();
                    logWriter = null;
                }

                FreeConsole();
                consoleVisible = false;
            }
        }

        /// <summary>
        /// Toggles the console visibility
        /// </summary>
        public static void Toggle()
        {
            if (consoleVisible)
            {
                IntPtr consoleWindow = GetConsoleWindow();
                if (consoleWindow != IntPtr.Zero)
                {
                    // Check if currently visible
                    // For simplicity, we'll just toggle between show and hide
                    Hide();
                }
            }
            else
            {
                Show();
            }
        }

        /// <summary>
        /// Writes a message to the console with a timestamp
        /// </summary>
        public static void WriteLine(string message, ConsoleColor color = ConsoleColor.White)
        {
            if (!consoleVisible)
                Show();

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"[{DateTime.Now:HH:mm:ss}] ");
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();

            // Also write to log file
            WriteToLog($"[{DateTime.Now:HH:mm:ss}] {message}");
        }

        /// <summary>
        /// Writes an info message (blue)
        /// </summary>
        public static void Info(string message)
        {
            WriteLine($"[INFO] {message}", ConsoleColor.Cyan);
        }

        /// <summary>
        /// Writes a warning message (yellow)
        /// </summary>
        public static void Warning(string message)
        {
            WriteLine($"[WARN] {message}", ConsoleColor.Yellow);
        }

        /// <summary>
        /// Writes an error message (red)
        /// </summary>
        public static void Error(string message)
        {
            WriteLine($"[ERROR] {message}", ConsoleColor.Red);
        }

        /// <summary>
        /// Writes a success message (green)
        /// </summary>
        public static void Success(string message)
        {
            WriteLine($"[SUCCESS] {message}", ConsoleColor.Green);
        }

        /// <summary>
        /// Writes a debug message (gray)
        /// </summary>
        public static void LogDebug(string message)
        {
            WriteLine($"[DEBUG] {message}", ConsoleColor.DarkGray);
        }

        /// <summary>
        /// Initializes file logging
        /// </summary>
        private static void InitializeFileLogging()
        {
            try
            {
                // Create logs directory if it doesn't exist
                string logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                if (!Directory.Exists(logsDir))
                {
                    Directory.CreateDirectory(logsDir);
                }

                // Create log file with timestamp
                logFilePath = Path.Combine(logsDir, $"debug_{DateTime.Now:yyyyMMdd_HHmmss}.log");

                // Create log writer
                FileStream logFile = new FileStream(logFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
                logWriter = new StreamWriter(logFile, Encoding.UTF8) { AutoFlush = true };

                // Write header to file
                logWriter.WriteLine("╔═══════════════════════════════════════════════════════════╗");
                logWriter.WriteLine("║         REACT CRM - Debug Log                             ║");
                logWriter.WriteLine("╚═══════════════════════════════════════════════════════════╝");
                logWriter.WriteLine($"Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                logWriter.WriteLine(new string('─', 60));
                logWriter.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize file logging: {ex.Message}");
            }
        }

        /// <summary>
        /// Writes to log file
        /// </summary>
        private static void WriteToLog(string message)
        {
            try
            {
                if (logWriter != null)
                {
                    logWriter.WriteLine(message);
                }
            }
            catch
            {
                // Ignore logging errors
            }
        }

        /// <summary>
        /// Gets the path to the current log file
        /// </summary>
        public static string GetLogFilePath()
        {
            return logFilePath;
        }

        /// <summary>
        /// Checks if console is currently visible
        /// </summary>
        public static bool IsVisible()
        {
            return consoleVisible;
        }

        /// <summary>
        /// Clears the console window
        /// </summary>
        public static void Clear()
        {
            if (consoleVisible)
            {
                Console.Clear();
            }
        }
    }
}