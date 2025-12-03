using System;
using System.Windows.Forms;
using ReactCRM.Plugins;
using ReactCRM.Plugins.AdvancedAnalytics.UI;

namespace ReactCRM.Plugins.AdvancedAnalytics
{
    /// <summary>
    /// Advanced Analytics Plugin - Provides comprehensive database analysis,
    /// stock-market style charting, and PDF report generation capabilities.
    /// </summary>
    public class AdvancedAnalyticsPlugin : IReactCrmPlugin
    {
        public string Name => "Advanced Analytics & Reporting";

        public string Version => "1.0.0";

        public string Description =>
            "Herramienta avanzada de análisis de datos con gráficas estilo mercado de valores, " +
            "operaciones matemáticas, algoritmos inteligentes y generación de reportes PDF. " +
            "Permite consultar y visualizar todos los datos del sistema incluyendo campos nativos y personalizados.";

        public string Author => "REACT CRM Team";

        public string Icon => "📊";

        public bool IsEnabled { get; set; } = true;

        private MainAnalyticsForm? _mainForm;

        /// <summary>
        /// Initializes the plugin when it's first loaded.
        /// </summary>
        public void Initialize()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[{Name}] Plugin initialized successfully");
                System.Diagnostics.Debug.WriteLine($"[{Name}] Version: {Version}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error initializing {Name}: {ex.Message}",
                    "Plugin Initialization Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// Executes the plugin and shows the main analytics interface.
        /// </summary>
        /// <param name="parentForm">Parent form for dialog centering</param>
        public void Execute(Form parentForm)
        {
            try
            {
                // If form already exists and is open, bring it to front
                if (_mainForm != null && !_mainForm.IsDisposed)
                {
                    _mainForm.BringToFront();
                    _mainForm.Focus();
                    return;
                }

                // Create and show new analytics form
                _mainForm = new MainAnalyticsForm();

                // Set owner for proper modal behavior
                if (parentForm != null && !parentForm.IsDisposed)
                {
                    _mainForm.Owner = parentForm;
                }

                _mainForm.Show();

                System.Diagnostics.Debug.WriteLine($"[{Name}] Main analytics form opened");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al ejecutar {Name}:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                    "Error de Plugin",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// Cleanup resources when CRM is closing.
        /// </summary>
        public void Cleanup()
        {
            try
            {
                // Close the main form if it's open
                if (_mainForm != null && !_mainForm.IsDisposed)
                {
                    _mainForm.Close();
                    _mainForm.Dispose();
                    _mainForm = null;
                }

                System.Diagnostics.Debug.WriteLine($"[{Name}] Plugin cleaned up successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[{Name}] Error during cleanup: {ex.Message}");
            }
        }
    }
}
