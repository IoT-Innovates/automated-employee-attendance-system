using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using Automated_Employee_Attendance_System.Models;

namespace Automated_Employee_Attendance_System.Services
{
    public static class DatabaseService
    {
        private static string dbFolder = "Database";
        private static string dbFile = Path.Combine(dbFolder, "attendance.db");
        private static string connectionString = $"Data Source={dbFile}";

        static DatabaseService()
        {
            InitializeDatabase();
        }

        private static void InitializeDatabase()
        {
            if (!Directory.Exists(dbFolder))
                Directory.CreateDirectory(dbFolder);

            // ✅ Create connection to ensure file exists
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
            }

            // ✅ ALWAYS create tables (IF NOT EXISTS prevents duplicates)
            CreateTables();
            
            SystemServices.Log("✓ Database initialized");
            }

        private static void CreateTables()
        {
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                // Create Employees Table
                string createEmployeesTable = @"
                    CREATE TABLE IF NOT EXISTS Employees (
                        emp_id TEXT PRIMARY KEY,
                        name TEXT NOT NULL,
                        email TEXT NOT NULL,
                        finger_id INTEGER NOT NULL,
                        created_at DATETIME DEFAULT CURRENT_TIMESTAMP
                    )";

                // Create Attendance Table
                string createAttendanceTable = @"
                    CREATE TABLE IF NOT EXISTS Attendance (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        emp_id TEXT NOT NULL,
                        finger_id INTEGER NOT NULL,
                        date TEXT NOT NULL,
                        time TEXT NOT NULL,
                        synced BOOLEAN DEFAULT 0,
                        created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (emp_id) REFERENCES Employees(emp_id)
                    )";

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = createEmployeesTable;
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = createAttendanceTable;
                    cmd.ExecuteNonQuery();
                }

