using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace ReactCRM.Database
{
    /// <summary>
    /// One-time migration to fix custom field key mismatches in ExtraData
    /// </summary>
    public static class MigrateCustomFieldKeys
    {
        private const string MIGRATION_NAME = "FixCustomFieldKeys_v1";

        public static void Execute()
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            // Create migrations table if it doesn't exist
            EnsureMigrationsTable(connection);

            // Check if migration has already been run
            if (HasMigrationBeenRun(connection, MIGRATION_NAME))
            {
                System.Diagnostics.Debug.WriteLine($"Migration '{MIGRATION_NAME}' already completed, skipping.");
                return;
            }

            System.Diagnostics.Debug.WriteLine("Starting custom field key migration...");

            // Define the key mappings (old key -> new key)
            var keyMappings = new Dictionary<string, string>
            {
                { "YEARS", "YEAR" },
                { "SP FIRST NAME", "SP_FIRST_NAME" },
                { "SP LAST NAME", "SP_LAST_NAME" }
            };

            // Get all clients with ExtraData
            string selectSql = "SELECT Id, ExtraData FROM Clients WHERE ExtraData IS NOT NULL AND ExtraData != '{}'";
            var clientsToUpdate = new List<(int Id, string ExtraData)>();

            using (var cmd = new SqliteCommand(selectSql, connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string extraData = reader.GetString(1);
                    clientsToUpdate.Add((id, extraData));
                }
            }

            System.Diagnostics.Debug.WriteLine($"Found {clientsToUpdate.Count} clients with ExtraData");

            int updatedCount = 0;
            foreach (var (id, extraDataJson) in clientsToUpdate)
            {
                try
                {
                    var extraData = JsonConvert.DeserializeObject<Dictionary<string, object>>(extraDataJson);
                    if (extraData == null || extraData.Count == 0)
                        continue;

                    bool modified = false;
                    var newExtraData = new Dictionary<string, object>();

                    // Copy all entries, renaming keys as needed
                    foreach (var kvp in extraData)
                    {
                        if (keyMappings.ContainsKey(kvp.Key))
                        {
                            // Use new key name
                            string newKey = keyMappings[kvp.Key];
                            newExtraData[newKey] = kvp.Value;
                            System.Diagnostics.Debug.WriteLine($"  Client {id}: Renaming '{kvp.Key}' -> '{newKey}' (value: {kvp.Value})");
                            modified = true;
                        }
                        else if (!keyMappings.ContainsValue(kvp.Key))
                        {
                            // Keep as-is (but don't copy if it's a new key that would conflict)
                            newExtraData[kvp.Key] = kvp.Value;
                        }
                        // If it's already the new key name, prefer it over the old one
                        else if (keyMappings.ContainsValue(kvp.Key))
                        {
                            // This is already the new key, keep it
                            newExtraData[kvp.Key] = kvp.Value;
                        }
                    }

                    if (modified)
                    {
                        // Update the database
                        string updateSql = "UPDATE Clients SET ExtraData = @extraData WHERE Id = @id";
                        using var updateCmd = new SqliteCommand(updateSql, connection);
                        updateCmd.Parameters.AddWithValue("@extraData", JsonConvert.SerializeObject(newExtraData));
                        updateCmd.Parameters.AddWithValue("@id", id);
                        updateCmd.ExecuteNonQuery();

                        updatedCount++;
                        System.Diagnostics.Debug.WriteLine($"  Client {id}: Updated successfully");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"  Client {id}: ERROR - {ex.Message}");
                }
            }

            // Mark migration as complete
            MarkMigrationComplete(connection, MIGRATION_NAME);

            System.Diagnostics.Debug.WriteLine($"Migration complete. Updated {updatedCount} clients.");
        }

        private static void EnsureMigrationsTable(SqliteConnection connection)
        {
            string createTableSql = @"
                CREATE TABLE IF NOT EXISTS Migrations (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL UNIQUE,
                    ExecutedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                )";

            using var cmd = new SqliteCommand(createTableSql, connection);
            cmd.ExecuteNonQuery();
        }

        private static bool HasMigrationBeenRun(SqliteConnection connection, string migrationName)
        {
            string checkSql = "SELECT COUNT(*) FROM Migrations WHERE Name = @name";
            using var cmd = new SqliteCommand(checkSql, connection);
            cmd.Parameters.AddWithValue("@name", migrationName);
            long count = (long)cmd.ExecuteScalar();
            return count > 0;
        }

        private static void MarkMigrationComplete(SqliteConnection connection, string migrationName)
        {
            string insertSql = "INSERT INTO Migrations (Name) VALUES (@name)";
            using var cmd = new SqliteCommand(insertSql, connection);
            cmd.Parameters.AddWithValue("@name", migrationName);
            cmd.ExecuteNonQuery();
        }
    }
}
