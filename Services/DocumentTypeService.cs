using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using ReactCRM.Database;

namespace ReactCRM.Services
{
    /// <summary>
    /// Service for managing document types in file attachments
    /// Provides CRUD operations and caching for document types
    /// </summary>
    public class DocumentTypeService
    {
        private static DocumentTypeService _instance;
        private static readonly object _lock = new object();
        private List<DocumentType> _cache;
        private DateTime _cacheLastUpdated;

        public DocumentTypeService() { }

        public static DocumentTypeService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DocumentTypeService();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Gets all active document types (with caching)
        /// </summary>
        public List<DocumentType> GetAllDocumentTypes(bool forceRefresh = false)
        {
            // Use cache if less than 5 minutes old and not forcing refresh
            if (!forceRefresh && _cache != null &&
                (DateTime.Now - _cacheLastUpdated).TotalMinutes < 5)
            {
                return _cache;
            }

            _cache = LoadDocumentTypesFromDatabase();
            _cacheLastUpdated = DateTime.Now;
            return _cache;
        }


        /// <summary>

        /// Gets active document types (alias for GetAllDocumentTypes)

        /// </summary>

        public List<DocumentType> GetActiveDocumentTypes()

        {

            return GetAllDocumentTypes();

        }

        /// <summary>
        /// Gets a specific document type by ID
        /// </summary>
        public DocumentType GetDocumentTypeById(int id)
        {
            return GetAllDocumentTypes().FirstOrDefault(d => d.Id == id);
        }

        /// <summary>
        /// Gets a document type by name
        /// </summary>
        public DocumentType GetDocumentTypeByName(string name)
        {
            return GetAllDocumentTypes().FirstOrDefault(d => d.TypeName == name);
        }

        /// <summary>
        /// Creates a new document type
        /// </summary>
        public DocumentType CreateDocumentType(string typeName, string description, string fileExtensions)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentException("Type name cannot be empty");

            // Check for duplicates
            if (GetDocumentTypeByName(typeName) != null)
                throw new InvalidOperationException($"Document type '{typeName}' already exists");

            try
            {
                using var connection = DbConnection.GetConnection();
                connection.Open();

                string sql = @"
                    INSERT INTO DocumentTypes (TypeName, Description, FileExtensions, IsActive, CreatedBy)
                    VALUES (@typeName, @description, @extensions, 1, @createdBy);
                    SELECT last_insert_rowid();";

                using var cmd = new SqliteCommand(sql, connection);
                cmd.Parameters.AddWithValue("@typeName", typeName);
                cmd.Parameters.AddWithValue("@description", description ?? "");
                cmd.Parameters.AddWithValue("@extensions", fileExtensions ?? "");
                cmd.Parameters.AddWithValue("@createdBy", AuthService.Instance.GetCurrentUserId());

                int newId = Convert.ToInt32(cmd.ExecuteScalar());

                // Log the action
                AuditService.LogAction("CreateDocumentType", "DocumentType", newId,
                    $"Created new document type: {typeName}");

                // Invalidate cache
                _cache = null;

                return GetDocumentTypeById(newId);
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException($"Error creating document type: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates a document type
        /// </summary>
        public void UpdateDocumentType(int id, string typeName, string description, string fileExtensions)
        {
            try
            {
                using var connection = DbConnection.GetConnection();
                connection.Open();

                string sql = @"
                    UPDATE DocumentTypes
                    SET TypeName = @typeName, Description = @description, FileExtensions = @extensions, UpdatedAt = CURRENT_TIMESTAMP
                    WHERE Id = @id";

                using var cmd = new SqliteCommand(sql, connection);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@typeName", typeName);
                cmd.Parameters.AddWithValue("@description", description ?? "");
                cmd.Parameters.AddWithValue("@extensions", fileExtensions ?? "");

                cmd.ExecuteNonQuery();

                // Log the action
                AuditService.LogAction("UpdateDocumentType", "DocumentType", id,
                    $"Updated document type: {typeName}");

                // Invalidate cache
                _cache = null;
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException($"Error updating document type: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Toggles document type active status
        /// </summary>
        public void ToggleDocumentTypeStatus(int id)
        {
            try
            {
                using var connection = DbConnection.GetConnection();
                connection.Open();

                string sql = @"
                    UPDATE DocumentTypes
                    SET IsActive = NOT IsActive, UpdatedAt = CURRENT_TIMESTAMP
                    WHERE Id = @id";

                using var cmd = new SqliteCommand(sql, connection);
                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();

                // Log the action
                AuditService.LogAction("ToggleDocumentTypeStatus", "DocumentType", id,
                    $"Toggled document type status");

                // Invalidate cache
                _cache = null;
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException($"Error toggling document type status: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deletes a document type
        /// </summary>
        public void DeleteDocumentType(int id)
        {
            try
            {
                using var connection = DbConnection.GetConnection();
                connection.Open();

                // Check if any files use this type
                string checkSql = "SELECT COUNT(*) FROM ClientFiles WHERE DocumentTypeId = @id";
                using var checkCmd = new SqliteCommand(checkSql, connection);
                checkCmd.Parameters.AddWithValue("@id", id);
                int fileCount = Convert.ToInt32(checkCmd.ExecuteScalar() ?? 0);

                if (fileCount > 0)
                    throw new InvalidOperationException($"Cannot delete document type - {fileCount} files are using this type");

                string deleteSql = "DELETE FROM DocumentTypes WHERE Id = @id";
                using var deleteCmd = new SqliteCommand(deleteSql, connection);
                deleteCmd.Parameters.AddWithValue("@id", id);

                deleteCmd.ExecuteNonQuery();

                // Log the action
                AuditService.LogAction("DeleteDocumentType", "DocumentType", id,
                    $"Deleted document type");

                // Invalidate cache
                _cache = null;
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException($"Error deleting document type: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Loads document types from database
        /// </summary>
        private List<DocumentType> LoadDocumentTypesFromDatabase()
        {
            var types = new List<DocumentType>();

            try
            {
                using var connection = DbConnection.GetConnection();
                connection.Open();

                string sql = @"
                    SELECT Id, TypeName, Description, FileExtensions, IsActive, CreatedAt
                    FROM DocumentTypes
                    WHERE IsActive = 1
                    ORDER BY TypeName ASC";

                using var cmd = new SqliteCommand(sql, connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    types.Add(new DocumentType
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        TypeName = reader["TypeName"].ToString(),
                        Description = reader["Description"]?.ToString() ?? "",
                        FileExtensions = reader["FileExtensions"]?.ToString() ?? "",
                        IsActive = Convert.ToBoolean(reader["IsActive"]),
                        CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                    });
                }
            }
            catch (SqliteException)
            {
                // Return empty list if table doesn't exist yet
                return types;
            }

            return types;
        }
    }

    /// <summary>
    /// Represents a document type for file attachments
    /// </summary>
    public class DocumentType
    {
        public int Id { get; set; }
        public string TypeName { get; set; }
        public string Description { get; set; }
        public string FileExtensions { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public override string ToString()
        {
            return TypeName;
        }
    }
}
