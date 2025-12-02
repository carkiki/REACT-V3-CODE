using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ReactCRM.Models;
using ReactCRM.Utils;

namespace ReactCRM.Services
{
    /// <summary>
    /// Background service that validates license periodically
    /// </summary>
    public class LicenseValidationService
    {
        private static LicenseValidationService? _instance;
        private static readonly object _lock = new object();

        private System.Threading.Timer? _validationTimer;
        private FirebaseLicenseService _firebaseLicenseService;
        private LocalLicenseData? _currentLicense;
        private bool _isRunning;
        private const int VALIDATION_INTERVAL_HOURS = 24; // Check license every 24 hours
        private const int VALIDATION_INTERVAL_MS = VALIDATION_INTERVAL_HOURS * 60 * 60 * 1000;

        public event EventHandler<LicenseStatusChangedEventArgs>? LicenseStatusChanged;

        public bool IsLicenseValid { get; private set; }
        public LocalLicenseData? CurrentLicense => _currentLicense;

        private LicenseValidationService()
        {
            _firebaseLicenseService = new FirebaseLicenseService();
            IsLicenseValid = false;
        }

        public static LicenseValidationService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new LicenseValidationService();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Starts the background license validation service
        /// </summary>
        public async Task StartAsync()
        {
            if (_isRunning)
                return;

            _isRunning = true;

            // Load local license
            _currentLicense = LocalLicenseStorage.LoadLicense();

            if (_currentLicense != null)
            {
                // Validate hardware ID
                if (!LocalLicenseStorage.ValidateHardwareId(_currentLicense))
                {
                    IsLicenseValid = false;
                    OnLicenseStatusChanged(new LicenseStatusChangedEventArgs
                    {
                        IsValid = false,
                        Message = "Hardware ID mismatch. License is bound to different machine."
                    });
                    return;
                }

                // Check cached expiration date
                if (DateTime.Now > _currentLicense.CachedExpirationDate)
                {
                    IsLicenseValid = false;
                    OnLicenseStatusChanged(new LicenseStatusChangedEventArgs
                    {
                        IsValid = false,
                        Message = "License has expired."
                    });
                    return;
                }

                // Check if license was previously deactivated
                if (!_currentLicense.IsActive)
                {
                    IsLicenseValid = false;
                    OnLicenseStatusChanged(new LicenseStatusChangedEventArgs
                    {
                        IsValid = false,
                        Message = "License has been deactivated."
                    });
                    return;
                }

                // ===== VALIDATE WITH FIREBASE FIRST (synchronously) =====
                await ValidateLicenseAsync();

                // If validation failed (license deactivated, etc.), don't continue
                if (!IsLicenseValid)
                {
                    return;
                }
            }
            else
            {
                IsLicenseValid = false;
                OnLicenseStatusChanged(new LicenseStatusChangedEventArgs
                {
                    IsValid = false,
                    Message = "No license found. Please activate your license."
                });
            }

            // Setup periodic validation timer
            _validationTimer = new System.Threading.Timer(
                async (state) => await ValidateLicenseAsync(),
                null,
                VALIDATION_INTERVAL_MS,
                VALIDATION_INTERVAL_MS
            );
        }

        /// <summary>
        /// Stops the background validation service
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            _validationTimer?.Dispose();
            _validationTimer = null;
        }

