using System;
using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;
using ReactCRM.Utils;
using File = System.IO.File;
using Directory = System.IO.Directory;
using Path = System.IO.Path;

namespace ReactCRM.Database
{
    public class DatabaseInitializer
    {
        private const string DB_FILE = "crm.db";
        private const string CONNECTION_STRING = $"Data Source={DB_FILE};Mode=ReadWriteCreate;";

        public static void Initialize()
        {
            try
            {
                // Delete corrupted database if it exists
                if (File.Exists(DB_FILE))
                {
                    try
                    {
                        // Try to verify database is valid by opening it and checking required tables
                        using (var testConn = new SqliteConnection(CONNECTION_STRING))
                        {
                            testConn.Open();

                            // Verify Workers table exists and has correct structure
                            using (var cmd = new SqliteCommand("SELECT Id, Username, PasswordHash, Salt, Role FROM Workers LIMIT 1;", testConn))
                            {
                                cmd.ExecuteReader().Close();
                            }

                            // Verify Clients table exists
                            using (var cmd = new SqliteCommand("SELECT Id FROM Clients LIMIT 1;", testConn))
                            {
                                cmd.ExecuteReader().Close();
                            }
                        }
                    }
                    catch (SqliteException ex)
                    {
                        // Database is corrupted or has wrong schema
                        // Create backup before attempting to delete
                        string backupFolder = "Backups";
                        if (!Directory.Exists(backupFolder))
                        {
                            Directory.CreateDirectory(backupFolder);
                        }

                        string backupPath = Path.Combine(backupFolder, $"corrupted_db_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db");

                        try
                        {
                            // Create backup of corrupted database
                            File.Copy(DB_FILE, backupPath, true);

                            // Log the issue
                            string errorLog = Path.Combine(backupFolder, "database_corruption_log.txt");
                            File.AppendAllText(errorLog,
                                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Database corruption detected: {ex.Message}\n" +
                                $"Backup created at: {backupPath}\n\n");
                        }
                        catch (Exception backupEx)
                        {
                            // If backup fails, log but continue
                            System.Diagnostics.Debug.WriteLine($"Failed to backup corrupted database: {backupEx.Message}");
                        }

                        // Delete corrupted database
                        File.Delete(DB_FILE);
                    }
                }

                // Create database file if it doesn't exist
                if (!File.Exists(DB_FILE))
                {
                    // Microsoft.Data.Sqlite creates the file automatically when you open a connection
                    using (var connection = new SqliteConnection(CONNECTION_STRING))
                    {
                        connection.Open();
                    }
                }

                // Always ensure tables exist (CREATE IF NOT EXISTS is safe to run)
                CreateTables();
                CreateAdminUser();
                CreateDirectories();
            }
            catch (SqliteException sqlEx)
            {
                // If SQLite error occurs, try deleting database and retry once
                System.Diagnostics.Debug.WriteLine($"SQLite error during initialization: {sqlEx.Message}");
                if (File.Exists(DB_FILE))
                {
                    File.Delete(DB_FILE);
                    System.Diagnostics.Debug.WriteLine("Deleted corrupted database, retrying...");

                    // Retry once
                    try
                    {
                        using (var connection = new SqliteConnection(CONNECTION_STRING))
                        {
                            connection.Open();
                        }
                        CreateTables();
                        CreateAdminUser();
                        CreateDirectories();
                    }
                    catch (Exception retryEx)
                    {
                        throw new Exception($"Failed to initialize database after retry: {retryEx.Message}", retryEx);
                    }
                }
                else
                {
                    throw new Exception($"Failed to initialize database: {sqlEx.Message}", sqlEx);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to initialize database: {ex.Message}", ex);
            }
        }

        private static void CreateTables()
        {
            using var connection = new SqliteConnection(CONNECTION_STRING);
            connection.Open();

            // Workers table
            string createWorkersTable = @"
                CREATE TABLE IF NOT EXISTS Workers (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    PasswordHash TEXT NOT NULL,
                    Salt TEXT NOT NULL,
                    Role TEXT NOT NULL CHECK(Role IN ('Admin', 'Employee', 'ViewOnly')),
                    IsActive BOOLEAN DEFAULT 1,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    LastLogin DATETIME
                );";

            // Clients table with JSON support
            string createClientsTable = @"
                CREATE TABLE IF NOT EXISTS Clients (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SSN TEXT NOT NULL UNIQUE,
                    Name TEXT NOT NULL,
                    DOB DATE,
                    Phone TEXT,
                    Email TEXT,
                    ExtraData TEXT DEFAULT '{}',
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    LastUpdated DATETIME DEFAULT CURRENT_TIMESTAMP,
                    CreatedBy INTEGER,
                    UpdatedBy INTEGER,
                    FOREIGN KEY (CreatedBy) REFERENCES Workers(Id),
                    FOREIGN KEY (UpdatedBy) REFERENCES Workers(Id)
                );";
            // PDF Templates table (for storing reusable PDF templates)

            string createPdfTemplatesTable = @"

                CREATE TABLE IF NOT EXISTS PdfTemplates (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Description TEXT,
                    Category TEXT,
                    FileName TEXT NOT NULL,
                    FilePath TEXT NOT NULL,
                    FileSize INTEGER,
                    FieldMappings TEXT DEFAULT '{}',
                    IsActive BOOLEAN DEFAULT 1,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    CreatedBy INTEGER,
                    FOREIGN KEY (CreatedBy) REFERENCES Workers(Id)
                );";

            // Custom Fields table
            string createCustomFieldsTable = @"
                CREATE TABLE IF NOT EXISTS CustomFields (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FieldName TEXT NOT NULL UNIQUE,
                    Label TEXT NOT NULL,
                    FieldType TEXT NOT NULL CHECK(FieldType IN ('text', 'number', 'date', 'dropdown', 'checkbox')),
                    Options TEXT,
                    IsRequired BOOLEAN DEFAULT 0,
                    DefaultValue TEXT,
                    IsActive BOOLEAN DEFAULT 1,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    CreatedBy INTEGER,
                    FOREIGN KEY (CreatedBy) REFERENCES Workers(Id)
                );";

            // Audit Log table
            string createAuditLogTable = @"
                CREATE TABLE IF NOT EXISTS AuditLog (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
                    User TEXT NOT NULL,
                    UserId INTEGER,
                    ActionType TEXT NOT NULL,
                    Entity TEXT,
                    EntityId INTEGER,
                    Description TEXT NOT NULL,
                    Metadata TEXT,
                    IpAddress TEXT,
                    FOREIGN KEY (UserId) REFERENCES Workers(Id)
                );";

            // Client Files table (for tracking client file attachments)
            string createClientFilesTable = @"
                CREATE TABLE IF NOT EXISTS ClientFiles (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ClientId INTEGER NOT NULL,
                    FileName TEXT NOT NULL,
                    FilePath TEXT NOT NULL,
                    FileSize INTEGER,
                    Description TEXT,
                    UploadedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UploadedBy INTEGER,
                    FOREIGN KEY (ClientId) REFERENCES Clients(Id) ON DELETE CASCADE,
                    FOREIGN KEY (UploadedBy) REFERENCES Workers(Id)
                );";

            // Import History table
            string createImportHistoryTable = @"
                CREATE TABLE IF NOT EXISTS ImportHistory (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ImportDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FileName TEXT NOT NULL,
                    RecordsTotal INTEGER,
                    RecordsSuccess INTEGER,
                    RecordsFailed INTEGER,
                    MappingConfig TEXT,
                    ImportedBy INTEGER,
                    Status TEXT,
                    ErrorLog TEXT,
                    FOREIGN KEY (ImportedBy) REFERENCES Workers(Id)
                );";

            // Field History table (for tracking changes)
            string createFieldHistoryTable = @"
                CREATE TABLE IF NOT EXISTS FieldHistory (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ClientId INTEGER NOT NULL,
                    FieldName TEXT NOT NULL,
                    OldValue TEXT,
                    NewValue TEXT,
                    ChangedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    ChangedBy INTEGER,
                    ChangeSource TEXT,
                    FOREIGN KEY (ClientId) REFERENCES Clients(Id) ON DELETE CASCADE,
                    FOREIGN KEY (ChangedBy) REFERENCES Workers(Id)
                );";

            // Menu Configuration table (for user-specific menu order and visibility)
            string createMenuConfigTable = @"
                CREATE TABLE IF NOT EXISTS MenuConfiguration (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER NOT NULL,
                    MenuItemId INTEGER NOT NULL,
                    MenuItem TEXT NOT NULL,
                    DisplayOrder INTEGER NOT NULL,
                    IsVisible BOOLEAN DEFAULT 1,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (UserId) REFERENCES Workers(Id) ON DELETE CASCADE,
                    UNIQUE(UserId, MenuItemId)
                );";

            // Document Types table (for dynamic document type management in file attachments)
            string createDocumentTypesTable = @"
                CREATE TABLE IF NOT EXISTS DocumentTypes (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TypeName TEXT NOT NULL UNIQUE,
                    Description TEXT,
                    FileExtensions TEXT,
                    IsActive BOOLEAN DEFAULT 1,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    CreatedBy INTEGER,
                    FOREIGN KEY (CreatedBy) REFERENCES Workers(Id)
                );";

            // TodoTasks table (for task management - team and client-specific)
            string createTodoTasksTable = @"
                CREATE TABLE IF NOT EXISTS TodoTasks (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Description TEXT,
                    Priority INTEGER DEFAULT 2,
                    Status TEXT DEFAULT 'Pending',
                    DueDate DATETIME,
                    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    CompletedDate DATETIME,
                    CreatedByUserId INTEGER NOT NULL,
                    AssignedToUserId INTEGER,
                    ClientId INTEGER,
                    Category TEXT,
                    Notes TEXT,
                    IsActive BOOLEAN DEFAULT 1,
                    FOREIGN KEY (CreatedByUserId) REFERENCES Workers(Id),
                    FOREIGN KEY (AssignedToUserId) REFERENCES Workers(Id),
                    FOREIGN KEY (ClientId) REFERENCES Clients(Id) ON DELETE CASCADE
                );";

            // Notifications table (for system notifications and alerts)
            string createNotificationsTable = @"
                CREATE TABLE IF NOT EXISTS Notifications (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Message TEXT NOT NULL,
                    Type TEXT DEFAULT 'Info',
                    UserId INTEGER NOT NULL,
                    IsRead BOOLEAN DEFAULT 0,
                    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    ReadDate DATETIME,
                    RelatedEntityId INTEGER,
                    RelatedEntityType TEXT,
                    Icon TEXT,
                    IsActive BOOLEAN DEFAULT 1,
                    FOREIGN KEY (UserId) REFERENCES Workers(Id) ON DELETE CASCADE
                );";

            // TimeEntries table (for employee time tracking)
            string createTimeEntriesTable = @"
                CREATE TABLE IF NOT EXISTS TimeEntries (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    WorkerId INTEGER NOT NULL,
                    Date DATE NOT NULL,
                    ClockIn DATETIME NOT NULL,
                    ClockOut DATETIME,
                    BreakMinutes INTEGER DEFAULT 0,
                    EntryType TEXT DEFAULT 'Regular',
                    Notes TEXT,
                    Location TEXT,
                    IsApproved BOOLEAN DEFAULT 0,
                    ApprovedByUserId INTEGER,
                    ApprovedDate DATETIME,
                    IsActive BOOLEAN DEFAULT 1,
                    FOREIGN KEY (WorkerId) REFERENCES Workers(Id) ON DELETE CASCADE,
                    FOREIGN KEY (ApprovedByUserId) REFERENCES Workers(Id)
                );";

            // ChatMessages table (for team chat system)
            string createChatMessagesTable = @"
                CREATE TABLE IF NOT EXISTS ChatMessages (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SenderId INTEGER NOT NULL,
                    Message TEXT NOT NULL,
                    SentDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    Channel TEXT DEFAULT 'general',
                    MessageType TEXT DEFAULT 'Text',
                    RelatedEntityId INTEGER,
                    RelatedEntityType TEXT,
                    IsPinned BOOLEAN DEFAULT 0,
                    IsEdited BOOLEAN DEFAULT 0,
                    EditedDate DATETIME,
                    AttachmentPath TEXT,
                    ReplyToMessageId INTEGER,
                    IsActive BOOLEAN DEFAULT 1,
                    FOREIGN KEY (SenderId) REFERENCES Workers(Id) ON DELETE CASCADE,
                    FOREIGN KEY (ReplyToMessageId) REFERENCES ChatMessages(Id)
                );";

            // ClientTabs table (for organizing clients into tabs/groups)
            string createClientTabsTable = @"
                CREATE TABLE IF NOT EXISTS ClientTabs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    DisplayOrder INTEGER NOT NULL DEFAULT 0,
                    IsDefault BOOLEAN DEFAULT 0,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    CreatedBy INTEGER,
                    FOREIGN KEY (CreatedBy) REFERENCES Workers(Id)
                );";

            // Execute all CREATE TABLE statements
            ExecuteNonQuery(connection, createWorkersTable);
            ExecuteNonQuery(connection, createClientTabsTable);
            ExecuteNonQuery(connection, createClientsTable);
            ExecuteNonQuery(connection, createCustomFieldsTable);
            ExecuteNonQuery(connection, createAuditLogTable);
            ExecuteNonQuery(connection, createClientFilesTable);
            ExecuteNonQuery(connection, createImportHistoryTable);
            ExecuteNonQuery(connection, createFieldHistoryTable);
            ExecuteNonQuery(connection, createMenuConfigTable);
            ExecuteNonQuery(connection, createDocumentTypesTable);
            ExecuteNonQuery(connection, createPdfTemplatesTable);
            ExecuteNonQuery(connection, createTodoTasksTable);
            ExecuteNonQuery(connection, createNotificationsTable);
            ExecuteNonQuery(connection, createTimeEntriesTable);
            ExecuteNonQuery(connection, createChatMessagesTable);

            // Create indices for better performance
            CreateIndices(connection);
            AddUploadLinkColumns(connection);
            InitializeDefaultTab(connection);
        }

