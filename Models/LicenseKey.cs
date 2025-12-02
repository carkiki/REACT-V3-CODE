using System;

namespace ReactCRM.Models
{
    /// <summary>
    /// License key model that matches Firebase structure
    /// </summary>
    public class LicenseKey
    {
        public string CompanyName { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool IsActive { get; set; }
        public string HardwareId { get; set; } = string.Empty;

        public LicenseKey()
        {
            IsActive = true;
        }

        public bool IsExpired()
        {
            return DateTime.Now > ExpirationDate;
        }

        public bool IsValid()
        {
            return IsActive && !IsExpired() && DateTime.Now >= StartDate;
        }

        public int DaysUntilExpiration()
        {
            return (ExpirationDate - DateTime.Now).Days;
        }
    }

    /// <summary>
    /// Local encrypted license storage model
    /// </summary>
    public class LocalLicenseData
    {
        public string CompanyName { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string HardwareId { get; set; } = string.Empty;
        public DateTime LastValidated { get; set; }
        public DateTime CachedExpirationDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}