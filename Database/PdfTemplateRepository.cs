using System;

using System.Collections.Generic;

using Microsoft.Data.Sqlite;

using Newtonsoft.Json;

using ReactCRM.Models;



namespace ReactCRM.Database

{

    public class PdfTemplateRepository

    {

        public static void InitializeTable()

        {

            using var connection = DbConnection.GetConnection();

            connection.Open();



            string sql = @"

                CREATE TABLE IF NOT EXISTS PdfTemplates (

                    Id INTEGER PRIMARY KEY AUTOINCREMENT,

                    Name TEXT NOT NULL,

                    Description TEXT,

                    Category TEXT,

                    FileName TEXT NOT NULL,

                    FilePath TEXT NOT NULL,

                    FileSize INTEGER DEFAULT 0,

                    FieldMappings TEXT,

                    IsActive INTEGER DEFAULT 1,

                    CreatedBy INTEGER,

                    CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,

                    UpdatedBy INTEGER,

                    UpdatedAt TEXT

                )";



            using var command = new SqliteCommand(sql, connection);

            command.ExecuteNonQuery();

        }



        #region Instance Methods (for PdfTemplateService)



        public List<PdfTemplate> GetAllTemplates()

        {

            return GetAll();

        }



        public List<PdfTemplate> GetActiveTemplates()

        {

            return GetActive();

        }



        public List<string> GetCategories()

        {

            var categories = new List<string>();



            using var connection = DbConnection.GetConnection();

            connection.Open();



            string sql = "SELECT DISTINCT Category FROM PdfTemplates WHERE Category IS NOT NULL AND Category != '' ORDER BY Category";

            using var command = new SqliteCommand(sql, connection);

            using var reader = command.ExecuteReader();



            while (reader.Read())

            {

                categories.Add(reader.GetString(0));

            }



            return categories;

        }



        public CrmPdfTemplate? GetTemplateById(int id)

        {

            var template = GetById(id);

            if (template == null) return null;



            return new CrmPdfTemplate(template);

        }



        public int CreateTemplate(PdfTemplate template, int createdByUserId)

        {

            using var connection = DbConnection.GetConnection();

            connection.Open();



            string sql = @"

                INSERT INTO PdfTemplates (Name, Description, Category, FileName, FilePath, FileSize, FieldMappings, IsActive, CreatedBy, CreatedAt)

                VALUES (@Name, @Description, @Category, @FileName, @FilePath, @FileSize, @FieldMappings, @IsActive, @CreatedBy, @CreatedAt);

                SELECT last_insert_rowid();";



            using var command = new SqliteCommand(sql, connection);

            command.Parameters.AddWithValue("@Name", template.Name);

            command.Parameters.AddWithValue("@Description", template.Description ?? (object)DBNull.Value);

            command.Parameters.AddWithValue("@Category", template.Category ?? (object)DBNull.Value);

            command.Parameters.AddWithValue("@FileName", template.FileName);

            command.Parameters.AddWithValue("@FilePath", template.FilePath);

            command.Parameters.AddWithValue("@FileSize", template.FileSize);

            command.Parameters.AddWithValue("@FieldMappings", template.FieldMappings ?? (object)DBNull.Value);

            command.Parameters.AddWithValue("@IsActive", template.IsActive ? 1 : 0);

            command.Parameters.AddWithValue("@CreatedBy", createdByUserId);

            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("o"));



            return Convert.ToInt32(command.ExecuteScalar());

        }



        public bool UpdateTemplate(CrmPdfTemplate template, int updatedByUserId)

