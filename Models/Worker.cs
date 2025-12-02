using System;

namespace ReactCRM.Models
{
    public enum UserRole
    {
        Admin,
        Employee,
        ViewOnly
    }

    public class Worker
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }

        public Worker()
        {
            IsActive = true;
            CreatedAt = DateTime.Now;
        }

        public bool HasPermission(string permission)
        {
            switch (Role)
            {
                case UserRole.Admin:
                    return true; // Admin has all permissions

                case UserRole.Employee:
                    // Employees can't view logs or create backups
                    return permission != "ViewLogs" &&
                           permission != "CreateBackup" &&
                           permission != "ManageWorkers";

                case UserRole.ViewOnly:
                    // ViewOnly can only read data
                    return permission == "ViewClients" ||
                           permission == "ViewReports";

                default:
                    return false;
            }
        }

        public bool CanEditClients()
        {
            return Role == UserRole.Admin || Role == UserRole.Employee;
        }

        public bool CanDeleteClients()
        {
            return Role == UserRole.Admin;
        }

        public bool CanManageWorkers()
        {
            return Role == UserRole.Admin;
        }

        public bool CanViewLogs()
        {
            return Role == UserRole.Admin;
        }

        public bool CanCreateBackups()
        {
            return Role == UserRole.Admin;
        }

        public bool CanImportData()
        {
            return Role == UserRole.Admin || Role == UserRole.Employee;
        }
    }
}