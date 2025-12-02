using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;
using ReactCRM.Models;
using ReactCRM.Utils;

namespace ReactCRM.Database
{
    public class WorkerRepository
    {
        public Worker Authenticate(string username, string password)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = "SELECT * FROM Workers WHERE Username = @username AND IsActive = 1";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@username", username);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var worker = MapReaderToWorker(reader);

                // Verify password
                string hashedPassword = HashUtils.HashPassword(password, worker.Salt);
                if (hashedPassword == worker.PasswordHash)
                {
                    // Update last login
                    UpdateLastLogin(worker.Id);
                    return worker;
                }
            }

            return null;
        }

        public List<Worker> GetAllWorkers()
        {
            var workers = new List<Worker>();
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = "SELECT * FROM Workers ORDER BY Username";
            using var cmd = new SqliteCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                workers.Add(MapReaderToWorker(reader));
            }

            return workers;
        }

        public Worker GetWorkerById(int id)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = "SELECT * FROM Workers WHERE Id = @id";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapReaderToWorker(reader);
            }

            return null;
        }

        public Worker GetWorkerByUsername(string username)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = "SELECT * FROM Workers WHERE Username = @username";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@username", username);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapReaderToWorker(reader);
            }

            return null;
        }

        public int CreateWorker(Worker worker, string password, int createdByUserId)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Generate salt and hash password
                worker.Salt = HashUtils.GenerateSalt();
                worker.PasswordHash = HashUtils.HashPassword(password, worker.Salt);

                string sql = @"
                    INSERT INTO Workers (Username, PasswordHash, Salt, Role, IsActive)
                    VALUES (@username, @passwordHash, @salt, @role, @isActive);
                    SELECT last_insert_rowid();";

                using var cmd = new SqliteCommand(sql, connection, transaction);
                cmd.Parameters.AddWithValue("@username", worker.Username);
                cmd.Parameters.AddWithValue("@passwordHash", worker.PasswordHash);
                cmd.Parameters.AddWithValue("@salt", worker.Salt);
                cmd.Parameters.AddWithValue("@role", worker.Role.ToString());
                cmd.Parameters.AddWithValue("@isActive", worker.IsActive);

                int workerId = Convert.ToInt32(cmd.ExecuteScalar());

                // Log the creation
                LogWorkerAction(connection, transaction, "CREATE", workerId, createdByUserId,
                    $"Worker created: {worker.Username} with role {worker.Role}");

                transaction.Commit();
                // Send notification to admins about new worker

                Services.NotificationService.NotifyWorkerCreated(worker.Username, worker.Role.ToString());



                // Send welcome notification to the new worker

                Services.NotificationService.SendWelcomeNotification(workerId, worker.Username);
                return workerId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool UpdateWorker(Worker worker, int updatedByUserId)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                string sql = @"
                    UPDATE Workers 
                    SET Username = @username, 
                        Role = @role, 
                        IsActive = @isActive
                    WHERE Id = @id";

                using var cmd = new SqliteCommand(sql, connection, transaction);
                cmd.Parameters.AddWithValue("@username", worker.Username);
                cmd.Parameters.AddWithValue("@role", worker.Role.ToString());
                cmd.Parameters.AddWithValue("@isActive", worker.IsActive);
                cmd.Parameters.AddWithValue("@id", worker.Id);

                int rowsAffected = cmd.ExecuteNonQuery();

                // Log the update
                LogWorkerAction(connection, transaction, "UPDATE", worker.Id, updatedByUserId,
                    $"Worker updated: {worker.Username}");

                transaction.Commit();
                return rowsAffected > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool ChangePassword(int workerId, string newPassword, int changedByUserId)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Generate new salt and hash
                string salt = HashUtils.GenerateSalt();
                string passwordHash = HashUtils.HashPassword(newPassword, salt);

                string sql = @"
                    UPDATE Workers 
                    SET PasswordHash = @passwordHash, Salt = @salt
                    WHERE Id = @id";

                using var cmd = new SqliteCommand(sql, connection, transaction);
                cmd.Parameters.AddWithValue("@passwordHash", passwordHash);
                cmd.Parameters.AddWithValue("@salt", salt);
                cmd.Parameters.AddWithValue("@id", workerId);

                int rowsAffected = cmd.ExecuteNonQuery();

                // Log the password change
                LogWorkerAction(connection, transaction, "PASSWORD_CHANGE", workerId, changedByUserId,
                    "Password changed");

                transaction.Commit();
                return rowsAffected > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        public bool DeactivateWorker(int workerId, int deactivatedByUserId)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                var worker = GetWorkerById(workerId);
                if (worker == null) return false;
                string sql = "UPDATE Workers SET IsActive = 0 WHERE Id = @id";
                using var cmd = new SqliteCommand(sql, connection, transaction);
                cmd.Parameters.AddWithValue("@id", workerId);
                int rowsAffected = cmd.ExecuteNonQuery();
                // Log the deactivation
                LogWorkerAction(connection, transaction, "DEACTIVATE", workerId, deactivatedByUserId,
                    $"Worker deactivated: {worker.Username}");

                // Send notification to admins

                Services.NotificationService.NotifyWorkerDeactivated(worker.Username, deactivatedByUserId);


                transaction.Commit();
                return rowsAffected > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        public bool DeleteWorker(int workerId, int deletedByUserId)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                var worker = GetWorkerById(workerId);
                if (worker == null) return false;
                // Store worker info for logging before deletion
                string workerUsername = worker.Username;
                // Delete the worker from database
                string sql = "DELETE FROM Workers WHERE Id = @id";
                using var cmd = new SqliteCommand(sql, connection, transaction);
                cmd.Parameters.AddWithValue("@id", workerId);
                int rowsAffected = cmd.ExecuteNonQuery();
                // Log the deletion
                LogWorkerAction(connection, transaction, "DELETE", workerId, deletedByUserId,
                    $"Worker permanently deleted: {workerUsername}");
                transaction.Commit();
                Services.NotificationService.NotifyWorkerDeleted(workerUsername, deletedByUserId);
                return rowsAffected > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

        }


        private void UpdateLastLogin(int workerId)
        {
            string sql = "UPDATE Workers SET LastLogin = CURRENT_TIMESTAMP WHERE Id = @id";
            DbConnection.ExecuteNonQuery(sql, new SqliteParameter("@id", workerId));
        }

        private void LogWorkerAction(SqliteConnection connection, SqliteTransaction transaction,
            string actionType, int workerId, int userId, string description)
        {
            string sql = @"
                INSERT INTO AuditLog (User, UserId, ActionType, Entity, EntityId, Description, Metadata)
                SELECT @user, @userId, @actionType, 'Worker', @entityId, @description, '{}'
                FROM Workers WHERE Id = @userId";

            using var cmd = new SqliteCommand(sql, connection, transaction);
            cmd.Parameters.AddWithValue("@user", GetUsername(userId));
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@actionType", actionType);
            cmd.Parameters.AddWithValue("@entityId", workerId);
            cmd.Parameters.AddWithValue("@description", description);
            cmd.ExecuteNonQuery();
        }

        private string GetUsername(int userId)
        {
            return DbConnection.ExecuteScalar<string>(
                "SELECT Username FROM Workers WHERE Id = @id",
                new SqliteParameter("@id", userId)
            ) ?? "System";
        }

        private Worker MapReaderToWorker(SqliteDataReader reader)
        {
            var worker = new Worker
            {
                Id = Convert.ToInt32(reader["Id"]),
                Username = reader["Username"].ToString(),
                PasswordHash = reader["PasswordHash"].ToString(),
                Salt = reader["Salt"].ToString(),
                IsActive = Convert.ToBoolean(reader["IsActive"]),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
            };

            if (Enum.TryParse<UserRole>(reader["Role"].ToString(), out UserRole role))
            {
                worker.Role = role;
            }

            if (reader["LastLogin"] != DBNull.Value)
            {
                worker.LastLogin = Convert.ToDateTime(reader["LastLogin"]);
            }

            return worker;
        }
    }
}