        private static void CreateIndices(SqliteConnection connection)
        {
            string[] indices = {
                "CREATE INDEX IF NOT EXISTS idx_clients_ssn ON Clients(SSN);",
                "CREATE INDEX IF NOT EXISTS idx_clients_name ON Clients(Name);",
                "CREATE INDEX IF NOT EXISTS idx_clients_email ON Clients(Email);",
                "CREATE INDEX IF NOT EXISTS idx_auditlog_timestamp ON AuditLog(Timestamp DESC);",
                "CREATE INDEX IF NOT EXISTS idx_auditlog_user ON AuditLog(UserId);",
                "CREATE INDEX IF NOT EXISTS idx_clientfiles_client ON ClientFiles(ClientId);",
                "CREATE INDEX IF NOT EXISTS idx_fieldhistory_client ON FieldHistory(ClientId);",
                "CREATE INDEX IF NOT EXISTS idx_workers_username ON Workers(Username);",
                "CREATE INDEX IF NOT EXISTS idx_menuconfig_user ON MenuConfiguration(UserId);",
                "CREATE INDEX IF NOT EXISTS idx_doctypes_active ON DocumentTypes(IsActive);",
                "CREATE INDEX IF NOT EXISTS idx_pdftemplates_active ON PdfTemplates(IsActive);",
                "CREATE INDEX IF NOT EXISTS idx_pdftemplates_category ON PdfTemplates(Category);",
                "CREATE INDEX IF NOT EXISTS idx_todotasks_status ON TodoTasks(Status);",
                "CREATE INDEX IF NOT EXISTS idx_todotasks_client ON TodoTasks(ClientId);",
                "CREATE INDEX IF NOT EXISTS idx_todotasks_assigned ON TodoTasks(AssignedToUserId);",
                "CREATE INDEX IF NOT EXISTS idx_todotasks_duedate ON TodoTasks(DueDate);",
                "CREATE INDEX IF NOT EXISTS idx_notifications_user ON Notifications(UserId);",
                "CREATE INDEX IF NOT EXISTS idx_notifications_isread ON Notifications(IsRead);",
                "CREATE INDEX IF NOT EXISTS idx_timeentries_worker ON TimeEntries(WorkerId);",
                "CREATE INDEX IF NOT EXISTS idx_timeentries_date ON TimeEntries(Date);",
                "CREATE INDEX IF NOT EXISTS idx_chatmessages_channel ON ChatMessages(Channel);",
                "CREATE INDEX IF NOT EXISTS idx_chatmessages_sender ON ChatMessages(SenderId);",
                "CREATE INDEX IF NOT EXISTS idx_chatmessages_date ON ChatMessages(SentDate DESC);",
                "CREATE INDEX IF NOT EXISTS idx_chatmessages_entity ON ChatMessages(RelatedEntityType, RelatedEntityId);",
                "CREATE INDEX IF NOT EXISTS idx_clients_tabid ON Clients(TabId);",
                "CREATE INDEX IF NOT EXISTS idx_clienttabs_order ON ClientTabs(DisplayOrder);"
            };

            foreach (var index in indices)
            {
                ExecuteNonQuery(connection, index);
            }
        }

