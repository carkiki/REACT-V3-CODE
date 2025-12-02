using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using ReactCRM.Database;
using ReactCRM.Models;
using Newtonsoft.Json;

namespace ReactCRM.Database
{
    public class CustomFieldRepository
    {
        public List<CustomField> GetAll()
        {
            var fields = new List<CustomField>();

            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();

                string sql = @"
                    SELECT Id, FieldName, Label, FieldType, Options, IsRequired, DefaultValue, CreatedAt
                    FROM CustomFields
                    ORDER BY Id";

                using (var cmd = new SqliteCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        fields.Add(new CustomField
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            FieldName = reader["FieldName"].ToString(),
                            Label = reader["Label"].ToString(),
                            FieldType = reader["FieldType"].ToString(),
                            Options = reader["Options"]?.ToString(),
                            IsRequired = Convert.ToBoolean(reader["IsRequired"]),
                            DefaultValue = reader["DefaultValue"]?.ToString(),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                        });
                    }
                }
            }

            return fields;
        }

        public CustomField GetByFieldName(string fieldName)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();

                string sql = @"
                    SELECT Id, FieldName, Label, FieldType, Options, IsRequired, DefaultValue, CreatedAt
                    FROM CustomFields
                    WHERE FieldName = @fieldName";

                using (var cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@fieldName", fieldName);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new CustomField
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                FieldName = reader["FieldName"].ToString(),
                                Label = reader["Label"].ToString(),
                                FieldType = reader["FieldType"].ToString(),
                                Options = reader["Options"]?.ToString(),
                                IsRequired = Convert.ToBoolean(reader["IsRequired"]),
                                DefaultValue = reader["DefaultValue"]?.ToString(),
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                            };
                        }
                    }
                }
            }

            return null;
        }

        public int Create(CustomField field)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();

                string sql = @"
                    INSERT INTO CustomFields (FieldName, Label, FieldType, Options, IsRequired, DefaultValue)
                    VALUES (@fieldName, @label, @fieldType, @options, @isRequired, @defaultValue);
                    SELECT last_insert_rowid();";

                using (var cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@fieldName", field.FieldName);
                    cmd.Parameters.AddWithValue("@label", field.Label);
                    cmd.Parameters.AddWithValue("@fieldType", field.FieldType);
                    cmd.Parameters.AddWithValue("@options", field.Options ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@isRequired", field.IsRequired);
                    cmd.Parameters.AddWithValue("@defaultValue", field.DefaultValue ?? (object)DBNull.Value);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void Update(CustomField field)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();

                string sql = @"
                    UPDATE CustomFields
                    SET Label = @label,
                        FieldType = @fieldType,
                        Options = @options,
                        IsRequired = @isRequired,
                        DefaultValue = @defaultValue
                    WHERE Id = @id";

                using (var cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", field.Id);
                    cmd.Parameters.AddWithValue("@label", field.Label);
                    cmd.Parameters.AddWithValue("@fieldType", field.FieldType);
                    cmd.Parameters.AddWithValue("@options", field.Options ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@isRequired", field.IsRequired);
                    cmd.Parameters.AddWithValue("@defaultValue", field.DefaultValue ?? (object)DBNull.Value);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();

                string sql = "DELETE FROM CustomFields WHERE Id = @id";

                using (var cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool FieldNameExists(string fieldName)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();

                string sql = "SELECT COUNT(*) FROM CustomFields WHERE FieldName = @fieldName";

                using (var cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@fieldName", fieldName);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }
    }
}