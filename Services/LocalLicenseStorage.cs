using System;
using System.IO;
using Newtonsoft.Json;
using ReactCRM.Models;
using ReactCRM.Utils;

namespace ReactCRM.Services
{
    /// <summary>
    /// Handles local encrypted storage of license information
    /// </summary>
    public class LocalLicenseStorage
    {
        private static readonly string LicenseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ReactCRM"
        );
        private static readonly string LicenseFilePath = Path.Combine(LicenseDirectory, ".license.dat");

        /// <summary>
        /// Saves license data to encrypted local file
        /// </summary>
        public static bool SaveLicense(LocalLicenseData licenseData)
        {
            try
            {
                // Ensure directory exists
                if (!Directory.Exists(LicenseDirectory))
                {
                    Directory.CreateDirectory(LicenseDirectory);
                }

                // If file exists, remove attributes to allow overwriting
                if (File.Exists(LicenseFilePath))
                {
                    File.SetAttributes(LicenseFilePath, FileAttributes.Normal);
                }

                // Serialize to JSON
                string jsonData = JsonConvert.SerializeObject(licenseData, Formatting.Indented);

                // Encrypt the JSON data
                string encryptedData = LicenseEncryption.Encrypt(jsonData);

                // Write to file
                File.WriteAllText(LicenseFilePath, encryptedData);

                // Set file as hidden
                File.SetAttributes(LicenseFilePath, FileAttributes.Hidden | FileAttributes.System);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving license: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads and decrypts license data from local file
        /// </summary>
        public static LocalLicenseData LoadLicense()
        {
            try
            {
                if (!File.Exists(LicenseFilePath))
                    return null;

                // Read encrypted data
                string encryptedData = File.ReadAllText(LicenseFilePath);

                // Decrypt the data
                string jsonData = LicenseEncryption.Decrypt(encryptedData);

                // Deserialize from JSON
                LocalLicenseData licenseData = JsonConvert.DeserializeObject<LocalLicenseData>(jsonData);

                return licenseData;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading license: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Checks if a local license file exists
        /// </summary>
        public static bool LicenseFileExists()
        {
            return File.Exists(LicenseFilePath);
        }

        /// <summary>
        /// Deletes the local license file
        /// </summary>
        public static bool DeleteLicense()
        {
            try
            {
                if (File.Exists(LicenseFilePath))
                {
                    File.SetAttributes(LicenseFilePath, FileAttributes.Normal);
                    File.Delete(LicenseFilePath);
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting license: {ex.Message}");
                return false;
            }
        }
        /// <summary>
        /// Validates that the hardware ID matches
        /// </summary>
        public static bool ValidateHardwareId(LocalLicenseData licenseData)
        {
            if (licenseData == null)
                return false;

            string currentHardwareId = HardwareInfo.GetHardwareId();
            return licenseData.HardwareId == currentHardwareId;
        }
    }
}