        private static void CreateAdminUser()
        {
            using var connection = new SqliteConnection(CONNECTION_STRING);
            connection.Open();

            // Check if admin exists
            using var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM Workers WHERE Username = 'admin'", connection);
            var count = Convert.ToInt32(checkCmd.ExecuteScalar());

            if (count == 0)
            {
                // Generate salt and hash password using HashUtils
                var salt = HashUtils.GenerateSalt();
                var passwordHash = HashUtils.HashPassword("admin123", salt);

                string insertAdmin = @"
                    INSERT INTO Workers (Username, PasswordHash, Salt, Role, IsActive)
                    VALUES ('admin', @passwordHash, @salt, 'Admin', 1);";

                using var cmd = new SqliteCommand(insertAdmin, connection);
                cmd.Parameters.AddWithValue("@passwordHash", passwordHash);
                cmd.Parameters.AddWithValue("@salt", salt);
                cmd.ExecuteNonQuery();

                // Log admin creation
                LogAdminCreation(connection);
            }
        }

        private static void LogAdminCreation(SqliteConnection connection)
        {
            string logEntry = @"
                INSERT INTO AuditLog (User, UserId, ActionType, Entity, Description, Metadata)
                VALUES ('System', NULL, 'CREATE', 'Worker', 'Admin user created during initialization', '{}');";

            ExecuteNonQuery(connection, logEntry);
        }