                SystemServices.Log("Database tables created");
            }
        }

        // ==================== EMPLOYEE METHODS ====================

        public static void SaveEmployee(Employee employee)
        {
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();

                    string query = @"
                        INSERT OR REPLACE INTO Employees (emp_id, name, email, finger_id)
                        VALUES (@emp_id, @name, @email, @finger_id)";

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = query;
                        cmd.Parameters.AddWithValue("@emp_id", employee.emp_id);
                        cmd.Parameters.AddWithValue("@name", employee.name);
                        cmd.Parameters.AddWithValue("@email", employee.email);
                        cmd.Parameters.AddWithValue("@finger_id", employee.finger_id);

                        cmd.ExecuteNonQuery();
                    }
                }

                SystemServices.Log($"Employee saved to database: {employee.name} (ID: {employee.emp_id})");
            }
            catch (Exception ex)
            {
                SystemServices.Log($"Database save employee error: {ex.Message}");
                throw;
            }
        }

        public static void DeleteEmployee(string empId)
        {
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();

                    // Delete employee
                    string deleteEmp = "DELETE FROM Employees WHERE emp_id = @emp_id";
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = deleteEmp;
                        cmd.Parameters.AddWithValue("@emp_id", empId);
                        cmd.ExecuteNonQuery();
                    }
                }

                SystemServices.Log($"Employee deleted from database: {empId}");
            }
            catch (Exception ex)
            {
                SystemServices.Log($"Database delete employee error: {ex.Message}");
                throw;
            }
        }

        public static List<Employee> GetAllEmployees()
        {
            var employees = new List<Employee>();

            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();

                    string query = "SELECT emp_id, name, email, finger_id FROM Employees ORDER BY emp_id";

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = query;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                employees.Add(new Employee
                                {
                                    emp_id = reader["emp_id"].ToString(),
                                    name = reader["name"].ToString(),
                                    email = reader["email"].ToString(),
                                    finger_id = Convert.ToInt32(reader["finger_id"])
                                });
                            }
                        }
                    }
                }

                SystemServices.Log($"Loaded {employees.Count} employees from database");
            }
            catch (Exception ex)
            {
                SystemServices.Log($"Database load employees error: {ex.Message}");
            }

            return employees;
        }

        // ==================== ATTENDANCE METHODS ====================

        public static void SaveAttendance(Attendance attendance)
        {
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();

                    string query = @"
                        INSERT INTO Attendance (emp_id, finger_id, date, time, synced)
                        VALUES (@emp_id, @finger_id, @date, @time, @synced)";

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = query;
                        cmd.Parameters.AddWithValue("@emp_id", attendance.emp_id);
                        cmd.Parameters.AddWithValue("@finger_id", attendance.finger_id);
                        cmd.Parameters.AddWithValue("@date", attendance.date);
                        cmd.Parameters.AddWithValue("@time", attendance.time);
                        cmd.Parameters.AddWithValue("@synced", true);

                        cmd.ExecuteNonQuery();
                    }
                }

                SystemServices.Log($"Attendance saved to database: {attendance.emp_id} at {attendance.time}");
            }
            catch (Exception ex)
            {
                SystemServices.Log($"Database save attendance error: {ex.Message}");
            }
        }

        public static void SaveAttendanceBulk(List<Attendance> attendanceList)
        {
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {
                        foreach (var attendance in attendanceList)
                        {
                            // Check if already exists
                            string checkQuery = @"
                                SELECT COUNT(*) FROM Attendance 
                                WHERE emp_id = @emp_id AND date = @date AND time = @time";

                            using (var checkCmd = conn.CreateCommand())
                            {
                                checkCmd.CommandText = checkQuery;
                                checkCmd.Parameters.AddWithValue("@emp_id", attendance.emp_id);
                                checkCmd.Parameters.AddWithValue("@date", attendance.date);
                                checkCmd.Parameters.AddWithValue("@time", attendance.time);

                                var result = checkCmd.ExecuteScalar();
                                int count = Convert.ToInt32(result);

                                if (count == 0)
                                {
                                    string insertQuery = @"
                                        INSERT INTO Attendance (emp_id, finger_id, date, time, synced)
                                        VALUES (@emp_id, @finger_id, @date, @time, @synced)";

                                    using (var insertCmd = conn.CreateCommand())
                                    {
                                        insertCmd.CommandText = insertQuery;
                                        insertCmd.Parameters.AddWithValue("@emp_id", attendance.emp_id);
                                        insertCmd.Parameters.AddWithValue("@finger_id", attendance.finger_id);
                                        insertCmd.Parameters.AddWithValue("@date", attendance.date);
                                        insertCmd.Parameters.AddWithValue("@time", attendance.time);
                                        insertCmd.Parameters.AddWithValue("@synced", true);

                                        insertCmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }

                        transaction.Commit();
                    }
                }

                SystemServices.Log($"Bulk saved {attendanceList.Count} attendance records to database");
            }
            catch (Exception ex)
            {
                SystemServices.Log($"Database bulk save attendance error: {ex.Message}");
            }
        }

        public static List<Attendance> GetAllAttendance()
        {
            var attendanceList = new List<Attendance>();

            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT emp_id, finger_id, date, time 
                        FROM Attendance 
                        ORDER BY date DESC, time DESC";

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = query;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                attendanceList.Add(new Attendance
                                {
                                    emp_id = reader["emp_id"].ToString(),
                                    finger_id = Convert.ToInt32(reader["finger_id"]),
                                    date = reader["date"].ToString(),
                                    time = reader["time"].ToString()
                                });
                            }
                        }
                    }
                }

                SystemServices.Log($"Loaded {attendanceList.Count} attendance records from database");
            }
            catch (Exception ex)
            {
                SystemServices.Log($"Database load attendance error: {ex.Message}");
            }

            return attendanceList;
        }

        public static List<Attendance> GetAttendanceByDate(string date)
        {
            var attendanceList = new List<Attendance>();

            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT emp_id, finger_id, date, time 
                        FROM Attendance 
                        WHERE date = @date
                        ORDER BY time DESC";

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = query;
                        cmd.Parameters.AddWithValue("@date", date);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                attendanceList.Add(new Attendance
                                {
                                    emp_id = reader["emp_id"].ToString(),
                                    finger_id = Convert.ToInt32(reader["finger_id"]),
                                    date = reader["date"].ToString(),
                                    time = reader["time"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SystemServices.Log($"Database load attendance by date error: {ex.Message}");
            }

            return attendanceList;
        }
    }
}