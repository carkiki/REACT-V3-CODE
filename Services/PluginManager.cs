using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ReactCRM.Database;
using ReactCRM.Models;
using ReactCRM.Plugins;

namespace ReactCRM.Services
{
    /// <summary>
    /// Gestor centralizado de plugins para REACT CRM
    /// Carga automáticamente plugins desde la carpeta /plugins/
    /// Soporta auto-inicio y ejecución manual
    /// </summary>
    public class PluginManager
    {
        private static PluginManager _instance;
        private static readonly object _lock = new object();

        private Dictionary<string, IReactCrmPlugin> loadedPlugins;
        private string pluginDirectory;
        private PluginConfigRepository configRepo;

        private PluginManager()
        {
            loadedPlugins = new Dictionary<string, IReactCrmPlugin>();
            configRepo = new PluginConfigRepository();

            // Ruta de la carpeta plugins (relativa al ejecutable)
            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            pluginDirectory = Path.Combine(appPath, "plugins");

            // Inicializar base de datos de plugins
            PluginDatabaseInitializer.InitializePluginDatabase();
        }

        public static PluginManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new PluginManager();
                        }
                    }
                }
                return _instance;
            }
        }

        public string PluginDirectory => pluginDirectory;

        /// <summary>
        /// Carga todos los plugins desde la carpeta /plugins/
        /// Solo ejecuta auto-inicio si executeAutoStart = true
        /// </summary>
        public void LoadPlugins(bool executeAutoStart = false)
        {
            // Crear carpeta si no existe
            if (!Directory.Exists(pluginDirectory))
            {
                Directory.CreateDirectory(pluginDirectory);
                System.Diagnostics.Debug.WriteLine($"[PluginManager] Created plugins directory: {pluginDirectory}");
            }

            // Buscar todos los archivos .dll
            var dllFiles = Directory.GetFiles(pluginDirectory, "*.dll", SearchOption.AllDirectories);

            System.Diagnostics.Debug.WriteLine($"[PluginManager] Found {dllFiles.Length} DLL files in plugins directory");

            int loadedCount = 0;
            int errorCount = 0;

            foreach (var dllPath in dllFiles)
            {
                try
                {
                    LoadPluginFromDll(dllPath, out bool success);
                    if (success) loadedCount++;
                }
                catch (Exception ex)
                {
                    errorCount++;
                    System.Diagnostics.Debug.WriteLine($"[PluginManager] Error loading {Path.GetFileName(dllPath)}: {ex.Message}");
                }
            }

            System.Diagnostics.Debug.WriteLine($"[PluginManager] Loading complete: {loadedCount} loaded, {errorCount} errors");

            // Ejecutar plugins con auto-inicio
            if (executeAutoStart)
            {
                ExecuteAutoStartPlugins();
            }
        }

        /// <summary>
        /// Carga un plugin desde un archivo DLL específico
        /// </summary>
        public IReactCrmPlugin LoadPluginFromDll(string dllPath, out bool success)
        {
            success = false;

            // Cargar el assembly
            var assembly = Assembly.LoadFrom(dllPath);

            // Buscar tipos que implementen IReactCrmPlugin
            var pluginTypes = assembly.GetTypes()
                .Where(t => typeof(IReactCrmPlugin).IsAssignableFrom(t)
                         && !t.IsInterface
                         && !t.IsAbstract);

            foreach (var pluginType in pluginTypes)
            {
                try
                {
                    // Crear instancia del plugin
                    var plugin = (IReactCrmPlugin)Activator.CreateInstance(pluginType);

                    // Obtener configuración guardada
                    var config = configRepo.GetPluginConfig(plugin.Name);

                    if (config == null)
                    {
                        // Primera vez que se carga este plugin, crear configuración
                        config = new PluginConfig
                        {
                            PluginName = plugin.Name,
                            DllPath = dllPath,
                            IsEnabled = plugin.IsEnabled,
                            AutoStart = false,
                            LoadOrder = 0,
                            CreatedDate = DateTime.Now
                        };
                        configRepo.SavePluginConfig(config);
                    }

                    // Aplicar configuración al plugin
                    plugin.IsEnabled = config.IsEnabled;

                    if (plugin.IsEnabled)
                    {
                        // Inicializar el plugin
                        plugin.Initialize();

                        // Agregar a la lista si no existe
                        if (!loadedPlugins.ContainsKey(plugin.Name))
                        {
                            loadedPlugins[plugin.Name] = plugin;

                            // Actualizar última fecha de carga
                            config.LastLoaded = DateTime.Now;
                            configRepo.SavePluginConfig(config);

                            System.Diagnostics.Debug.WriteLine(
                                $"[PluginManager] ✓ Loaded: {plugin.Name} v{plugin.Version} (AutoStart: {config.AutoStart})"
                            );

                            success = true;
                            return plugin;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[PluginManager] ⊗ Disabled: {plugin.Name}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[PluginManager] Error creating plugin: {ex.Message}");
                }
            }

            return null;
        }

        /// <summary>
        /// Importa un plugin desde cualquier ubicación y lo copia a /plugins/
        /// </summary>
        public bool ImportPlugin(string sourceDllPath, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                if (!File.Exists(sourceDllPath))
                {
                    errorMessage = "DLL file not found.";
                    return false;
                }

                string fileName = Path.GetFileName(sourceDllPath);
                string destPath = Path.Combine(pluginDirectory, fileName);

                // Copiar DLL a carpeta plugins
                File.Copy(sourceDllPath, destPath, overwrite: true);

                // Cargar el plugin
                var plugin = LoadPluginFromDll(destPath, out bool success);

                if (success && plugin != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[PluginManager] ✓ Imported: {plugin.Name} from {sourceDllPath}");
                    return true;
                }
                else
                {
                    errorMessage = "Failed to load plugin from DLL. Make sure it implements IReactCrmPlugin.";
                    return false;
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Ejecuta todos los plugins marcados como auto-inicio
        /// </summary>
        private void ExecuteAutoStartPlugins()
        {
            var autoStartConfigs = configRepo.GetAutoStartPlugins();

            System.Diagnostics.Debug.WriteLine($"[PluginManager] Executing {autoStartConfigs.Count} auto-start plugins...");

            foreach (var config in autoStartConfigs)
            {
                try
                {
                    if (loadedPlugins.ContainsKey(config.PluginName))
                    {
                        var plugin = loadedPlugins[config.PluginName];
                        System.Diagnostics.Debug.WriteLine($"[PluginManager] Auto-executing: {plugin.Name}");
                        plugin.Execute(null); // null parent form for auto-start
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[PluginManager] Error auto-executing {config.PluginName}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Obtiene todos los plugins cargados
        /// </summary>
        public List<IReactCrmPlugin> GetPlugins()
        {
            return loadedPlugins.Values.Where(p => p.IsEnabled).ToList();
        }

        /// <summary>
        /// Obtiene un plugin por su nombre
        /// </summary>
        public IReactCrmPlugin GetPluginByName(string name)
        {
            return loadedPlugins.ContainsKey(name) ? loadedPlugins[name] : null;
        }

        /// <summary>
        /// Ejecuta un plugin específico
        /// </summary>
        public void ExecutePlugin(string pluginName, System.Windows.Forms.Form parentForm)
        {
            if (loadedPlugins.ContainsKey(pluginName))
            {
                var plugin = loadedPlugins[pluginName];
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[PluginManager] Executing: {pluginName}");
                    plugin.Execute(parentForm);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[PluginManager] Error executing {pluginName}: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Habilita o deshabilita un plugin
        /// </summary>
        public void SetPluginEnabled(string pluginName, bool enabled)
        {
            var config = configRepo.GetPluginConfig(pluginName);
            if (config != null)
            {
                config.IsEnabled = enabled;
                configRepo.SavePluginConfig(config);

                if (loadedPlugins.ContainsKey(pluginName))
                {
                    loadedPlugins[pluginName].IsEnabled = enabled;
                }

                System.Diagnostics.Debug.WriteLine($"[PluginManager] {pluginName} {(enabled ? "enabled" : "disabled")}");
            }
        }

        /// <summary>
        /// Configura si un plugin se ejecuta al inicio
        /// </summary>
        public void SetPluginAutoStart(string pluginName, bool autoStart)
        {
            var config = configRepo.GetPluginConfig(pluginName);
            if (config != null)
            {
                config.AutoStart = autoStart;
                configRepo.SavePluginConfig(config);

                System.Diagnostics.Debug.WriteLine($"[PluginManager] {pluginName} auto-start: {autoStart}");
            }
        }

        /// <summary>
        /// Obtiene la configuración de un plugin
        /// </summary>
        public PluginConfig GetPluginConfig(string pluginName)
        {
            return configRepo.GetPluginConfig(pluginName);
        }

        /// <summary>
        /// Desinstala un plugin (elimina DLL y configuración)
        /// </summary>
        public bool UninstallPlugin(string pluginName, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                var config = configRepo.GetPluginConfig(pluginName);
                if (config == null)
                {
                    errorMessage = "Plugin configuration not found.";
                    return false;
                }

                // Cleanup del plugin
                if (loadedPlugins.ContainsKey(pluginName))
                {
                    loadedPlugins[pluginName].Cleanup();
                    loadedPlugins.Remove(pluginName);
                }

                // Eliminar DLL
                if (File.Exists(config.DllPath))
                {
                    File.Delete(config.DllPath);
                }

                // Eliminar configuración
                configRepo.DeletePluginConfig(pluginName);

                System.Diagnostics.Debug.WriteLine($"[PluginManager] Uninstalled: {pluginName}");
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Limpia todos los plugins (llamar al cerrar la aplicación)
        /// </summary>
        public void CleanupPlugins()
        {
            System.Diagnostics.Debug.WriteLine("[PluginManager] Cleaning up plugins...");

            foreach (var plugin in loadedPlugins.Values)
            {
                try
                {
                    plugin.Cleanup();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[PluginManager] Error cleaning up {plugin.Name}: {ex.Message}");
                }
            }

            loadedPlugins.Clear();
        }

        /// <summary>
        /// Obtiene el número de plugins cargados
        /// </summary>
        public int GetPluginCount()
        {
            return loadedPlugins.Count(p => p.Value.IsEnabled);
        }

        /// <summary>
        /// Recarga todos los plugins
        /// </summary>
        public void ReloadPlugins(bool executeAutoStart = false)
        {
            CleanupPlugins();
            LoadPlugins(executeAutoStart);
        }
    }
}