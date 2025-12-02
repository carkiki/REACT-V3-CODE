using System;

using System.IO;

using System.Security.Cryptography;

using System.Text;

using Microsoft.Data.Sqlite;



namespace ReactCRM.Services

{

    /// <summary>

    /// Manages database encryption using SQLCipher with license-based key derivation

    /// </summary>

    public class DatabaseEncryptionService

    {

        private const string DB_FILE = "crm.db";

        private const string ENCRYPTION_KEY_FILE = ".dbkey";

        private static string? _encryptionKey;

        private static readonly object _lock = new();



        /// <summary>

        /// Initialize SQLCipher bundle

        /// </summary>

        public static void InitializeSQLCipher()

        {

            try

            {

                // Load SQLCipher native library

                // Try V2 first (for newer Microsoft.Data.Sqlite)

                SQLitePCL.Batteries_V2.Init();

            }

            catch

            {

                try

                {

                    // Fall back to regular Batteries if V2 fails

                    // This handles the bundle_sqlcipher package

                    var initMethod = typeof(SQLitePCL.raw).Assembly

                        .GetType("SQLitePCL.Batteries")?.GetMethod("Init");

                    initMethod?.Invoke(null, null);

                }

                catch

                {

                    // If both fail, SQLite might already be initialized

                    // or initialization might not be needed

                    System.Diagnostics.Debug.WriteLine("SQLCipher initialization skipped - may already be initialized");

                }

            }

        }



        /// <summary>

        /// Get or generate the database encryption key

        /// </summary>

        public static string GetEncryptionKey()

        {

            lock (_lock)

            {

                if (_encryptionKey != null)

                    return _encryptionKey;



                // Check if encryption key file exists

                if (File.Exists(ENCRYPTION_KEY_FILE))

                {

                    try

                    {

                        // Read existing key (stored as base64)

                        _encryptionKey = File.ReadAllText(ENCRYPTION_KEY_FILE).Trim();



                        if (!string.IsNullOrWhiteSpace(_encryptionKey))

                            return _encryptionKey;

                    }

                    catch (Exception ex)

                    {

                        System.Diagnostics.Debug.WriteLine($"Error reading encryption key: {ex.Message}");

                    }

                }



                // Generate new encryption key

                _encryptionKey = GenerateEncryptionKey();



                // Save encryption key

                try

                {

                    File.WriteAllText(ENCRYPTION_KEY_FILE, _encryptionKey);



                    // Hide the key file

                    FileInfo keyFile = new FileInfo(ENCRYPTION_KEY_FILE);

                    keyFile.Attributes = FileAttributes.Hidden | FileAttributes.System;

                }

                catch (Exception ex)

                {

                    System.Diagnostics.Debug.WriteLine($"Error saving encryption key: {ex.Message}");

                }



                return _encryptionKey;

            }

        }



        /// <summary>

        /// Generate a secure random encryption key

        /// </summary>

        private static string GenerateEncryptionKey()

        {

            // Generate 256-bit (32-byte) random key

            byte[] keyBytes = new byte[32];

            using (var rng = RandomNumberGenerator.Create())

            {

                rng.GetBytes(keyBytes);

            }



            // Convert to hex string (SQLCipher compatible format)

            return "x'" + BitConverter.ToString(keyBytes).Replace("-", "") + "'";

        }



        /// <summary>

        /// Get connection string with encryption

        /// </summary>

        public static string GetEncryptedConnectionString()

        {

            string key = GetEncryptionKey();

            return $"Data Source={DB_FILE};Mode=ReadWriteCreate;Password={key};";

        }



        /// <summary>

        /// Create an encrypted database connection

        /// </summary>

        public static SqliteConnection CreateEncryptedConnection()

        {

            var connection = new SqliteConnection(GetEncryptedConnectionString());

            return connection;

        }



        /// <summary>

        /// Encrypt an existing unencrypted database

        /// </summary>

        public static bool EncryptExistingDatabase()

        {

            try

            {

                if (!File.Exists(DB_FILE))

                {

                    System.Diagnostics.Debug.WriteLine("Database file not found.");

                    return false;

                }



                // Create backup first

                string backupFile = $"{DB_FILE}.backup_{DateTime.Now:yyyyMMdd_HHmmss}";

                File.Copy(DB_FILE, backupFile, true);

                System.Diagnostics.Debug.WriteLine($"Backup created: {backupFile}");



                // Generate encryption key

                string encryptionKey = GetEncryptionKey();



                // Create new encrypted database

                string tempEncryptedDb = $"{DB_FILE}.encrypted";



                // Export unencrypted database to encrypted one

                using (var sourceConn = new SqliteConnection($"Data Source={DB_FILE};"))

                {

                    sourceConn.Open();



                    // Attach the new encrypted database

                    using (var cmd = sourceConn.CreateCommand())

                    {

                        cmd.CommandText = $"ATTACH DATABASE '{tempEncryptedDb}' AS encrypted KEY {encryptionKey};";

                        cmd.ExecuteNonQuery();

                    }



                    // Export schema and data

                    using (var cmd = sourceConn.CreateCommand())

                    {

                        cmd.CommandText = "SELECT sqlcipher_export('encrypted');";

                        cmd.ExecuteNonQuery();

                    }



                    // Detach

                    using (var cmd = sourceConn.CreateCommand())

                    {

                        cmd.CommandText = "DETACH DATABASE encrypted;";

                        cmd.ExecuteNonQuery();

                    }

                }



                // Replace original database with encrypted one

                File.Delete(DB_FILE);

                File.Move(tempEncryptedDb, DB_FILE);



                System.Diagnostics.Debug.WriteLine("Database encrypted successfully!");

                return true;

            }

            catch (Exception ex)

            {

                System.Diagnostics.Debug.WriteLine($"Error encrypting database: {ex.Message}\n{ex.StackTrace}");

                return false;

            }

        }



        /// <summary>

        /// Verify if database is encrypted and accessible

        /// </summary>

        public static bool VerifyEncryption()

        {

            try

            {

                using (var conn = CreateEncryptedConnection())

                {

                    conn.Open();



                    using (var cmd = conn.CreateCommand())

                    {

                        cmd.CommandText = "SELECT count(*) FROM sqlite_master;";

                        cmd.ExecuteScalar();

                    }

                }



                return true;

            }

            catch (Exception ex)

            {

                System.Diagnostics.Debug.WriteLine($"Encryption verification failed: {ex.Message}");

                return false;

            }

        }



        /// <summary>

        /// Check if database appears to be encrypted

        /// </summary>

        public static bool IsDatabaseEncrypted()

        {

            try

            {

                if (!File.Exists(DB_FILE))

                    return false;



                // Try to open without encryption

                using (var conn = new SqliteConnection($"Data Source={DB_FILE};"))

                {

                    conn.Open();



                    using (var cmd = conn.CreateCommand())

                    {

                        cmd.CommandText = "SELECT count(*) FROM sqlite_master;";

                        cmd.ExecuteScalar();

                    }

                }



                // If we get here, database is NOT encrypted

                return false;

            }

            catch (SqliteException)

            {

                // If opening fails, it's likely encrypted (or corrupted)

                // Try with encryption to verify

                return VerifyEncryption();

            }

        }

    }

}