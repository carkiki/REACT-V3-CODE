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
        // Using BusyTimeout to handle multiple concurrent connections
        private static readonly string CONNECTION_STRING = $"Data Source={DB_FILE};Foreign Keys=True;Default Timeout=120;";
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

                    // Enable appropriate journal mode based on location
                    // WAL mode doesn't work on network drives, use DELETE mode instead
                    if (!_walModeEnabled)
                    {
                        try
                        {
                            connection.Open();

                            // Check if database is on a network path
                            bool isNetworkPath = IsNetworkPath(System.IO.Path.GetFullPath(DB_FILE));

                            if (isNetworkPath)
                            {
                                // Use DELETE journal mode for network paths (more compatible)
                                // Set busy_timeout to handle concurrent access better
                                using var cmd = new SqliteCommand(@"
                                    PRAGMA journal_mode=DELETE;
                                    PRAGMA synchronous=NORMAL;
                                    PRAGMA busy_timeout=30000;", connection);
                                var result = cmd.ExecuteScalar();
                                _logger.LogInfo($"Network path detected. Journal mode set to DELETE with 30s busy timeout: {result}", "DbConnection.GetConnection");
                            }
                            else
                            {
                                // Use WAL mode for local paths (better concurrency)
                                using var cmd = new SqliteCommand("PRAGMA journal_mode=WAL;", connection);
                                var result = cmd.ExecuteScalar();
                                _logger.LogInfo($"Local path detected. WAL mode enabled: {result}", "DbConnection.GetConnection");
                            }

                            _walModeEnabled = true;
                            connection.Close();
                        }
                        catch (Exception journalEx)
                        {
                            _logger.LogWarning($"Could not set journal mode: {journalEx.Message}", "DbConnection.GetConnection");
                            // Continue without specific journal mode - SQLite will use default
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
        /// Checks if a path is on a network drive (UNC path or mapped network drive)
        /// </summary>
        private static bool IsNetworkPath(string path)
        {
            try
            {
                // Check for UNC path (\\server\share)
                if (path.StartsWith(@"\\") || path.StartsWith(@"//"))
                {
                    return true;
                }

                // Check if the drive is a network drive
                var root = System.IO.Path.GetPathRoot(path);
                if (string.IsNullOrEmpty(root))
                {
                    return false;
                }

                var driveInfo = new System.IO.DriveInfo(root);
                return driveInfo.DriveType == System.IO.DriveType.Network;
            }
            catch
            {
                // If we can't determine, assume it's local (safer default)
                return false;
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

                if (result == null || result == DBNull.Value)
                    return default(T);

                // Handle SQLite Int64 to Int32 conversion for COUNT queries
                if (typeof(T) == typeof(int) && result is long longValue)
                {
                    return (T)(object)(int)longValue;
                }

                // Handle other numeric conversions
                if (typeof(T) == typeof(int) && result is Int64)
                {
                    return (T)Convert.ChangeType(result, typeof(int));
                }

                return (T)result;
            }
            catch (SqliteException sqlEx)
            {
                _logger.LogError($"Database error executing scalar query: {sql}", sqlEx, "DbConnection.ExecuteScalar");
                throw;
            }
            catch (InvalidCastException castEx)
            {
                _logger.LogError($"Cast error in scalar query. SQL: {sql}, Expected type: {typeof(T).Name}", castEx, "DbConnection.ExecuteScalar");
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