        {

            try

            {

                using var connection = DbConnection.GetConnection();

                connection.Open();



                string sql = @"

                    UPDATE PdfTemplates

                    SET Name = @Name,

                        Description = @Description,

                        Category = @Category,

                        FileName = @FileName,

                        FilePath = @FilePath,

                        FileSize = @FileSize,

                        FieldMappings = @FieldMappings,

                        IsActive = @IsActive,

                        UpdatedBy = @UpdatedBy,

                        UpdatedAt = @UpdatedAt

                    WHERE Id = @Id";



                using var command = new SqliteCommand(sql, connection);

                command.Parameters.AddWithValue("@Id", template.Id);

                command.Parameters.AddWithValue("@Name", template.Name);

                command.Parameters.AddWithValue("@Description", template.Description ?? (object)DBNull.Value);

                command.Parameters.AddWithValue("@Category", template.Category ?? (object)DBNull.Value);

                command.Parameters.AddWithValue("@FileName", template.FileName);

                command.Parameters.AddWithValue("@FilePath", template.FilePath);

                command.Parameters.AddWithValue("@FileSize", template.FileSize);

                command.Parameters.AddWithValue("@FieldMappings", JsonConvert.SerializeObject(template.FieldMappings));

                command.Parameters.AddWithValue("@IsActive", template.IsActive ? 1 : 0);

                command.Parameters.AddWithValue("@UpdatedBy", updatedByUserId);

                command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("o"));



                return command.ExecuteNonQuery() > 0;

            }

            catch

