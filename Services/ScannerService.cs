using System;

using System.Collections.Generic;

using System.Drawing;

using System.Drawing.Imaging;

using System.IO;

using System.Linq;

using System.Runtime.InteropServices;

using WIA;



namespace ReactCRM.Services

{

    /// <summary>

    /// Service for scanning documents from network and local scanners

    /// Uses Windows Image Acquisition (WIA) for scanner support

    /// Supports both local USB and network scanners (LAN/WiFi)

    /// </summary>

    public static class ScannerService

    {

        // WIA Property IDs

        private const string WIA_HORIZONTAL_SCAN_RESOLUTION = "6147";

        private const string WIA_VERTICAL_SCAN_RESOLUTION = "6148";

        private const string WIA_HORIZONTAL_SCAN_START = "6149";

        private const string WIA_VERTICAL_SCAN_START = "6150";

        private const string WIA_HORIZONTAL_SCAN_SIZE = "6151";

        private const string WIA_VERTICAL_SCAN_SIZE = "6152";

        private const string WIA_SCAN_COLOR_MODE = "6146";



        /// <summary>

        /// Get list of all available scanners (local and network)

        /// </summary>

        public static List<ScannerDevice> GetAvailableScanners()

        {

            var scanners = new List<ScannerDevice>();



            try

            {

                var deviceManager = new DeviceManager();



                foreach (DeviceInfo deviceInfo in deviceManager.DeviceInfos)

                {

                    // Type 1 = Scanner device

                    if (deviceInfo.Type == WiaDeviceType.ScannerDeviceType)

                    {

                        scanners.Add(new ScannerDevice

                        {

                            DeviceId = deviceInfo.DeviceID,

                            Name = GetProperty(deviceInfo.Properties, "Name"),

                            Manufacturer = GetProperty(deviceInfo.Properties, "Manufacturer"),

                            Description = GetProperty(deviceInfo.Properties, "Description"),

                            IsNetworkScanner = IsNetworkDevice(deviceInfo),

                            Driver = "WIA"

                        });

                    }

                }

            }

            catch (COMException ex)

            {

                System.Diagnostics.Debug.WriteLine($"Error getting scanners: {ex.Message}");

            }

            catch (Exception ex)

            {

                System.Diagnostics.Debug.WriteLine($"Unexpected error: {ex.Message}");

            }



            return scanners;

        }



        /// <summary>

        /// Scan a document from the specified scanner

        /// </summary>

        public static Image ScanDocument(string deviceId = null, ScanSettings settings = null)

        {

            try

            {

                var deviceManager = new DeviceManager();

                Device device = null;



                if (string.IsNullOrEmpty(deviceId))

                {

                    // Show scanner selection dialog

                    var dialog = new CommonDialogClass();

                    device = dialog.ShowSelectDevice(WiaDeviceType.ScannerDeviceType, false, false);

                }

                else

                {

                    // Use specified scanner

                    foreach (DeviceInfo info in deviceManager.DeviceInfos)

                    {

                        if (info.DeviceID == deviceId)

                        {

                            device = info.Connect();

                            break;

                        }

                    }

                }



                if (device == null)

                {

                    throw new Exception("No scanner selected or found.");

                }



                // Apply scan settings if provided

                if (settings != null)

                {

                    ApplyScanSettings(device, settings);

                }



                // Get the first item (scanner bed or feeder)

                Item scannerItem = device.Items[1];



                // Perform the scan

                ImageFile imageFile = (ImageFile)scannerItem.Transfer(FormatID.wiaFormatPNG);



                // Convert WIA ImageFile to System.Drawing.Image

                byte[] imageBytes = (byte[])imageFile.FileData.get_BinaryData();

                using (MemoryStream ms = new MemoryStream(imageBytes))

                {

                    return new Bitmap(ms);

                }

            }

            catch (COMException ex)

            {

                throw new Exception($"Scanner error: {ex.Message}. Make sure the scanner is turned on and connected.", ex);

            }

        }



        /// <summary>

        /// Scan document with UI (shows scanner dialog)

