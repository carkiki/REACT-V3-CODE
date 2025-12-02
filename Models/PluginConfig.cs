using System;

namespace ReactCRM.Models
{
    /// <summary>
    /// Configuración de un plugin guardada en la base de datos
    /// </summary>
    public class PluginConfig
    {
        public int Id { get; set; }
        public string PluginName { get; set; }
        public string DllPath { get; set; }
        public bool IsEnabled { get; set; }
        public bool AutoStart { get; set; }
        public int LoadOrder { get; set; }
        public DateTime? LastLoaded { get; set; }
        public DateTime CreatedDate { get; set; }

        public PluginConfig()
        {
            PluginName = string.Empty;
            DllPath = string.Empty;
            IsEnabled = true;
            AutoStart = false;
            LoadOrder = 0;
            CreatedDate = DateTime.Now;
        }
    }
}