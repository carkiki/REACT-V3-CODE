using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.Sqlite;
using ReactCRM.Models;

namespace ReactCRM.Database
{
    public class TimeEntryRepository
    {
        public List<TimeEntry> GetWorkerEntries(int workerId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var entries = new List<TimeEntry>();
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                SELECT t.*, w.Username as WorkerName
                FROM TimeEntries t
                LEFT JOIN Workers w ON t.WorkerId = w.Id
                WHERE t.WorkerId = @workerId AND t.IsActive = 1";

            if (startDate.HasValue)
                sql += " AND t.Date >= @startDate";
            if (endDate.HasValue)
                sql += " AND t.Date <= @endDate";

            sql += " ORDER BY t.Date DESC, t.ClockIn DESC";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@workerId", workerId);
            if (startDate.HasValue)
                cmd.Parameters.AddWithValue("@startDate", startDate.Value);
            if (endDate.HasValue)
                cmd.Parameters.AddWithValue("@endDate", endDate.Value);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                entries.Add(MapReaderToTimeEntry(reader));
            }

            return entries;
        }

        public List<TimeEntry> GetAllEntries(DateTime? startDate = null, DateTime? endDate = null)
        {
            var entries = new List<TimeEntry>();
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                SELECT t.*, w.Username as WorkerName
                FROM TimeEntries t
                LEFT JOIN Workers w ON t.WorkerId = w.Id
                WHERE t.IsActive = 1";

            if (startDate.HasValue)
                sql += " AND t.Date >= @startDate";
            if (endDate.HasValue)
                sql += " AND t.Date <= @endDate";

            sql += " ORDER BY t.Date DESC, t.ClockIn DESC";

            using var cmd = new SqliteCommand(sql, connection);
            if (startDate.HasValue)
                cmd.Parameters.AddWithValue("@startDate", startDate.Value);
            if (endDate.HasValue)
                cmd.Parameters.AddWithValue("@endDate", endDate.Value);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                entries.Add(MapReaderToTimeEntry(reader));
            }

