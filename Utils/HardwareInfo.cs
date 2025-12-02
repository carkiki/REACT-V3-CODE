using System;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace ReactCRM.Utils
{
    /// <summary>
    /// Utility class to retrieve hardware information for license binding
    /// </summary>
    public static class HardwareInfo
    {
        /// <summary>
        /// Gets a unique hardware identifier based on CPU ID and motherboard serial
        /// </summary>
        public static string GetHardwareId()
        {
            try
            {
                string cpuId = GetCpuId();
                string motherboardId = GetMotherboardId();

                // Combine CPU ID and Motherboard ID
                string combinedId = $"{cpuId}-{motherboardId}";

                // Create a hash for consistent length
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedId));
                    return BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 32);
                }
            }
            catch (Exception ex)
            {
                // Fallback to a machine name-based identifier
                return GetFallbackHardwareId();
            }
        }

        private static string GetCpuId()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
                ManagementObjectCollection collection = searcher.Get();

                foreach (ManagementObject obj in collection)
                {
                    return obj["ProcessorId"]?.ToString() ?? "UNKNOWN";
                }
            }
            catch { }

            return "UNKNOWN_CPU";
        }

        private static string GetMotherboardId()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
                ManagementObjectCollection collection = searcher.Get();

                foreach (ManagementObject obj in collection)
                {
                    return obj["SerialNumber"]?.ToString() ?? "UNKNOWN";
                }
            }
            catch { }

            return "UNKNOWN_MB";
        }

        private static string GetFallbackHardwareId()
        {
            try
            {
                string machineName = Environment.MachineName;
                string userName = Environment.UserName;
                string combined = $"{machineName}-{userName}";

                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                    return BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 32);
                }
            }
            catch
            {
                return "FALLBACK_HARDWARE_ID";
            }
        }
    }
}