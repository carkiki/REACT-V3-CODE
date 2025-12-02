using System;
using Microsoft.Data.Sqlite;
using ReactCRM.Database;

namespace ReactCRM.Services
{
    /// <summary>
    /// Inicializa la tabla de configuración de plugins
    /// </summary>
    public static class PluginDatabaseInitializer
    {
        public static void InitializePluginDatabase()
        {
            using (var connection = DbConnection.GetConnection())
            {
                connection.Open();

                var createTableSql = @"
                    CREATE TABLE IF NOT EXISTS PluginConfig (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        PluginName TEXT NOT NULL UNIQUE,
                        DllPath TEXT NOT NULL,
                        IsEnabled INTEGER NOT NULL DEFAULT 1,
                        AutoStart INTEGER NOT NULL DEFAULT 0,
                        LoadOrder INTEGER NOT NULL DEFAULT 0,
                        LastLoaded TEXT,
                        CreatedDate TEXT NOT NULL,
                        UNIQUE(PluginName)
                    );
                ";

                using (var command = new SqliteCommand(createTableSql, connection))
                {
                    command.ExecuteNonQuery();
                }

                System.Diagnostics.Debug.WriteLine("[PluginDB] Plugin configuration table initialized");
            }
        }
    }
}