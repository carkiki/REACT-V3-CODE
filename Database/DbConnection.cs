using System;
using Microsoft.Data.Sqlite;
using File = System.IO.File;
using FileNotFoundException = System.IO.FileNotFoundException;

namespace ReactCRM.Database
{
    public static class DbConnection
    {
        private static readonly string DB_FILE = "crm.db";
        // Connection string with timeout in seconds
        private static readonly string CONNECTION_STRING = $"Data Source={DB_FILE};Foreign Keys=True;Default Timeout=30;";
        private static readonly object _lock = new object();
        private static bool _walModeEnabled = false;

        public static SqliteConnection GetConnection()
        {
            lock (_lock)
            {
                if (!File.Exists(DB_FILE))
                {
                    throw new FileNotFoundException($"Database file not found: {DB_FILE}");
                }

                var connection = new SqliteConnection(CONNECTION_STRING);

                // Enable WAL mode on first connection for better concurrency
                if (!_walModeEnabled)
                {
                    try
                    {
                        connection.Open();
                        using var cmd = new SqliteCommand("PRAGMA journal_mode=WAL;", connection);
                        cmd.ExecuteNonQuery();
                        _walModeEnabled = true;
                        connection.Close();
                    }
                    catch
                    {
                        // Ignore if WAL mode can't be enabled
                    }
                }

                return connection;
            }
        }

        /// <summary>
        /// Gets the connection string for creating independent connections
        /// </summary>
        public static string GetConnectionString()
        {
            return CONNECTION_STRING;
        }

        public static void ExecuteInTransaction(Action<SqliteTransaction> action)
        {
            using var connection = GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                action(transaction);
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        public static T ExecuteScalar<T>(string sql, params SqliteParameter[] parameters)
        {
            using var connection = GetConnection();
            connection.Open();
            using var cmd = new SqliteCommand(sql, connection);

            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }

            var result = cmd.ExecuteScalar();
            return result == null || result == DBNull.Value ? default(T) : (T)result;
        }

        public static int ExecuteNonQuery(string sql, params SqliteParameter[] parameters)
        {
            using var connection = GetConnection();
            connection.Open();
            using var cmd = new SqliteCommand(sql, connection);

            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }

            return cmd.ExecuteNonQuery();
        }

        public static bool TestConnection()
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();
                using var cmd = new SqliteCommand("SELECT 1", connection);
                cmd.ExecuteScalar();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}