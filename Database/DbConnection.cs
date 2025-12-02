using System;
using Microsoft.Data.Sqlite;
using ReactCRM.Services;
using File = System.IO.File;
using FileNotFoundException = System.IO.FileNotFoundException;

namespace ReactCRM.Database
{
    public static class DbConnection
    {
        private static readonly string DB_FILE = "crm.db";
        // Connection string with timeout in seconds - increased for network scenarios
        private static readonly string CONNECTION_STRING = $"Data Source={DB_FILE};Foreign Keys=True;Default Timeout=60;";
        private static readonly object _lock = new object();
        private static bool _walModeEnabled = false;
        private static readonly ErrorLogger _logger = ErrorLogger.Instance;

        public static SqliteConnection GetConnection()
        {
            lock (_lock)
            {
                try
                {
                    if (!File.Exists(DB_FILE))
                    {
                        _logger.LogError($"Database file not found: {DB_FILE}", null, "DbConnection.GetConnection");
                        throw new FileNotFoundException($"Database file not found: {DB_FILE}");
                    }

                    var connection = new SqliteConnection(CONNECTION_STRING);

                    // Enable WAL mode on first connection for better concurrency
                    // WAL mode allows multiple readers and one writer simultaneously
                    if (!_walModeEnabled)
                    {
                        try
                        {
                            connection.Open();
                            using var cmd = new SqliteCommand("PRAGMA journal_mode=WAL;", connection);
                            var result = cmd.ExecuteScalar();
                            _walModeEnabled = true;
                            _logger.LogInfo($"WAL mode enabled: {result}", "DbConnection.GetConnection");
                            connection.Close();
                        }
                        catch (Exception walEx)
                        {
                            _logger.LogWarning($"Could not enable WAL mode: {walEx.Message}", "DbConnection.GetConnection");
                            // Continue without WAL mode - not critical but reduces concurrency
                        }
                    }

                    return connection;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to create database connection", ex, "DbConnection.GetConnection");
                    throw;
                }
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
            try
            {
                using var connection = GetConnection();
                connection.Open();
                using var transaction = connection.BeginTransaction();

                try
                {
                    action(transaction);
                    transaction.Commit();
                }
                catch (Exception transEx)
                {
                    transaction.Rollback();
                    _logger.LogError("Transaction rolled back due to error", transEx, "DbConnection.ExecuteInTransaction");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to execute transaction", ex, "DbConnection.ExecuteInTransaction");
                throw;
            }
        }

        public static T ExecuteScalar<T>(string sql, params SqliteParameter[] parameters)
        {
            try
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
            catch (SqliteException sqlEx)
            {
                _logger.LogError($"Database error executing scalar query: {sql}", sqlEx, "DbConnection.ExecuteScalar");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error executing scalar query: {sql}", ex, "DbConnection.ExecuteScalar");
                throw;
            }
        }

        public static int ExecuteNonQuery(string sql, params SqliteParameter[] parameters)
        {
            try
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
            catch (SqliteException sqlEx)
            {
                _logger.LogError($"Database error executing non-query: {sql}", sqlEx, "DbConnection.ExecuteNonQuery");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error executing non-query: {sql}", ex, "DbConnection.ExecuteNonQuery");
                throw;
            }
        }

        public static bool TestConnection()
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();
                using var cmd = new SqliteCommand("SELECT 1", connection);
                cmd.ExecuteScalar();
                _logger.LogInfo("Database connection test successful", "DbConnection.TestConnection");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Database connection test failed", ex, "DbConnection.TestConnection");
                return false;
            }
        }
    }
}