            return entries;
        }

        public TimeEntry GetActiveEntry(int workerId)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                SELECT t.*, w.Username as WorkerName
                FROM TimeEntries t
                LEFT JOIN Workers w ON t.WorkerId = w.Id
                WHERE t.WorkerId = @workerId
                  AND t.ClockOut IS NULL
                  AND t.IsActive = 1
                ORDER BY t.ClockIn DESC
                LIMIT 1";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@workerId", workerId);
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return MapReaderToTimeEntry(reader);
            }

            return null;
        }

        public int ClockIn(int workerId, string location = null, string notes = null)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            var entry = new TimeEntry
            {
                WorkerId = workerId,
                Date = DateTime.Today,
                ClockIn = DateTime.Now,
                Location = location,
                Notes = notes,
                EntryType = "Regular",
                IsActive = true
            };

            string sql = @"
                INSERT INTO TimeEntries (WorkerId, Date, ClockIn, EntryType,
                                        Location, Notes, IsActive, IsApproved, BreakMinutes)
                VALUES (@workerId, @date, @clockIn, @entryType,
                        @location, @notes, @isActive, @isApproved, @breakMinutes);
                SELECT last_insert_rowid();";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@workerId", entry.WorkerId);
            cmd.Parameters.AddWithValue("@date", entry.Date);
            cmd.Parameters.AddWithValue("@clockIn", entry.ClockIn);
            cmd.Parameters.AddWithValue("@entryType", entry.EntryType);
            cmd.Parameters.AddWithValue("@location", entry.Location ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@notes", entry.Notes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@isActive", entry.IsActive);
            cmd.Parameters.AddWithValue("@isApproved", entry.IsApproved);
            cmd.Parameters.AddWithValue("@breakMinutes", entry.BreakMinutes);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void ClockOut(int entryId, int breakMinutes = 0)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                UPDATE TimeEntries
                SET ClockOut = @clockOut,
                    BreakMinutes = @breakMinutes
                WHERE Id = @id";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", entryId);
            cmd.Parameters.AddWithValue("@clockOut", DateTime.Now);
            cmd.Parameters.AddWithValue("@breakMinutes", breakMinutes);
            cmd.ExecuteNonQuery();
        }

        public void UpdateEntry(TimeEntry entry)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                UPDATE TimeEntries
                SET Date = @date,
                    ClockIn = @clockIn,
                    ClockOut = @clockOut,
                    BreakMinutes = @breakMinutes,
                    EntryType = @entryType,
                    Location = @location,
                    Notes = @notes,
                    IsApproved = @isApproved,
                    ApprovedByUserId = @approvedByUserId,
                    ApprovedDate = @approvedDate
                WHERE Id = @id";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", entry.Id);
            cmd.Parameters.AddWithValue("@date", entry.Date);
            cmd.Parameters.AddWithValue("@clockIn", entry.ClockIn);
            cmd.Parameters.AddWithValue("@clockOut", entry.ClockOut ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@breakMinutes", entry.BreakMinutes);
            cmd.Parameters.AddWithValue("@entryType", entry.EntryType);
            cmd.Parameters.AddWithValue("@location", entry.Location ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@notes", entry.Notes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@isApproved", entry.IsApproved);
            cmd.Parameters.AddWithValue("@approvedByUserId", entry.ApprovedByUserId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@approvedDate", entry.ApprovedDate ?? (object)DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public void DeleteEntry(int id)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = "UPDATE TimeEntries SET IsActive = 0 WHERE Id = @id";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        // Additional methods for compatibility

        public List<TimeEntry> GetAll()

        {

            return GetAllEntries();

        }



        public List<TimeEntry> GetByWorkerId(int workerId)

        {

            return GetWorkerEntries(workerId);

        }



        public List<TimeEntry> GetByWorkerIdAndDate(int workerId, DateTime date)

        {

            return GetWorkerEntries(workerId, date, date);

        }



        public int Create(TimeEntry entry)

        {

            using var connection = DbConnection.GetConnection();

            connection.Open();



            string sql = @"

                INSERT INTO TimeEntries (WorkerId, Date, ClockIn, ClockOut, EntryType,

                                        Location, Notes, IsActive, IsApproved, BreakMinutes)

                VALUES (@workerId, @date, @clockIn, @clockOut, @entryType,

                        @location, @notes, @isActive, @isApproved, @breakMinutes);

                SELECT last_insert_rowid();";



            using var cmd = new SqliteCommand(sql, connection);

            cmd.Parameters.AddWithValue("@workerId", entry.WorkerId);

            cmd.Parameters.AddWithValue("@date", entry.Date);

            cmd.Parameters.AddWithValue("@clockIn", entry.ClockIn);

            cmd.Parameters.AddWithValue("@clockOut", entry.ClockOut ?? (object)DBNull.Value);

            cmd.Parameters.AddWithValue("@entryType", entry.EntryType);

            cmd.Parameters.AddWithValue("@location", entry.Location ?? (object)DBNull.Value);

            cmd.Parameters.AddWithValue("@notes", entry.Notes ?? (object)DBNull.Value);

            cmd.Parameters.AddWithValue("@isActive", entry.IsActive);

            cmd.Parameters.AddWithValue("@isApproved", entry.IsApproved);

            cmd.Parameters.AddWithValue("@breakMinutes", entry.BreakMinutes);



            return Convert.ToInt32(cmd.ExecuteScalar());

        }



        public void Update(TimeEntry entry)

        {

            UpdateEntry(entry);

        }



        public TimeEntry GetById(int id)

        {

            using var connection = DbConnection.GetConnection();

            connection.Open();



            string sql = @"

                SELECT t.*, w.Username as WorkerName

                FROM TimeEntries t

                LEFT JOIN Workers w ON t.WorkerId = w.Id

                WHERE t.Id = @id AND t.IsActive = 1";



            using var cmd = new SqliteCommand(sql, connection);

            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();



            if (reader.Read())

            {

                return MapReaderToTimeEntry(reader);

            }



            return null;

        }

        public Dictionary<DateTime, double> GetWeeklyHours(int workerId, DateTime weekStart)
        {
            var weekEnd = weekStart.AddDays(7);
            var entries = GetWorkerEntries(workerId, weekStart, weekEnd);

            return entries
                .Where(e => e.ClockOut.HasValue)
                .GroupBy(e => e.Date.Date)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(e => e.GetTotalHours())
                );
        }

        private TimeEntry MapReaderToTimeEntry(SqliteDataReader reader)
        {
            return new TimeEntry
            {
                Id = reader.GetInt32("Id"),
                WorkerId = reader.GetInt32("WorkerId"),
                Date = reader.GetDateTime("Date"),
                ClockIn = reader.GetDateTime("ClockIn"),
                ClockOut = reader.IsDBNull("ClockOut") ? null : reader.GetDateTime("ClockOut"),
                BreakMinutes = reader.GetInt32("BreakMinutes"),
                EntryType = reader.GetString("EntryType"),
                Notes = reader.IsDBNull("Notes") ? null : reader.GetString("Notes"),
                Location = reader.IsDBNull("Location") ? null : reader.GetString("Location"),
                IsApproved = reader.GetBoolean("IsApproved"),
                ApprovedByUserId = reader.IsDBNull("ApprovedByUserId") ? null : reader.GetInt32("ApprovedByUserId"),
                ApprovedDate = reader.IsDBNull("ApprovedDate") ? null : reader.GetDateTime("ApprovedDate"),
                IsActive = reader.GetBoolean("IsActive")
            };
        }
    }
}