        private static void CreateDirectories()
        {
            string[] directories = {
                "ClientFiles",
                "Logs",
                "Backups",
                "Temp",
                "Reports",
                "Templates"
            };

            foreach (var dir in directories)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }

            // Create initial log file
            string logFile = Path.Combine("Logs", "activity-log.csv");
            if (!File.Exists(logFile))
            {
                File.WriteAllText(logFile, "Timestamp,User,Action,Entity,EntityId,Description,Metadata\n");
            }
        }

        private static void VerifyDatabaseStructure()
        {
            using var connection = new SqliteConnection(CONNECTION_STRING);
            connection.Open();

            // Verify all required tables exist
            string[] requiredTables = {
                "Workers", "Clients", "CustomFields", "AuditLog",
                "ClientFiles", "ImportHistory", "FieldHistory", "MenuConfiguration", "DocumentTypes", "PdfTemplates",
                "TodoTasks", "Notifications", "TimeEntries", "ChatMessages"
            };

            foreach (var table in requiredTables)
            {
                using var cmd = new SqliteCommand(
                    $"SELECT name FROM sqlite_master WHERE type='table' AND name='{table}';",
                    connection);

                var result = cmd.ExecuteScalar();
                if (result == null)
                {
                    throw new Exception($"Required table '{table}' is missing from database");
                }
            }

            // Create default document types if none exist
            CreateDefaultDocumentTypes(connection);
        }

