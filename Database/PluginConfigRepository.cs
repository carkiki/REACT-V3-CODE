using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using ReactCRM.Models;

namespace ReactCRM.Database
{
    /// <summary>
    /// Repositorio para gestionar la configuración de plugins
    /// </summary>
    public class PluginConfigRepository
    {
        public List<PluginConfig> GetAllPluginConfigs()
        {
            var configs = new List<PluginConfig>();

            using (var connection = DbConnection.GetConnection())
            {
                connection.Open();

                var sql = "SELECT * FROM PluginConfig ORDER BY LoadOrder, PluginName";

                using (var command = new SqliteCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        configs.Add(MapFromReader(reader));
                    }
                }
            }

            return configs;
        }

        public PluginConfig GetPluginConfig(string pluginName)
        {
            using (var connection = DbConnection.GetConnection())
            {
                connection.Open();

                var sql = "SELECT * FROM PluginConfig WHERE PluginName = @PluginName";

                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PluginName", pluginName);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapFromReader(reader);
                        }
                    }
                }
            }

            return null;
        }

        public void SavePluginConfig(PluginConfig config)
        {
            using (var connection = DbConnection.GetConnection())
            {
                connection.Open();

                var existing = GetPluginConfig(config.PluginName);

                if (existing != null)
                {
                    // Update existing
                    var sql = @"UPDATE PluginConfig SET
                                DllPath = @DllPath,
                                IsEnabled = @IsEnabled,
                                AutoStart = @AutoStart,
                                LoadOrder = @LoadOrder,
                                LastLoaded = @LastLoaded
                                WHERE PluginName = @PluginName";

                    using (var command = new SqliteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@PluginName", config.PluginName);
                        command.Parameters.AddWithValue("@DllPath", config.DllPath);
                        command.Parameters.AddWithValue("@IsEnabled", config.IsEnabled ? 1 : 0);
                        command.Parameters.AddWithValue("@AutoStart", config.AutoStart ? 1 : 0);
                        command.Parameters.AddWithValue("@LoadOrder", config.LoadOrder);
                        command.Parameters.AddWithValue("@LastLoaded",
                            config.LastLoaded.HasValue ? config.LastLoaded.Value.ToString("yyyy-MM-dd HH:mm:ss") : DBNull.Value);

                        command.ExecuteNonQuery();
                    }
                }
                else
                {
                    // Insert new
                    var sql = @"INSERT INTO PluginConfig (PluginName, DllPath, IsEnabled, AutoStart, LoadOrder, CreatedDate)
                                VALUES (@PluginName, @DllPath, @IsEnabled, @AutoStart, @LoadOrder, @CreatedDate)";

                    using (var command = new SqliteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@PluginName", config.PluginName);
                        command.Parameters.AddWithValue("@DllPath", config.DllPath);
                        command.Parameters.AddWithValue("@IsEnabled", config.IsEnabled ? 1 : 0);
                        command.Parameters.AddWithValue("@AutoStart", config.AutoStart ? 1 : 0);
                        command.Parameters.AddWithValue("@LoadOrder", config.LoadOrder);
                        command.Parameters.AddWithValue("@CreatedDate", config.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"));

                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public void DeletePluginConfig(string pluginName)
        {
            using (var connection = DbConnection.GetConnection())
            {
                connection.Open();

                var sql = "DELETE FROM PluginConfig WHERE PluginName = @PluginName";

                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PluginName", pluginName);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<PluginConfig> GetAutoStartPlugins()
        {
            var configs = new List<PluginConfig>();

            using (var connection = DbConnection.GetConnection())
            {
                connection.Open();

                var sql = "SELECT * FROM PluginConfig WHERE AutoStart = 1 AND IsEnabled = 1 ORDER BY LoadOrder";

                using (var command = new SqliteCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        configs.Add(MapFromReader(reader));
                    }
                }
            }

            return configs;
        }

        private PluginConfig MapFromReader(SqliteDataReader reader)
        {
            return new PluginConfig
            {
                Id = reader.GetInt32(0),
                PluginName = reader.GetString(1),
                DllPath = reader.GetString(2),
                IsEnabled = reader.GetInt32(3) == 1,
                AutoStart = reader.GetInt32(4) == 1,
                LoadOrder = reader.GetInt32(5),
                LastLoaded = reader.IsDBNull(6) ? null : DateTime.Parse(reader.GetString(6)),
                CreatedDate = DateTime.Parse(reader.GetString(7))
            };
        }
    }
}