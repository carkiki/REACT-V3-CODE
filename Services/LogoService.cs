using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ReactCRM.Services
{
    /// <summary>
    /// Service to handle application logo/icon loading
    /// </summary>
    public static class LogoService
    {
        private const string LOGO_PATH = "logo/logo.png";
        private static Icon _cachedIcon = null;
        private static Image _cachedImage = null;

        /// <summary>
        /// Get the application icon
        /// Returns default system icon if logo.png not found
        /// </summary>
        public static Icon GetApplicationIcon()
        {
            if (_cachedIcon != null)
                return _cachedIcon;

            try
            {
                if (File.Exists(LOGO_PATH))
                {
                    using (var bmp = new Bitmap(LOGO_PATH))
                    {
                        _cachedIcon = Icon.FromHandle(bmp.GetHicon());
                        return _cachedIcon;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading logo: {ex.Message}");
            }

            // Return default icon if logo not found
            return SystemIcons.Application;
        }

        /// <summary>
        /// Get the application logo as an Image
        /// Returns null if logo.png not found
        /// </summary>
        public static Image GetApplicationLogo()
        {
            if (_cachedImage != null)
                return _cachedImage;

            try
            {
                if (File.Exists(LOGO_PATH))
                {
                    _cachedImage = Image.FromFile(LOGO_PATH);
                    return _cachedImage;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading logo image: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Check if logo file exists
        /// </summary>
        public static bool LogoExists()
        {
            return File.Exists(LOGO_PATH);
        }

        /// <summary>
        /// Apply logo to a form
        /// </summary>
        public static void ApplyLogoToForm(Form form)
        {
            try
            {
                form.Icon = GetApplicationIcon();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying logo to form: {ex.Message}");
            }
        }

        /// <summary>
        /// Clear cached icon and image (call this if logo is updated)
        /// </summary>
        public static void ClearCache()
        {
            _cachedIcon?.Dispose();
            _cachedImage?.Dispose();
            _cachedIcon = null;
            _cachedImage = null;
        }
    }
}