            {

                return false;

            }

        }



        public bool DeleteTemplate(int templateId, int deletedByUserId)

        {

            try

            {

                using var connection = DbConnection.GetConnection();

                connection.Open();



                string sql = "DELETE FROM PdfTemplates WHERE Id = @Id";

                using var command = new SqliteCommand(sql, connection);

                command.Parameters.AddWithValue("@Id", templateId);



                return command.ExecuteNonQuery() > 0;

            }

            catch

            {

                return false;

            }

        }



        #endregion



        #region Static Methods



        public static List<PdfTemplate> GetAll()

        {

            var templates = new List<PdfTemplate>();



            using var connection = DbConnection.GetConnection();

            connection.Open();



            string sql = "SELECT * FROM PdfTemplates ORDER BY Name";

            using var command = new SqliteCommand(sql, connection);

            using var reader = command.ExecuteReader();



            while (reader.Read())

            {

                templates.Add(MapReaderToTemplate(reader));

            }



            return templates;

        }



        public static List<PdfTemplate> GetActive()

        {

            var templates = new List<PdfTemplate>();



            using var connection = DbConnection.GetConnection();

            connection.Open();



            string sql = "SELECT * FROM PdfTemplates WHERE IsActive = 1 ORDER BY Name";

            using var command = new SqliteCommand(sql, connection);

            using var reader = command.ExecuteReader();



            while (reader.Read())

            {

                templates.Add(MapReaderToTemplate(reader));

            }



            return templates;

        }



        public static PdfTemplate? GetById(int id)

        {

            using var connection = DbConnection.GetConnection();

            connection.Open();



            string sql = "SELECT * FROM PdfTemplates WHERE Id = @Id";

            using var command = new SqliteCommand(sql, connection);

            command.Parameters.AddWithValue("@Id", id);



            using var reader = command.ExecuteReader();

            if (reader.Read())

            {

                return MapReaderToTemplate(reader);

            }



            return null;

        }



        public static PdfTemplate? GetByName(string name)

        {

            using var connection = DbConnection.GetConnection();

            connection.Open();



            string sql = "SELECT * FROM PdfTemplates WHERE Name = @Name";

            using var command = new SqliteCommand(sql, connection);

            command.Parameters.AddWithValue("@Name", name);



            using var reader = command.ExecuteReader();

            if (reader.Read())

            {

                return MapReaderToTemplate(reader);

            }



            return null;

        }



        public static List<PdfTemplate> GetByCategory(string category)

        {

            var templates = new List<PdfTemplate>();



            using var connection = DbConnection.GetConnection();

            connection.Open();



            string sql = "SELECT * FROM PdfTemplates WHERE Category = @Category ORDER BY Name";

            using var command = new SqliteCommand(sql, connection);

            command.Parameters.AddWithValue("@Category", category);



            using var reader = command.ExecuteReader();

            while (reader.Read())

            {

                templates.Add(MapReaderToTemplate(reader));

            }



            return templates;

        }



        public static int Insert(PdfTemplate template)

        {

            using var connection = DbConnection.GetConnection();

            connection.Open();



            string sql = @"

                INSERT INTO PdfTemplates (Name, Description, Category, FileName, FilePath, FileSize, FieldMappings, IsActive, CreatedAt)

                VALUES (@Name, @Description, @Category, @FileName, @FilePath, @FileSize, @FieldMappings, @IsActive, @CreatedAt);

                SELECT last_insert_rowid();";



            using var command = new SqliteCommand(sql, connection);

            command.Parameters.AddWithValue("@Name", template.Name);

            command.Parameters.AddWithValue("@Description", template.Description ?? (object)DBNull.Value);

            command.Parameters.AddWithValue("@Category", template.Category ?? (object)DBNull.Value);

            command.Parameters.AddWithValue("@FileName", template.FileName);

            command.Parameters.AddWithValue("@FilePath", template.FilePath);

            command.Parameters.AddWithValue("@FileSize", template.FileSize);

            command.Parameters.AddWithValue("@FieldMappings", template.FieldMappings ?? (object)DBNull.Value);

            command.Parameters.AddWithValue("@IsActive", template.IsActive ? 1 : 0);

            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("o"));



            return Convert.ToInt32(command.ExecuteScalar());

        }



        public static void Update(PdfTemplate template)

        {

            using var connection = DbConnection.GetConnection();

            connection.Open();



            string sql = @"

                UPDATE PdfTemplates

                SET Name = @Name,

                    Description = @Description,

                    Category = @Category,

                    FileName = @FileName,

                    FilePath = @FilePath,

                    FileSize = @FileSize,

                    FieldMappings = @FieldMappings,

                    IsActive = @IsActive,

                    UpdatedAt = @UpdatedAt

                WHERE Id = @Id";



            using var command = new SqliteCommand(sql, connection);

            command.Parameters.AddWithValue("@Id", template.Id);

            command.Parameters.AddWithValue("@Name", template.Name);

            command.Parameters.AddWithValue("@Description", template.Description ?? (object)DBNull.Value);

            command.Parameters.AddWithValue("@Category", template.Category ?? (object)DBNull.Value);

            command.Parameters.AddWithValue("@FileName", template.FileName);

            command.Parameters.AddWithValue("@FilePath", template.FilePath);

            command.Parameters.AddWithValue("@FileSize", template.FileSize);

            command.Parameters.AddWithValue("@FieldMappings", template.FieldMappings ?? (object)DBNull.Value);

            command.Parameters.AddWithValue("@IsActive", template.IsActive ? 1 : 0);

            command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("o"));



            command.ExecuteNonQuery();

        }



        public static void Delete(int id)

        {

            using var connection = DbConnection.GetConnection();

            connection.Open();



            string sql = "DELETE FROM PdfTemplates WHERE Id = @Id";

            using var command = new SqliteCommand(sql, connection);

            command.Parameters.AddWithValue("@Id", id);

            command.ExecuteNonQuery();

        }



        #endregion



        private static PdfTemplate MapReaderToTemplate(SqliteDataReader reader)

        {

            return new PdfTemplate

            {

                Id = reader.GetInt32(reader.GetOrdinal("Id")),

                Name = reader.GetString(reader.GetOrdinal("Name")),

                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? string.Empty : reader.GetString(reader.GetOrdinal("Description")),

                Category = reader.IsDBNull(reader.GetOrdinal("Category")) ? string.Empty : reader.GetString(reader.GetOrdinal("Category")),

                FileName = reader.GetString(reader.GetOrdinal("FileName")),

                FilePath = reader.GetString(reader.GetOrdinal("FilePath")),

                FileSize = reader.IsDBNull(reader.GetOrdinal("FileSize")) ? 0 : reader.GetInt64(reader.GetOrdinal("FileSize")),

                FieldMappings = reader.IsDBNull(reader.GetOrdinal("FieldMappings")) ? string.Empty : reader.GetString(reader.GetOrdinal("FieldMappings")),

                IsActive = reader.GetInt32(reader.GetOrdinal("IsActive")) == 1,

                CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt")) ? DateTime.Now : DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt"))),

                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : DateTime.Parse(reader.GetString(reader.GetOrdinal("UpdatedAt")))

            };

        }

    }

}