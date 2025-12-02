using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using ReactCRM.Models;

namespace ReactCRM.Database
{
    public class ClientRepository
    {
        public List<Client> GetAllClients()
        {
            var clients = new List<Client>();
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                SELECT c.*, w.Username as CreatedByName, w2.Username as UpdatedByName
                FROM Clients c
                LEFT JOIN Workers w ON c.CreatedBy = w.Id
                LEFT JOIN Workers w2 ON c.UpdatedBy = w2.Id
                ORDER BY c.Name";

            using var cmd = new SqliteCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                clients.Add(MapReaderToClient(reader));
            }

            return clients;
        }

        public Client GetClientById(int id)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                SELECT c.*, w.Username as CreatedByName, w2.Username as UpdatedByName
                FROM Clients c
                LEFT JOIN Workers w ON c.CreatedBy = w.Id
                LEFT JOIN Workers w2 ON c.UpdatedBy = w2.Id
                WHERE c.Id = @id";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapReaderToClient(reader);
            }

            return null;
        }

        public Client GetClientBySSN(string ssn)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                SELECT c.*, w.Username as CreatedByName, w2.Username as UpdatedByName
                FROM Clients c
                LEFT JOIN Workers w ON c.CreatedBy = w.Id
                LEFT JOIN Workers w2 ON c.UpdatedBy = w2.Id
                WHERE c.SSN = @ssn";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@ssn", ssn);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapReaderToClient(reader);
            }

            return null;
        }

        public int CreateClient(Client client, int userId)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                string sql = @"
                    INSERT INTO Clients (SSN, Name, DOB, Phone, Email, Notes, ExtraData, CreatedBy, UpdatedBy)
                    VALUES (@ssn, @name, @dob, @phone, @email, @notes, @extraData, @userId, @userId);
                    SELECT last_insert_rowid();";

                using var cmd = new SqliteCommand(sql, connection, transaction);
                cmd.Parameters.AddWithValue("@ssn", client.SSN);
                cmd.Parameters.AddWithValue("@name", client.Name);
                cmd.Parameters.AddWithValue("@dob", client.DOB?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@phone", client.Phone ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@email", client.Email ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@notes", client.Notes ?? (object)DBNull.Value);

                // Serialize ExtraData with explicit settings to preserve empty strings
                string extraDataJson = JsonConvert.SerializeObject(
                    client.ExtraData ?? new Dictionary<string, object>(),
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        DefaultValueHandling = DefaultValueHandling.Include
                    });

                System.Diagnostics.Debug.WriteLine($"CreateClient: Serialized ExtraData JSON = '{extraDataJson}'");
                cmd.Parameters.AddWithValue("@extraData", extraDataJson);
                cmd.Parameters.AddWithValue("@userId", userId);

                int clientId = Convert.ToInt32(cmd.ExecuteScalar());

                // Log the creation
                LogClientAction(connection, transaction, "CREATE", clientId, userId, $"Client created: {client.Name}");

                transaction.Commit();
                return clientId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool UpdateClient(Client client, int userId, bool trackHistory = true)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Get current values for history tracking
                if (trackHistory)
                {
                    var oldClient = GetClientById(client.Id);
                    if (oldClient != null)
                    {
                        TrackFieldChanges(connection, transaction, oldClient, client, userId);
                    }
                }

                string sql = @"
                    UPDATE Clients
                    SET Name = @name,
                        DOB = @dob,
                        Phone = @phone,
                        Email = @email,
                        Notes = @notes,
                        ExtraData = @extraData,
                        UpdatedBy = @userId,
                        LastUpdated = CURRENT_TIMESTAMP
                    WHERE Id = @id";

                using var cmd = new SqliteCommand(sql, connection, transaction);
                cmd.Parameters.AddWithValue("@name", client.Name);
                cmd.Parameters.AddWithValue("@dob", client.DOB?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@phone", client.Phone ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@email", client.Email ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@notes", client.Notes ?? (object)DBNull.Value);

                // Serialize ExtraData with explicit settings to preserve empty strings
                string extraDataJson = JsonConvert.SerializeObject(
                    client.ExtraData ?? new Dictionary<string, object>(),
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        DefaultValueHandling = DefaultValueHandling.Include
                    });

                System.Diagnostics.Debug.WriteLine($"UpdateClient (Client {client.Id}): Serialized ExtraData JSON = '{extraDataJson}'");
                cmd.Parameters.AddWithValue("@extraData", extraDataJson);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@id", client.Id);

                int rowsAffected = cmd.ExecuteNonQuery();

                // Log the update
                LogClientAction(connection, transaction, "UPDATE", client.Id, userId, $"Client updated: {client.Name}");

                transaction.Commit();
                return rowsAffected > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool DeleteClient(int clientId, int userId)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Get client info for logging
                var client = GetClientById(clientId);
                if (client == null) return false;

                // Delete associated files
                string deleteFiles = "DELETE FROM ClientFiles WHERE ClientId = @clientId";
                using (var cmd = new SqliteCommand(deleteFiles, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@clientId", clientId);
                    cmd.ExecuteNonQuery();
                }

                // Delete field history
                string deleteHistory = "DELETE FROM FieldHistory WHERE ClientId = @clientId";
                using (var cmd = new SqliteCommand(deleteHistory, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@clientId", clientId);
                    cmd.ExecuteNonQuery();
                }

                // Delete client
                string deleteClient = "DELETE FROM Clients WHERE Id = @clientId";
                using (var cmd = new SqliteCommand(deleteClient, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@clientId", clientId);
                    cmd.ExecuteNonQuery();
                }

                // Log the deletion
                LogClientAction(connection, transaction, "DELETE", clientId, userId, $"Client deleted: {client.Name}");

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<Client> SearchClients(string searchTerm)
        {
            var clients = new List<Client>();
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                SELECT c.*, w.Username as CreatedByName, w2.Username as UpdatedByName
                FROM Clients c
                LEFT JOIN Workers w ON c.CreatedBy = w.Id
                LEFT JOIN Workers w2 ON c.UpdatedBy = w2.Id
                WHERE c.Name LIKE @search 
                   OR c.SSN LIKE @search 
                   OR c.Email LIKE @search 
                   OR c.Phone LIKE @search
                ORDER BY c.Name";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                clients.Add(MapReaderToClient(reader));
            }

            return clients;
        }

        public void UpdateExtraData(int clientId, string fieldName, object value, int userId)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Get current ExtraData
                string getSql = "SELECT ExtraData FROM Clients WHERE Id = @id";
                using var getCmd = new SqliteCommand(getSql, connection, transaction);
                getCmd.Parameters.AddWithValue("@id", clientId);

                string extraDataJson = getCmd.ExecuteScalar()?.ToString() ?? "{}";
                var extraData = JsonConvert.DeserializeObject<Dictionary<string, object>>(extraDataJson)
                    ?? new Dictionary<string, object>();

                // Track old value
                object oldValue = extraData.ContainsKey(fieldName) ? extraData[fieldName] : null;

                // Update value
                extraData[fieldName] = value;

                // Save back to database
                string updateSql = @"
                    UPDATE Clients 
                    SET ExtraData = @extraData, 
                        UpdatedBy = @userId, 
                        LastUpdated = CURRENT_TIMESTAMP 
                    WHERE Id = @id";

                using var updateCmd = new SqliteCommand(updateSql, connection, transaction);

                // Serialize with explicit settings to preserve empty strings
                string updatedJson = JsonConvert.SerializeObject(
                    extraData,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        DefaultValueHandling = DefaultValueHandling.Include
                    });

                updateCmd.Parameters.AddWithValue("@extraData", updatedJson);
                updateCmd.Parameters.AddWithValue("@userId", userId);
                updateCmd.Parameters.AddWithValue("@id", clientId);
                updateCmd.ExecuteNonQuery();

                // Track field history
                TrackSingleFieldChange(connection, transaction, clientId, fieldName, oldValue, value, userId, "UPDATE");

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private void TrackFieldChanges(SqliteConnection connection, SqliteTransaction transaction,
            Client oldClient, Client newClient, int userId)
        {
            // Track standard field changes
            if (oldClient.Name != newClient.Name)
                TrackSingleFieldChange(connection, transaction, newClient.Id, "Name", oldClient.Name, newClient.Name, userId, "UPDATE");

            if (oldClient.Phone != newClient.Phone)
                TrackSingleFieldChange(connection, transaction, newClient.Id, "Phone", oldClient.Phone, newClient.Phone, userId, "UPDATE");

            if (oldClient.Email != newClient.Email)
                TrackSingleFieldChange(connection, transaction, newClient.Id, "Email", oldClient.Email, newClient.Email, userId, "UPDATE");

            if (oldClient.DOB != newClient.DOB)
                TrackSingleFieldChange(connection, transaction, newClient.Id, "DOB",
                    oldClient.DOB?.ToString("yyyy-MM-dd"),
                    newClient.DOB?.ToString("yyyy-MM-dd"),
                    userId, "UPDATE");

            // Track ExtraData changes
            var oldExtra = oldClient.ExtraData ?? new Dictionary<string, object>();
            var newExtra = newClient.ExtraData ?? new Dictionary<string, object>();

            foreach (var kvp in newExtra)
            {
                if (!oldExtra.ContainsKey(kvp.Key) || !Equals(oldExtra[kvp.Key], kvp.Value))
                {
                    var oldValue = oldExtra.ContainsKey(kvp.Key) ? oldExtra[kvp.Key] : null;
                    TrackSingleFieldChange(connection, transaction, newClient.Id, kvp.Key, oldValue, kvp.Value, userId, "UPDATE");
                }
            }
        }

        private void TrackSingleFieldChange(SqliteConnection connection, SqliteTransaction transaction,
            int clientId, string fieldName, object oldValue, object newValue, int userId, string source)
        {
            string sql = @"
                INSERT INTO FieldHistory (ClientId, FieldName, OldValue, NewValue, ChangedBy, ChangeSource)
                VALUES (@clientId, @fieldName, @oldValue, @newValue, @userId, @source)";

            using var cmd = new SqliteCommand(sql, connection, transaction);
            cmd.Parameters.AddWithValue("@clientId", clientId);
            cmd.Parameters.AddWithValue("@fieldName", fieldName);
            cmd.Parameters.AddWithValue("@oldValue", oldValue?.ToString() ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@newValue", newValue?.ToString() ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@source", source);
            cmd.ExecuteNonQuery();
        }

        private void LogClientAction(SqliteConnection connection, SqliteTransaction transaction,
            string actionType, int clientId, int userId, string description)
        {
            string sql = @"
                INSERT INTO AuditLog (User, UserId, ActionType, Entity, EntityId, Description, Metadata)
                SELECT @user, @userId, @actionType, 'Client', @entityId, @description, '{}'
                FROM Workers WHERE Id = @userId";

            using var cmd = new SqliteCommand(sql, connection, transaction);
            cmd.Parameters.AddWithValue("@user", GetUsername(userId));
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@actionType", actionType);
            cmd.Parameters.AddWithValue("@entityId", clientId);
            cmd.Parameters.AddWithValue("@description", description);
            cmd.ExecuteNonQuery();
        }

        private string GetUsername(int userId)
        {
            return DbConnection.ExecuteScalar<string>(
                "SELECT Username FROM Workers WHERE Id = @id",
                new SqliteParameter("@id", userId)
            ) ?? "Unknown";
        }

        private Client MapReaderToClient(SqliteDataReader reader)
        {
            var client = new Client
            {
                Id = Convert.ToInt32(reader["Id"]),
                SSN = reader["SSN"].ToString(),
                Name = reader["Name"].ToString(),
                Phone = reader["Phone"]?.ToString(),
                Email = reader["Email"]?.ToString(),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                LastUpdated = Convert.ToDateTime(reader["LastUpdated"])
            };

            // Map Notes field (may not exist in older DBs)
            if (HasColumn(reader, "Notes") && reader["Notes"] != DBNull.Value)
            {
                client.Notes = reader["Notes"].ToString();
            }

            if (reader["DOB"] != DBNull.Value)
            {
                client.DOB = Convert.ToDateTime(reader["DOB"]);
            }

            if (reader["ExtraData"] != DBNull.Value)
            {
                string extraDataJson = reader["ExtraData"].ToString();
                System.Diagnostics.Debug.WriteLine($"MapReaderToClient: Client {client.Id} ExtraData JSON from DB = '{extraDataJson}'");

                client.ExtraData = JsonConvert.DeserializeObject<Dictionary<string, object>>(extraDataJson);

                System.Diagnostics.Debug.WriteLine($"MapReaderToClient: Client {client.Id} after deserialize has {client.ExtraData?.Count ?? 0} entries");
                if (client.ExtraData != null)
                {
                    foreach (var kvp in client.ExtraData)
                    {
                        System.Diagnostics.Debug.WriteLine($"  '{kvp.Key}' = '{kvp.Value}' (Type: {kvp.Value?.GetType().Name})");
                    }
                }
            }

            return client;
            // Map upload link fields (may not exist in older DBs)

            try

            {

                if (HasColumn(reader, "UploadToken") && reader["UploadToken"] != DBNull.Value)

                {

                    client.UploadToken = reader["UploadToken"].ToString();

                }



                if (HasColumn(reader, "UploadLinkExpires") && reader["UploadLinkExpires"] != DBNull.Value)

                {

                    client.UploadLinkExpires = Convert.ToDateTime(reader["UploadLinkExpires"]);

                }

            }

            catch { /* Column may not exist yet */ }



            return client;

        }



        private bool HasColumn(SqliteDataReader reader, string columnName)

        {

            for (int i = 0; i < reader.FieldCount; i++)

            {

                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))

                    return true;

            }

            return false;
        }
    }
}