        /// <summary>
        /// Validates the license with Firebase
        /// </summary>
        private async Task ValidateLicenseAsync()
        {
            // Create a local reference to avoid race conditions with nullability
            var license = _currentLicense;

            if (license == null)
            {
                IsLicenseValid = false;
                return;
            }

            try
            {
                var result = await _firebaseLicenseService.ValidateLicenseAsync(license.Key);

                if (result.IsValid)
                {
                    IsLicenseValid = true;

                    // Update cached expiration date and active status
                    license.LastValidated = DateTime.Now;
                    license.CachedExpirationDate = result.License.ExpirationDate;
                    license.IsActive = true; // Mark as active
                    LocalLicenseStorage.SaveLicense(license);

                    // Update the field reference just in case
                    _currentLicense = license;

                    OnLicenseStatusChanged(new LicenseStatusChangedEventArgs
                    {
                        IsValid = true,
                        Message = $"License valid until {result.License.ExpirationDate:yyyy-MM-dd}"
                    });
                }
                else
                {
                    // If network error, rely on cached data
                    if (result.IsNetworkError)
                    {
                        // Check if we can still use cached license
                        // MUST check if it was active last time we checked
                        if (DateTime.Now <= license.CachedExpirationDate && license.IsActive)
                        {
                            IsLicenseValid = true;
                            OnLicenseStatusChanged(new LicenseStatusChangedEventArgs
                            {
                                IsValid = true,
                                Message = "Using cached license (network unavailable)"
                            });
                            return;
                        }
                    }
                    else
                    {
                        // Explicitly invalid (revoked, deactivated, etc.)
                        // Update local cache to reflect deactivation
                        license.IsActive = false;
                        LocalLicenseStorage.SaveLicense(license);
                    }

                    IsLicenseValid = false;
                    OnLicenseStatusChanged(new LicenseStatusChangedEventArgs
                    {
                        IsValid = false,
                        Message = result.ErrorMessage
                    });
                }
            }
            catch (Exception ex)
            {
                // On exception, check cached license
                if (DateTime.Now <= license.CachedExpirationDate && license.IsActive)
                {
                    IsLicenseValid = true;
                }
                else
                {
                    IsLicenseValid = false;
                    OnLicenseStatusChanged(new LicenseStatusChangedEventArgs
                    {
                        IsValid = false,
                        Message = $"License validation error: {ex.Message}"
                    });
                }
            }
        }

        /// <summary>
        /// Activates a new license
        /// </summary>
        public async Task<LicenseActivationResult> ActivateLicenseAsync(string licenseKey, string? firebaseUrl = null)
        {
            try
            {
                // Use custom Firebase URL if provided
                var firebaseService = string.IsNullOrEmpty(firebaseUrl)
                    ? _firebaseLicenseService
                    : new FirebaseLicenseService(firebaseUrl);

                // Validate license with Firebase
                var result = await firebaseService.ValidateLicenseAsync(licenseKey);

                if (!result.IsValid)
                {
                    return new LicenseActivationResult
                    {
                        Success = false,
                        Message = result.ErrorMessage
                    };
                }

                // Get hardware ID
                string hardwareId = HardwareInfo.GetHardwareId();

                // Check if license already has a hardware ID
                if (!string.IsNullOrEmpty(result.License.HardwareId) &&
                    result.License.HardwareId != hardwareId)
                {
                    return new LicenseActivationResult
                    {
                        Success = false,
                        Message = "This license is already activated on another machine."
                    };
                }

                // Update hardware ID in Firebase if not set
                if (string.IsNullOrEmpty(result.License.HardwareId))
                {
                    await firebaseService.UpdateHardwareIdAsync(licenseKey, hardwareId);
                }

                // Save license locally
                var localLicense = new LocalLicenseData
                {
                    CompanyName = result.License.CompanyName,
                    Key = licenseKey,
                    HardwareId = hardwareId,
                    LastValidated = DateTime.Now,
                    CachedExpirationDate = result.License.ExpirationDate,
                    IsActive = true
                };

                if (LocalLicenseStorage.SaveLicense(localLicense))
                {
                    _currentLicense = localLicense;
                    IsLicenseValid = true;

                    OnLicenseStatusChanged(new LicenseStatusChangedEventArgs
                    {
                        IsValid = true,
                        Message = "License activated successfully"
                    });

                    return new LicenseActivationResult
                    {
                        Success = true,
                        Message = $"License activated successfully for {result.License.CompanyName}",
                        License = result.License
                    };
                }
                else
                {
                    return new LicenseActivationResult
                    {
                        Success = false,
                        Message = "Failed to save license locally"
                    };
                }
            }
            catch (Exception ex)
            {
                return new LicenseActivationResult
                {
                    Success = false,
                    Message = $"Activation failed: {ex.Message}"
                };
            }
        }

        protected virtual void OnLicenseStatusChanged(LicenseStatusChangedEventArgs e)
        {
            LicenseStatusChanged?.Invoke(this, e);
        }
    }

    public class LicenseStatusChangedEventArgs : EventArgs
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class LicenseActivationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public LicenseKey? License { get; set; }
    }
}