using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using Newtonsoft.Json;
using ReactCRM.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace ReactCRM.Database
{
    public class TodoTaskRepository
    {
        public List<TodoTask> GetAllTasks()
        {
            var tasks = new List<TodoTask>();
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                SELECT t.*,
                       w1.Username as CreatedByName,
                       w2.Username as AssignedToName,
                       c.Name as ClientName,
                       c.ExtraData as ClientExtraData
                FROM TodoTasks t
                LEFT JOIN Workers w1 ON t.CreatedByUserId = w1.Id
                LEFT JOIN Workers w2 ON t.AssignedToUserId = w2.Id
                LEFT JOIN Clients c ON t.ClientId = c.Id
                WHERE t.IsActive = 1
                ORDER BY t.Priority DESC, t.DueDate ASC";

            using var cmd = new SqliteCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                tasks.Add(MapReaderToTask(reader));
            }

            return tasks;
        }

        public List<TodoTask> GetTeamTasks()
        {
            var tasks = new List<TodoTask>();
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                SELECT t.*,
                       w1.Username as CreatedByName,
                       w2.Username as AssignedToName
                FROM TodoTasks t
                LEFT JOIN Workers w1 ON t.CreatedByUserId = w1.Id
                LEFT JOIN Workers w2 ON t.AssignedToUserId = w2.Id
                WHERE t.IsActive = 1 AND t.ClientId IS NULL
                ORDER BY t.Priority DESC, t.DueDate ASC";

            using var cmd = new SqliteCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                tasks.Add(MapReaderToTask(reader));
            }

            return tasks;
        }

        public List<TodoTask> GetClientTasks(int clientId)
        {
            var tasks = new List<TodoTask>();
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                SELECT t.*,
                       w1.Username as CreatedByName,
                       w2.Username as AssignedToName
                FROM TodoTasks t
                LEFT JOIN Workers w1 ON t.CreatedByUserId = w1.Id
                LEFT JOIN Workers w2 ON t.AssignedToUserId = w2.Id
                WHERE t.IsActive = 1 AND t.ClientId = @clientId
                ORDER BY t.Priority DESC, t.DueDate ASC";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@clientId", clientId);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                tasks.Add(MapReaderToTask(reader));
            }

            return tasks;
        }

        public List<TodoTask> GetPendingTasks()
        {
            var tasks = new List<TodoTask>();
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                SELECT t.*,
                       w1.Username as CreatedByName,
                       w2.Username as AssignedToName,
                        c.Name as ClientName,
                       c.ExtraData as ClientExtraData
                FROM TodoTasks t
                LEFT JOIN Workers w1 ON t.CreatedByUserId = w1.Id
                LEFT JOIN Workers w2 ON t.AssignedToUserId = w2.Id
                LEFT JOIN Clients c ON t.ClientId = c.Id
                WHERE t.IsActive = 1 AND t.Status IN ('Pending', 'InProgress')
                ORDER BY t.Priority DESC, t.DueDate ASC
                LIMIT 10";

            using var cmd = new SqliteCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                tasks.Add(MapReaderToTask(reader));
            }

            return tasks;
        }

        public TodoTask GetTaskById(int id)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                SELECT t.*,
                       w1.Username as CreatedByName,
                       w2.Username as AssignedToName,
                        c.Name as ClientName,
                       c.ExtraData as ClientExtraData
                FROM TodoTasks t
                LEFT JOIN Workers w1 ON t.CreatedByUserId = w1.Id
                LEFT JOIN Workers w2 ON t.AssignedToUserId = w2.Id
                LEFT JOIN Clients c ON t.ClientId = c.Id
                WHERE t.Id = @id";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return MapReaderToTask(reader);
            }

            return null;
        }

        public int CreateTask(TodoTask task)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                INSERT INTO TodoTasks (Title, Description, Priority, Status, DueDate,
                                       CreatedDate, CreatedByUserId, AssignedToUserId,
                                       ClientId, Category, Notes, IsActive)
                VALUES (@title, @description, @priority, @status, @dueDate,
                        @createdDate, @createdByUserId, @assignedToUserId,
                        @clientId, @category, @notes, @isActive);
                SELECT last_insert_rowid();";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@title", task.Title);
            cmd.Parameters.AddWithValue("@description", task.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@priority", task.Priority);
            cmd.Parameters.AddWithValue("@status", task.Status);
            cmd.Parameters.AddWithValue("@dueDate", task.DueDate ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@createdDate", task.CreatedDate);
            cmd.Parameters.AddWithValue("@createdByUserId", task.CreatedByUserId);
            cmd.Parameters.AddWithValue("@assignedToUserId", task.AssignedToUserId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@clientId", task.ClientId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@category", task.Category ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@notes", task.Notes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@isActive", task.IsActive);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void UpdateTask(TodoTask task)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                UPDATE TodoTasks
                SET Title = @title,
                    Description = @description,
                    Priority = @priority,
                    Status = @status,
                    DueDate = @dueDate,
                    CompletedDate = @completedDate,
                    AssignedToUserId = @assignedToUserId,
                    ClientId = @clientId,
                    Category = @category,
                    Notes = @notes,
                    IsActive = @isActive
                WHERE Id = @id";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", task.Id);
            cmd.Parameters.AddWithValue("@title", task.Title);
            cmd.Parameters.AddWithValue("@description", task.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@priority", task.Priority);
            cmd.Parameters.AddWithValue("@status", task.Status);
            cmd.Parameters.AddWithValue("@dueDate", task.DueDate ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@completedDate", task.CompletedDate ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@assignedToUserId", task.AssignedToUserId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@clientId", task.ClientId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@category", task.Category ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@notes", task.Notes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@isActive", task.IsActive);

            cmd.ExecuteNonQuery();
        }

        public void DeleteTask(int id)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = "UPDATE TodoTasks SET IsActive = 0 WHERE Id = @id";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public void CompleteTask(int id)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                UPDATE TodoTasks
                SET Status = 'Completed',
                    CompletedDate = @completedDate
                WHERE Id = @id";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@completedDate", DateTime.Now);
            cmd.ExecuteNonQuery();
        }

        private TodoTask MapReaderToTask(SqliteDataReader reader)
        {
            var task = new TodoTask

            {

                Id = reader.GetInt32("Id"),

                Title = reader.GetString("Title"),

                Description = reader.IsDBNull("Description") ? null : reader.GetString("Description"),

                Priority = reader.GetInt32("Priority"),

                Status = reader.GetString("Status"),

                DueDate = reader.IsDBNull("DueDate") ? null : reader.GetDateTime("DueDate"),

                CreatedDate = reader.GetDateTime("CreatedDate"),

                CompletedDate = reader.IsDBNull("CompletedDate") ? null : reader.GetDateTime("CompletedDate"),

                CreatedByUserId = reader.GetInt32("CreatedByUserId"),

                AssignedToUserId = reader.IsDBNull("AssignedToUserId") ? null : reader.GetInt32("AssignedToUserId"),

                ClientId = reader.IsDBNull("ClientId") ? null : reader.GetInt32("ClientId"),

                Category = reader.IsDBNull("Category") ? null : reader.GetString("Category"),

                Notes = reader.IsDBNull("Notes") ? null : reader.GetString("Notes"),

                IsActive = reader.GetBoolean("IsActive")

            };



            // Build full client name (first name + last name from ExtraData)

            if (!reader.IsDBNull("ClientName"))

            {

                string firstName = reader.GetString("ClientName");

                string fullName = firstName;



                // Check if we have ExtraData to extract last name

                if (!reader.IsDBNull("ClientExtraData"))

                {

                    try

                    {

                        string extraDataJson = reader.GetString("ClientExtraData");

                        var extraData = JsonConvert.DeserializeObject<Dictionary<string, object>>(extraDataJson);



                        if (extraData != null)

                        {

                            // Create a temporary Client object to use the GetLastName helper

                            var tempClient = new Client
                            {
                                Name = firstName,
                                ExtraData = extraData
                            };
                            fullName = tempClient.GetFullName();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error parsing client ExtraData: {ex.Message}");
                        // Fall back to just the first name
                    }
                }
                task.ClientName = fullName;
            }
            return task;
        }
    }
}
