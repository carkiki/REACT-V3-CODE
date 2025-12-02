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
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);



                // Initialize database on first run
                DatabaseInitializer.Initialize();

                // Start automatic database backup service
                var backupService = DatabaseBackupService.Instance;
                backupService.StartBackupService();

                // Start license validation service
                var licenseService = LicenseValidationService.Instance;
                licenseService.StartAsync().GetAwaiter().GetResult();

                // Check if license is valid
                if (!licenseService.IsLicenseValid)
                {
                    // Show license activation form
                    using (var licenseForm = new LicenseActivationForm())
                    {
                        var result = licenseForm.ShowDialog();

                        if (result != DialogResult.OK || !licenseForm.LicenseActivated)
                        {
                            MessageBox.Show(
                                "REACT CRM requires a valid license to run.\n\n" +
                                "Please contact your administrator to obtain a license key.",
                                "License Required",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            );

                            // Cleanup services before exit
                            backupService.StopBackupService();
                            backupService.Dispose();
                            licenseService.Stop();

                            return; // Exit application
                        }
                    }
                }

                // Cleanup when application exits
                Application.ApplicationExit += (s, e) =>
                {
                    backupService.StopBackupService();
                    backupService.Dispose();
                    licenseService.Stop();
                };

                // License is valid, continue to login
                Application.Run(new LoginForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Critical Error: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}