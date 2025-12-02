using System;
using System.Windows.Forms;
using ReactCRM.UI.Login;
using ReactCRM.UI.Forms;
using ReactCRM.Database;
using ReactCRM.Services;

namespace ReactCRM
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var logger = ErrorLogger.Instance;

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                logger.LogInfo("Application starting", "Program.Main");

                // Initialize database on first run
                try
                {
                    DatabaseInitializer.Initialize();
                    logger.LogInfo("Database initialized successfully", "Program.Main");
                }
                catch (Exception dbEx)
                {
                    logger.LogCriticalError(
                        "Failed to initialize database. The application cannot start.",
                        dbEx,
                        "Program.Main"
                    );
                    return;
                }

                // Start automatic database backup service
                DatabaseBackupService? backupService = null;
                try
                {
                    backupService = DatabaseBackupService.Instance;
                    backupService.StartBackupService();
                    logger.LogInfo("Backup service started", "Program.Main");
                }
                catch (Exception backupEx)
                {
                    logger.LogWarning($"Failed to start backup service: {backupEx.Message}", "Program.Main");
                    // Continue without backup service - non-critical
                }

                // Start license validation service
                LicenseValidationService? licenseService = null;
                try
                {
                    licenseService = LicenseValidationService.Instance;
                    licenseService.StartAsync().GetAwaiter().GetResult();
                    logger.LogInfo("License validation service started", "Program.Main");
                }
                catch (Exception licEx)
                {
                    logger.LogError("Failed to start license validation service", licEx, "Program.Main");
                }

                // Check if license is valid
                if (licenseService != null && !licenseService.IsLicenseValid)
                {
                    // Show license activation form
                    using (var licenseForm = new LicenseActivationForm())
                    {
                        var result = licenseForm.ShowDialog();

                        if (result != DialogResult.OK || !licenseForm.LicenseActivated)
                        {
                            logger.LogWarning("License activation cancelled or failed", "Program.Main");

                            MessageBox.Show(
                                "REACT CRM requires a valid license to run.\n\n" +
                                "Please contact your administrator to obtain a license key.",
                                "License Required",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            );

                            // Cleanup services before exit
                            if (backupService != null)
                            {
                                backupService.StopBackupService();
                                backupService.Dispose();
                            }
                            if (licenseService != null)
                            {
                                licenseService.Stop();
                            }

                            return; // Exit application
                        }
                    }
                }

                // Cleanup when application exits
                Application.ApplicationExit += (s, e) =>
                {
                    try
                    {
                        logger.LogInfo("Application shutting down", "Program.ApplicationExit");

                        if (backupService != null)
                        {
                            backupService.StopBackupService();
                            backupService.Dispose();
                            logger.LogInfo("Backup service stopped", "Program.ApplicationExit");
                        }

                        if (licenseService != null)
                        {
                            licenseService.Stop();
                            logger.LogInfo("License service stopped", "Program.ApplicationExit");
                        }
                    }
                    catch (Exception cleanupEx)
                    {
                        logger.LogError("Error during application cleanup", cleanupEx, "Program.ApplicationExit");
                    }
                };

                // License is valid, continue to login
                logger.LogInfo("Starting login form", "Program.Main");
                Application.Run(new LoginForm());
                logger.LogInfo("Application closed normally", "Program.Main");
            }
            catch (Exception ex)
            {
                logger.LogCriticalError(
                    "A critical error occurred and the application must close.",
                    ex,
                    "Program.Main"
                );
            }
        }
    }
}