        private static void CreateDefaultDocumentTypes(SqliteConnection connection)
        {
            // Check if document types already exist
            using var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM DocumentTypes WHERE IsActive = 1", connection);
            var count = Convert.ToInt32(checkCmd.ExecuteScalar() ?? 0);

            if (count == 0)
            {
                var defaultTypes = new[]
                {
                    ("Invoice", "Financial document - billing statement", ".pdf,.doc,.docx"),
                    ("Receipt", "Purchase or payment receipt", ".pdf,.jpg,.png"),
                    ("Identification", "ID document - passport, driver license, etc", ".pdf,.jpg,.png"),
                    ("Contract", "Legal agreement document", ".pdf,.doc,.docx"),
                    ("Report", "Business or analytical report", ".pdf,.doc,.docx"),
                    ("Passport", "Travel document", ".pdf,.jpg,.png"),
                    ("Tax Document", "Tax return or tax-related document", ".pdf,.doc,.xlsx"),
                    ("Certificate", "Certificate of completion or certification", ".pdf,.doc,.jpg,.png"),
                    ("License", "Professional or government license", ".pdf,.doc,.jpg,.png"),
                    ("Other", "Other document type", ".*")
                };

                foreach (var (typeName, description, extensions) in defaultTypes)
                {
                    string insertSql = @"
                        INSERT INTO DocumentTypes (TypeName, Description, FileExtensions, IsActive, CreatedBy)
                        VALUES (@typeName, @description, @extensions, 1, NULL)";

                    using var insertCmd = new SqliteCommand(insertSql, connection);
                    insertCmd.Parameters.AddWithValue("@typeName", typeName);
                    insertCmd.Parameters.AddWithValue("@description", description);
                    insertCmd.Parameters.AddWithValue("@extensions", extensions);
                    insertCmd.ExecuteNonQuery();
                }
            }
        }