        /// </summary>

        public static Image ScanDocumentWithUI()

        {

            try

            {

                var dialog = new CommonDialogClass();



                // Show acquire image dialog

                ImageFile imageFile = (ImageFile)dialog.ShowAcquireImage(

                    WiaDeviceType.ScannerDeviceType,

                    WiaImageIntent.UnspecifiedIntent,

                    WiaImageBias.MaximizeQuality,

                    FormatID.wiaFormatPNG,

                    false,

                    false,

                    false

                );



                if (imageFile != null)

                {

                    byte[] imageBytes = (byte[])imageFile.FileData.get_BinaryData();

                    using (MemoryStream ms = new MemoryStream(imageBytes))

                    {

                        return new Bitmap(ms);

                    }

                }



                return null;

            }

            catch (COMException ex)

            {

                throw new Exception($"Scanning cancelled or error: {ex.Message}", ex);

            }

        }



        /// <summary>

        /// Scan multiple pages from document feeder (ADF)

        /// </summary>

        public static List<Image> ScanMultiplePages(string deviceId = null, ScanSettings settings = null)

        {

            var images = new List<Image>();



            try

            {

                var deviceManager = new DeviceManager();

                Device device = null;



                if (string.IsNullOrEmpty(deviceId))

                {

                    var dialog = new CommonDialogClass();

                    device = dialog.ShowSelectDevice(WiaDeviceType.ScannerDeviceType, false, false);

                }

                else

                {

                    foreach (DeviceInfo info in deviceManager.DeviceInfos)

                    {

                        if (info.DeviceID == deviceId)

                        {

                            device = info.Connect();

                            break;

                        }

                    }

                }



                if (device == null)

                    throw new Exception("No scanner selected.");



                // Apply settings

                if (settings != null)

                {

                    ApplyScanSettings(device, settings);

                }



                Item scannerItem = device.Items[1];



                // Check if device has ADF (Automatic Document Feeder)

                bool hasADF = false;

                try

                {

                    var documentHandlingSelect = GetPropertyById(scannerItem.Properties, "3088");

                    if (documentHandlingSelect != null)

                    {

                        hasADF = true;

                    }

                }

                catch { }



                if (hasADF)

                {

                    // Scan from feeder until no more pages

                    bool hasMorePages = true;

                    while (hasMorePages)

                    {

                        try

                        {

                            ImageFile imageFile = (ImageFile)scannerItem.Transfer(FormatID.wiaFormatPNG);

                            byte[] imageBytes = (byte[])imageFile.FileData.get_BinaryData();

                            using (MemoryStream ms = new MemoryStream(imageBytes))

                            {

                                images.Add(new Bitmap(ms));

                            }

                        }

                        catch

                        {

                            hasMorePages = false;

                        }

                    }

                }

                else

                {

                    // Single page scan

                    Image singleImage = ScanDocument(deviceId, settings);

                    if (singleImage != null)

                    {

                        images.Add(singleImage);

                    }

                }

            }

            catch (Exception ex)

            {

                throw new Exception($"Error scanning multiple pages: {ex.Message}", ex);

            }



            return images;

        }



        /// <summary>

        /// Save scanned image to file

        /// </summary>

        public static void SaveScannedImage(Image image, string filePath, ImageFormat format = null)

        {

            if (format == null)

            {

                format = ImageFormat.Png;

            }



            // Ensure directory exists

            var directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))

            {

                Directory.CreateDirectory(directory);

            }



