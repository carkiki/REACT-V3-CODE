using System;
using Microsoft.Data.Sqlite;
using ReactCRM.Database;
using ReactCRM.Utils;

namespace ReactCRM.Services
{
    public class AuthService
    {
        private static AuthService _instance;
        private static readonly object _lock = new object();

        public string CurrentUser { get; private set; }
        public int CurrentUserId { get; private set; }
        public string CurrentRole { get; private set; }
        public bool IsAuthenticated { get; private set; }

        private AuthService() { }

        public static AuthService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new AuthService();
                        }
                    }
                }
                return _instance;
            }
        }

        public bool Login(string username, string password)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();

                string sql = @"
                    SELECT Id, PasswordHash, Salt, Role
                    FROM Workers
                    WHERE Username = @username AND IsActive = 1";

                using (var cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedHash = reader["PasswordHash"].ToString();
                            string salt = reader["Salt"].ToString();

                            // Combine hash and salt in the format expected by VerifyPassword
                            string hashedPasswordWithSalt = $"{storedHash}:{salt}";

                            if (HashUtils.VerifyPassword(password, hashedPasswordWithSalt))
                            {
                                CurrentUser = username;
                                CurrentUserId = Convert.ToInt32(reader["Id"]);
                                CurrentRole = reader["Role"].ToString();
                                IsAuthenticated = true;

                                // Log successful login
                                AuditService.LogAction("Login", "Authentication", null,
                                    $"User {username} logged in successfully");

                                return true;
                            }
                        }
                    }
                }
            }

            // Log failed login attempt
            AuditService.LogAction("LoginFailed", "Authentication", null,
                $"Failed login attempt for user {username}");

            return false;
        }

        public void Logout()
        {
            if (IsAuthenticated)
            {
                AuditService.LogAction("Logout", "Authentication", null,
                    $"User {CurrentUser} logged out");
            }

            CurrentUser = null;
            CurrentUserId = 0;
            CurrentRole = null;
            IsAuthenticated = false;
        }

        public bool HasPermission(string requiredRole)
        {
            if (!IsAuthenticated) return false;

            // Admin has all permissions
            if (CurrentRole == "Admin") return true;

            // Check specific role requirements
            switch (requiredRole)
            {
                case "ViewOnly":
                    return true; // Everyone can view

                case "Employee":
                    return CurrentRole == "Employee" || CurrentRole == "Admin";

                case "Admin":
                    return CurrentRole == "Admin";

                default:
                    return false;
            }
        }

        public string GetCurrentUsername()
        {
            return CurrentUser ?? "Guest";
        }

        public int GetCurrentUserId()
        {
            return CurrentUserId;
        }

        public string GetCurrentUserRole()
        {
            return CurrentRole ?? "None";
        }

        public bool CanViewLogs()
        {
            // Only Admin can view audit logs
            return IsAuthenticated && CurrentRole == "Admin";
        }

        public bool CanCreateBackups()
        {
            // Only Admin can create backups
            return IsAuthenticated && CurrentRole == "Admin";
        }

        public bool CanManageWorkers()
        {
            // Only Admin can manage workers
            return IsAuthenticated && CurrentRole == "Admin";
        }

        public bool CanImportData()
        {
            // Admin and Employee can import data
            return IsAuthenticated && (CurrentRole == "Admin" || CurrentRole == "Employee");
        }

        public bool CanEditClients()
        {
            // Admin and Employee can edit clients and custom fields
            return IsAuthenticated && (CurrentRole == "Admin" || CurrentRole == "Employee");
        }
    }
}