        private static void ExecuteNonQuery(SqliteConnection connection, string sql)
        {
            using var cmd = new SqliteCommand(sql, connection);
            cmd.ExecuteNonQuery();
        }
        /// <summary>

        /// Adds upload link columns to Clients and ClientFiles tables (migration)

        /// </summary>

        private static void AddUploadLinkColumns(SqliteConnection connection)

        {

            // Check if columns exist before adding

            var columnsToAdd = new[]

            {

                ("Clients", "UploadToken", "TEXT"),

                ("Clients", "UploadLinkExpires", "DATETIME"),

                 ("Clients", "DropboxRequestId", "TEXT"),

                ("Clients", "DropboxUploadUrl", "TEXT"),

                ("Clients", "DropboxFolder", "TEXT"),

                ("Clients", "UploadLinkCreated", "DATETIME"),

                ("Clients", "Notes", "TEXT"),

                ("Clients", "TabId", "INTEGER"),

                ("Clients", "IsActive", "BOOLEAN DEFAULT 1"),

                ("ClientFiles", "MimeType", "TEXT"),

                ("ClientFiles", "UploadcareUuid", "TEXT"),

                ("ClientFiles", "DropboxPath", "TEXT")

            };



            foreach (var (table, column, type) in columnsToAdd)

            {

                try

                {

                    // Check if column exists

                    using var checkCmd = new SqliteCommand(

                        $"SELECT COUNT(*) FROM pragma_table_info('{table}') WHERE name = '{column}'",

                        connection);

                    var exists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;



                    if (!exists)

                    {

                        ExecuteNonQuery(connection, $"ALTER TABLE {table} ADD COLUMN {column} {type}");

                        System.Diagnostics.Debug.WriteLine($"Added column {table}.{column}");

                    }

                }

                catch (Exception ex)

                {

                    System.Diagnostics.Debug.WriteLine($"Error adding column {table}.{column}: {ex.Message}");

                }

            }



            // Create index for upload token

            try

            {

                ExecuteNonQuery(connection,

                    "CREATE INDEX IF NOT EXISTS idx_clients_upload_token ON Clients(UploadToken)");

                ExecuteNonQuery(connection,

                    "CREATE INDEX IF NOT EXISTS idx_clientfiles_uploadcare ON ClientFiles(UploadcareUuid)");

            }

            catch (Exception ex)

            {

                System.Diagnostics.Debug.WriteLine($"Error creating indices: {ex.Message}");

            }

        }

        private static void InitializeDefaultTab(SqliteConnection connection)
        {
            try
            {
                // Check if default tab exists
                using var checkCmd = new SqliteCommand(
                    "SELECT COUNT(*) FROM ClientTabs WHERE IsDefault = 1", connection);
                int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (count == 0)
                {
                    // Create default tab
                    using var insertCmd = new SqliteCommand(@"
                        INSERT INTO ClientTabs (Name, DisplayOrder, IsDefault, CreatedAt, CreatedBy)
                        VALUES ('All Clients', 0, 1, @createdAt, 1)", connection);
                    insertCmd.Parameters.AddWithValue("@createdAt", DateTime.Now);
                    insertCmd.ExecuteNonQuery();

                    System.Diagnostics.Debug.WriteLine("Default ClientTab 'All Clients' created");

                    // Get the default tab ID
                    using var getIdCmd = new SqliteCommand(
                        "SELECT Id FROM ClientTabs WHERE IsDefault = 1 LIMIT 1", connection);
                    int defaultTabId = Convert.ToInt32(getIdCmd.ExecuteScalar());

                    // Update all existing clients to use the default tab
                    using var updateCmd = new SqliteCommand(@"
                        UPDATE Clients
                        SET TabId = @tabId
                        WHERE TabId IS NULL", connection);
                    updateCmd.Parameters.AddWithValue("@tabId", defaultTabId);
                    int updated = updateCmd.ExecuteNonQuery();

                    System.Diagnostics.Debug.WriteLine($"Updated {updated} clients to default tab");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing default tab: {ex.Message}");
            }
        }
    }
}