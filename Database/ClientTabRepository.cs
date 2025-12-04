using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using ReactCRM.Models;

namespace ReactCRM.Database
{
    public class ClientTabRepository
    {
        public List<ClientTab> GetAllTabs()
        {
            var tabs = new List<ClientTab>();

            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqliteCommand(@"
                    SELECT Id, Name, DisplayOrder, IsDefault, CreatedAt, CreatedBy
                    FROM ClientTabs
                    ORDER BY DisplayOrder, Id", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tabs.Add(new ClientTab
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                DisplayOrder = reader.GetInt32(2),
                                IsDefault = reader.GetBoolean(3),
                                CreatedAt = reader.GetDateTime(4),
                                CreatedBy = reader.GetInt32(5)
                            });
                        }
                    }
                }
            }

            return tabs;
        }

        public ClientTab GetTabById(int tabId)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqliteCommand(@"
                    SELECT Id, Name, DisplayOrder, IsDefault, CreatedAt, CreatedBy
                    FROM ClientTabs
                    WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", tabId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new ClientTab
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                DisplayOrder = reader.GetInt32(2),
                                IsDefault = reader.GetBoolean(3),
                                CreatedAt = reader.GetDateTime(4),
                                CreatedBy = reader.GetInt32(5)
                            };
                        }
                    }
                }
            }

            return null;
        }

        public int CreateTab(string name, int userId)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();

                // Get max display order
                int maxOrder = 0;
                using (var cmd = new SqliteCommand("SELECT COALESCE(MAX(DisplayOrder), 0) FROM ClientTabs", conn))
                {
                    maxOrder = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Insert new tab
                using (var cmd = new SqliteCommand(@"
                    INSERT INTO ClientTabs (Name, DisplayOrder, IsDefault, CreatedAt, CreatedBy)
                    VALUES (@name, @order, 0, @createdAt, @userId);
                    SELECT last_insert_rowid();", conn))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@order", maxOrder + 1);
                    cmd.Parameters.AddWithValue("@createdAt", DateTime.Now);
                    cmd.Parameters.AddWithValue("@userId", userId);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void UpdateTabName(int tabId, string newName)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqliteCommand(@"
                    UPDATE ClientTabs
                    SET Name = @name
                    WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@name", newName);
                    cmd.Parameters.AddWithValue("@id", tabId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteTab(int tabId)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();

                // Check if it's the default tab
                using (var cmd = new SqliteCommand("SELECT IsDefault FROM ClientTabs WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", tabId);
                    var isDefault = Convert.ToBoolean(cmd.ExecuteScalar());
                    if (isDefault)
                    {
                        throw new InvalidOperationException("Cannot delete the default tab.");
                    }
                }

                // Get default tab ID
                int defaultTabId = 0;
                using (var cmd = new SqliteCommand("SELECT Id FROM ClientTabs WHERE IsDefault = 1 LIMIT 1", conn))
                {
                    defaultTabId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Move all clients from this tab to default tab
                using (var cmd = new SqliteCommand(@"
                    UPDATE Clients
                    SET TabId = @defaultTabId
                    WHERE TabId = @tabId", conn))
                {
                    cmd.Parameters.AddWithValue("@defaultTabId", defaultTabId);
                    cmd.Parameters.AddWithValue("@tabId", tabId);
                    cmd.ExecuteNonQuery();
                }

                // Delete the tab
                using (var cmd = new SqliteCommand("DELETE FROM ClientTabs WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", tabId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateTabOrder(int tabId, int newOrder)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqliteCommand(@"
                    UPDATE ClientTabs
                    SET DisplayOrder = @order
                    WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@order", newOrder);
                    cmd.Parameters.AddWithValue("@id", tabId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void EnsureDefaultTabExists(int userId)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();

                // Check if default tab exists
                using (var cmd = new SqliteCommand("SELECT COUNT(*) FROM ClientTabs WHERE IsDefault = 1", conn))
                {
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    if (count == 0)
                    {
                        // Create default tab
                        using (var insertCmd = new SqliteCommand(@"
                            INSERT INTO ClientTabs (Name, DisplayOrder, IsDefault, CreatedAt, CreatedBy)
                            VALUES ('All Clients', 0, 1, @createdAt, @userId)", conn))
                        {
                            insertCmd.Parameters.AddWithValue("@createdAt", DateTime.Now);
                            insertCmd.Parameters.AddWithValue("@userId", userId);
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public int GetClientCount(int tabId)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqliteCommand(@"
                    SELECT COUNT(*)
                    FROM Clients
                    WHERE TabId = @tabId AND IsActive = 1", conn))
                {
                    cmd.Parameters.AddWithValue("@tabId", tabId);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }
    }
}