            image.Save(filePath, format);

        }



        private static void ApplyScanSettings(Device device, ScanSettings settings)

        {

            try

            {

                Item scannerItem = device.Items[1];



                // Set resolution

                if (settings.ResolutionDPI > 0)

                {

                    SetPropertyById(scannerItem.Properties, WIA_HORIZONTAL_SCAN_RESOLUTION, settings.ResolutionDPI);

                    SetPropertyById(scannerItem.Properties, WIA_VERTICAL_SCAN_RESOLUTION, settings.ResolutionDPI);

                }



                // Set color mode

                if (settings.ColorMode != ScanColorMode.Default)

                {

                    int colorMode = settings.ColorMode == ScanColorMode.Color ? 1 :

                                   settings.ColorMode == ScanColorMode.Grayscale ? 2 : 4;

                    SetPropertyById(scannerItem.Properties, WIA_SCAN_COLOR_MODE, colorMode);

                }



                // Set scan area (if specified)

                if (settings.ScanArea.HasValue)

                {

                    var area = settings.ScanArea.Value;

                    SetPropertyById(scannerItem.Properties, WIA_HORIZONTAL_SCAN_START, area.Left);

                    SetPropertyById(scannerItem.Properties, WIA_VERTICAL_SCAN_START, area.Top);

                    SetPropertyById(scannerItem.Properties, WIA_HORIZONTAL_SCAN_SIZE, area.Width);

                    SetPropertyById(scannerItem.Properties, WIA_VERTICAL_SCAN_SIZE, area.Height);

                }

            }

            catch (Exception ex)

            {

                System.Diagnostics.Debug.WriteLine($"Error applying scan settings: {ex.Message}");

                // Continue with default settings

            }

        }



        private static string GetProperty(IProperties properties, string propertyName)

        {

            try

            {

                foreach (Property prop in properties)

                {

                    if (prop.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))

                    {

                        return prop.get_Value()?.ToString() ?? "";

                    }

                }

            }

            catch { }



            return "";

        }



        private static Property GetPropertyById(IProperties properties, string propertyId)

        {

            try

            {

                foreach (Property prop in properties)

                {

                    if (prop.PropertyID.ToString() == propertyId)

                    {

                        return prop;

                    }

                }

            }

            catch { }



            return null;

        }



        private static void SetPropertyById(IProperties properties, string propertyId, object value)

        {

            try

            {

                Property prop = GetPropertyById(properties, propertyId);

                if (prop != null)

                {

                    prop.set_Value(ref value);

                }

            }

            catch (Exception ex)

            {

                System.Diagnostics.Debug.WriteLine($"Error setting property {propertyId}: {ex.Message}");

            }

        }



        private static bool IsNetworkDevice(DeviceInfo deviceInfo)

        {

            try

            {

                string deviceId = deviceInfo.DeviceID?.ToLower() ?? "";

                string name = GetProperty(deviceInfo.Properties, "Name")?.ToLower() ?? "";



                // Network scanners typically have IP addresses or network identifiers in their device ID

                return deviceId.Contains("ip") || deviceId.Contains("network") ||

                       deviceId.Contains("lan") || deviceId.Contains("wifi") ||

                       name.Contains("network") || name.Contains("wifi");

            }

            catch

            {

                return false;

            }

        }



        /// <summary>

        /// No initialization needed for WIA

        /// </summary>

        public static void Initialize()

        {

            // WIA doesn't require initialization

        }



        /// <summary>

        /// No cleanup needed for WIA

        /// </summary>

        public static void Cleanup()

        {

            // WIA doesn't require cleanup

        }

    }



    /// <summary>

    /// Represents a scanner device

    /// </summary>

    public class ScannerDevice

    {

        public string DeviceId { get; set; }

        public string Name { get; set; }

        public string Manufacturer { get; set; }

        public string Description { get; set; }

        public bool IsNetworkScanner { get; set; }

        public string Driver { get; set; }



        public override string ToString()

        {

            string networkIndicator = IsNetworkScanner ? " (Network)" : "";

            return $"{Name}{networkIndicator}";

        }

    }



    /// <summary>

    /// Settings for scanning

    /// </summary>

    public class ScanSettings

    {

        public int ResolutionDPI { get; set; } = 300;

        public ScanColorMode ColorMode { get; set; } = ScanColorMode.Color;

        public Rectangle? ScanArea { get; set; } = null;

        public bool UseFeeder { get; set; } = false;

    }



    public enum ScanColorMode

    {

        Default = 0,

        Color = 1,

        Grayscale = 2,

        BlackAndWhite = 3

    }

}