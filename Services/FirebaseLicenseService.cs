using Firebase.Database;
using Firebase.Database.Query;
using ReactCRM.Models;
using ReactCRM.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ReactCRM.Services
{
    /// <summary>
    /// Service to interact with Firebase for license validation
    /// </summary>
    public class FirebaseLicenseService
    {
        private readonly FirebaseClient _firebaseClient;
        private const string FIREBASE_BASE_URL = "https://login-d35c1.firebaseio.com/"; // Update this with your Firebase URL
        private const string LICENSES_PATH = "licenses";

        public FirebaseLicenseService()
        {
            _firebaseClient = new FirebaseClient(FIREBASE_BASE_URL);
        }

        public FirebaseLicenseService(string firebaseUrl)
        {
            _firebaseClient = new FirebaseClient(firebaseUrl);
        }

        /// <summary>
        /// Validates a license key with Firebase
        /// </summary>
        public async Task<LicenseValidationResult> ValidateLicenseAsync(string licenseKey)
        {
            try
            {
                // Query Firebase for the license key
                var licenses = await _firebaseClient
                    .Child(LICENSES_PATH)
                    .OrderBy("Key")
                    .EqualTo(licenseKey)
                    .OnceAsync<LicenseKey>();

                if (licenses == null || !licenses.Any())
                {
                    return new LicenseValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "License key not found"
                    };
                }

                var license = licenses.First().Object;

                // Check if license is active
                if (!license.IsActive)
                {
                    return new LicenseValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "License has been deactivated"
                    };
                }

                // Check if license has started
                if (DateTime.Now < license.StartDate)
                {
                    return new LicenseValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = $"License is not yet active. Start date: {license.StartDate:yyyy-MM-dd}"
                    };
                }

                // Check if license is expired
                if (DateTime.Now > license.ExpirationDate)
                {
                    return new LicenseValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = $"License expired on {license.ExpirationDate:yyyy-MM-dd}"
                    };
                }

                // License is valid
                return new LicenseValidationResult
                {
                    IsValid = true,
                    License = license,
                    ErrorMessage = string.Empty
                };
            }
            catch (Exception ex)
            {
                return new LicenseValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Error validating license: {ex.Message}",
                    IsNetworkError = true
                };
            }
        }

        /// <summary>
        /// Updates the hardware ID for a license in Firebase
        /// </summary>
        public async Task<bool> UpdateHardwareIdAsync(string licenseKey, string hardwareId)
        {
            try
            {
                var licenses = await _firebaseClient
                    .Child(LICENSES_PATH)
                    .OrderBy("Key")
                    .EqualTo(licenseKey)
                    .OnceAsync<LicenseKey>();

                if (licenses == null || !licenses.Any())
                    return false;

                var licenseEntry = licenses.First();
                var license = licenseEntry.Object;
                license.HardwareId = hardwareId;

                await _firebaseClient
                    .Child(LICENSES_PATH)
                    .Child(licenseEntry.Key)
                    .PutAsync(license);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Result of license validation
    /// </summary>
    public class LicenseValidationResult
    {
        public bool IsValid { get; set; }
        public LicenseKey License { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsNetworkError { get; set; }
    }
}