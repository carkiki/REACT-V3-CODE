using System;
using System.Windows.Forms;

namespace ReactCRM.Plugins
{
    /// <summary>
    /// Interface que deben implementar todos los plugins de REACT CRM
    /// </summary>
    public interface IReactCrmPlugin
    {
        /// <summary>
        /// Nombre del plugin
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Versión del plugin
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Descripción del plugin
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Autor del plugin
        /// </summary>
        string Author { get; }

        /// <summary>
        /// Icono del plugin (emoji o texto)
        /// </summary>
        string Icon { get; }

        /// <summary>
        /// Se llama cuando el plugin se carga por primera vez
        /// </summary>
        void Initialize();

        /// <summary>
        /// Se llama cuando el usuario ejecuta el plugin
        /// </summary>
        /// <param name="parentForm">Formulario padre para mostrar diálogos</param>
        void Execute(Form parentForm);

        /// <summary>
        /// Se llama cuando el CRM se está cerrando
        /// </summary>
        void Cleanup();

        /// <summary>
        /// Indica si el plugin está habilitado
        /// </summary>
        bool IsEnabled { get; set; }
    }
}