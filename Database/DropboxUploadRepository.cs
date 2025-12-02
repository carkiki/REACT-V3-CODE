using System;

using Microsoft.Data.Sqlite;

using ReactCRM.Services;



namespace ReactCRM.Database

{

    /// <summary>

    /// Repositorio para manejar File Requests de Dropbox

    /// </summary>

    public class DropboxUploadRepository

    {

        /// <summary>

        /// Guarda el File Request de Dropbox en el cliente

        /// </summary>

        public void SaveFileRequest(int clientId, DropboxFileRequest request, int userId)

        {

            using var conn = DbConnection.GetConnection();

            conn.Open();

            using var transaction = conn.BeginTransaction();



            try

            {

                var sql = @"

                    UPDATE Clients SET

                        DropboxRequestId = @requestId,

                        DropboxUploadUrl = @url,

                        DropboxFolder = @folder,

                        UploadLinkCreated = @created,

                        UpdatedBy = @userId,

                        LastUpdated = CURRENT_TIMESTAMP

                    WHERE Id = @clientId";



                using var cmd = new SqliteCommand(sql, conn, transaction);

                cmd.Parameters.AddWithValue("@requestId", request.Id);

                cmd.Parameters.AddWithValue("@url", request.Url);

                cmd.Parameters.AddWithValue("@folder", request.Destination);

                cmd.Parameters.AddWithValue("@created", request.Created.ToString("yyyy-MM-dd HH:mm:ss"));

                cmd.Parameters.AddWithValue("@userId", userId);

                cmd.Parameters.AddWithValue("@clientId", clientId);

                cmd.ExecuteNonQuery();



                // Log

                LogAction(conn, transaction, clientId, userId, "CREATE_UPLOAD_LINK",

                    $"Dropbox upload link created: {request.Url}");



                transaction.Commit();

            }

            catch

            {

                transaction.Rollback();

                throw;

            }

        }



        /// <summary>

        /// Obtiene los datos del File Request de un cliente

        /// </summary>

        public ClientDropboxInfo GetClientDropboxInfo(int clientId)

        {

            using var conn = DbConnection.GetConnection();

            conn.Open();



            var sql = @"

                SELECT DropboxRequestId, DropboxUploadUrl, DropboxFolder, UploadLinkCreated

                FROM Clients WHERE Id = @clientId";



            using var cmd = new SqliteCommand(sql, conn);

            cmd.Parameters.AddWithValue("@clientId", clientId);



            using var reader = cmd.ExecuteReader();

            if (reader.Read())

            {

                var requestId = reader["DropboxRequestId"]?.ToString();

                if (string.IsNullOrEmpty(requestId))

                    return null;



                return new ClientDropboxInfo

                {

                    RequestId = requestId,

                    UploadUrl = reader["DropboxUploadUrl"]?.ToString(),

                    DropboxFolder = reader["DropboxFolder"]?.ToString(),

                    Created = reader["UploadLinkCreated"] != DBNull.Value

                        ? Convert.ToDateTime(reader["UploadLinkCreated"])

                        : DateTime.MinValue

                };

            }



            return null;

        }



        /// <summary>

        /// Elimina el File Request del cliente

        /// </summary>

        public void ClearFileRequest(int clientId, int userId)

        {

            using var conn = DbConnection.GetConnection();

            conn.Open();

            using var transaction = conn.BeginTransaction();



            try

            {

                var sql = @"

                    UPDATE Clients SET

                        DropboxRequestId = NULL,

                        DropboxUploadUrl = NULL,

                        DropboxFolder = NULL,

                        UploadLinkCreated = NULL,

                        UpdatedBy = @userId,

                        LastUpdated = CURRENT_TIMESTAMP

                    WHERE Id = @clientId";



                using var cmd = new SqliteCommand(sql, conn, transaction);

                cmd.Parameters.AddWithValue("@userId", userId);

                cmd.Parameters.AddWithValue("@clientId", clientId);

                cmd.ExecuteNonQuery();



                LogAction(conn, transaction, clientId, userId, "REVOKE_UPLOAD_LINK",

                    "Dropbox upload link removed");



                transaction.Commit();

            }

            catch

            {

                transaction.Rollback();

                throw;

            }

        }



        /// <summary>

        /// Guarda registro de archivo descargado de Dropbox

        /// </summary>

        public int SaveDownloadedFile(int clientId, string fileName, string localPath,

            long fileSize, string dropboxPath)

        {

            using var conn = DbConnection.GetConnection();

            conn.Open();

            using var transaction = conn.BeginTransaction();



            try

            {

                var sql = @"

                    INSERT INTO ClientFiles

                    (ClientId, FileName, FilePath, FileSize, Description, DropboxPath, UploadedAt, UploadedBy)

                    VALUES

                    (@clientId, @fileName, @localPath, @fileSize, 'Downloaded from Dropbox', @dropboxPath, CURRENT_TIMESTAMP, NULL);

                    SELECT last_insert_rowid();";



                using var cmd = new SqliteCommand(sql, conn, transaction);

                cmd.Parameters.AddWithValue("@clientId", clientId);

                cmd.Parameters.AddWithValue("@fileName", fileName);

                cmd.Parameters.AddWithValue("@localPath", localPath);

                cmd.Parameters.AddWithValue("@fileSize", fileSize);

                cmd.Parameters.AddWithValue("@dropboxPath", dropboxPath);



                var fileId = Convert.ToInt32(cmd.ExecuteScalar());



                // Log

                var logSql = @"

                    INSERT INTO AuditLog (User, UserId, ActionType, Entity, EntityId, Description, Metadata)

                    VALUES ('Dropbox Sync', NULL, 'DOWNLOAD', 'ClientFile', @fileId, @desc, '{}')";



                using var logCmd = new SqliteCommand(logSql, conn, transaction);

                logCmd.Parameters.AddWithValue("@fileId", fileId);

                logCmd.Parameters.AddWithValue("@desc", $"File downloaded: {fileName}");

                logCmd.ExecuteNonQuery();



                transaction.Commit();

                return fileId;

            }

            catch

            {

                transaction.Rollback();

                throw;

            }

        }



        private void LogAction(SqliteConnection conn, SqliteTransaction transaction,

            int clientId, int userId, string action, string description)

        {

            var sql = @"

                INSERT INTO AuditLog (User, UserId, ActionType, Entity, EntityId, Description, Metadata)

                SELECT Username, @userId, @action, 'Client', @clientId, @desc, '{}'

                FROM Workers WHERE Id = @userId";



            using var cmd = new SqliteCommand(sql, conn, transaction);

            cmd.Parameters.AddWithValue("@userId", userId);

            cmd.Parameters.AddWithValue("@action", action);

            cmd.Parameters.AddWithValue("@clientId", clientId);

            cmd.Parameters.AddWithValue("@desc", description);

            cmd.ExecuteNonQuery();

        }

    }



    public class ClientDropboxInfo

    {

        public string RequestId { get; set; }

        public string UploadUrl { get; set; }

        public string DropboxFolder { get; set; }

        public DateTime Created { get; set; }

    }

}