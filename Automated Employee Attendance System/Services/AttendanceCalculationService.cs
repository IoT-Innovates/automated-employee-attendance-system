using System;
using System.Collections.Generic;
using System.Linq;
using Automated_Employee_Attendance_System.Models;

namespace Automated_Employee_Attendance_System.Services
{
    public class AttendanceCalculation
    {
        public string EmployeeId { get; set; } = "";
        public string EmployeeName { get; set; } = "";
        public string Date { get; set; } = "";
        public string FirstCheckIn { get; set; } = "";
        public string LastCheckIn { get; set; } = "";
        public string Status { get; set; } = "";
        public TimeSpan? WorkingHours { get; set; }
    }

    public static class AttendanceCalculationService
    {
        public static List<AttendanceCalculation> CalculateAttendanceForDate(string date)
        {
            var calculationResults = new List<AttendanceCalculation>();

            try
            {
                // Get all employees
                var allEmployees = DatabaseService.GetAllEmployees();

                // Get attendance records for the specified date
                var attendanceRecords = DatabaseService.GetAttendanceByDate(date);

                SystemServices.Log($"Calculating attendance for {date} - {allEmployees.Count} employees, {attendanceRecords.Count} records");

                foreach (var employee in allEmployees)
                {
                    var employeeAttendance = attendanceRecords
                        .Where(a => a.emp_id == employee.emp_id)
                        .OrderBy(a => a.time)
                        .ToList();

                    var calculation = new AttendanceCalculation
                    {
                        EmployeeId = employee.emp_id,
                        EmployeeName = employee.name,
                        Date = date
                    };

                    if (employeeAttendance.Count == 0)
                    {
                        // Employee is absent
                        calculation.Status = "Absent";
                        calculation.FirstCheckIn = "-";
                        calculation.LastCheckIn = "-";
                    }
                    else if (employeeAttendance.Count == 1)
                    {
                        // Only one fingerprint record - missing check out
                        calculation.FirstCheckIn = employeeAttendance[0].time;
                        calculation.LastCheckIn = "-";
                        calculation.Status = "Missing Check-out";
                    }
                    else
                    {
                        // Multiple records - normal case
                        calculation.FirstCheckIn = employeeAttendance.First().time;
                        calculation.LastCheckIn = employeeAttendance.Last().time;
                        calculation.Status = "Present";

                        // Calculate working hours
                        if (TimeSpan.TryParse(calculation.FirstCheckIn, out TimeSpan firstTime) &&
                            TimeSpan.TryParse(calculation.LastCheckIn, out TimeSpan lastTime))
                        {
                            calculation.WorkingHours = lastTime - firstTime;
                        }
                    }

                    calculationResults.Add(calculation);
                }

                SystemServices.Log($"Attendance calculation completed - {calculationResults.Count} records");
            }
            catch (Exception ex)
            {
                SystemServices.Log($"Attendance calculation error: {ex.Message}");
            }

            return calculationResults;
        }

        public static List<AttendanceCalculation> CalculateAttendanceForDateRange(DateTime startDate, DateTime endDate)
        {
            var allResults = new List<AttendanceCalculation>();

            try
            {
                for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    string dateString = date.ToString("yyyy-MM-dd");
                    var dayResults = CalculateAttendanceForDate(dateString);
                    allResults.AddRange(dayResults);
                }

                SystemServices.Log($"Date range calculation completed - {allResults.Count} records from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                SystemServices.Log($"Date range calculation error: {ex.Message}");
            }

            return allResults;
        }

        public static List<AttendanceCalculation> CalculateEmployeeAttendanceForDateRange(string employeeId, DateTime startDate, DateTime endDate)
        {
            var calculationResults = new List<AttendanceCalculation>();

            try
            {
                // Get specific employee
                var allEmployees = DatabaseService.GetAllEmployees();
                var employee = allEmployees.FirstOrDefault(e => e.emp_id.Equals(employeeId, StringComparison.OrdinalIgnoreCase));

                if (employee == null)
                {
                    SystemServices.Log($"Employee with ID {employeeId} not found");
                    return calculationResults;
                }

                SystemServices.Log($"Calculating attendance for employee {employeeId} from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

                // Get all attendance records for the date range
                var allAttendanceRecords = DatabaseService.GetAllAttendance();

                for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    string dateString = date.ToString("yyyy-MM-dd");

                    // Filter attendance records for this employee and date
                    var employeeAttendance = allAttendanceRecords
                        .Where(a => a.emp_id == employee.emp_id && a.date == dateString)
                        .OrderBy(a => a.time)
                        .ToList();

                    var calculation = new AttendanceCalculation
                    {
                        EmployeeId = employee.emp_id,
                        EmployeeName = employee.name,
                        Date = dateString
                    };

                    if (employeeAttendance.Count == 0)
                    {
                        // Employee is absent
                        calculation.Status = "Absent";
                        calculation.FirstCheckIn = "-";
                        calculation.LastCheckIn = "-";
                    }
                    else if (employeeAttendance.Count == 1)
                    {
                        // Only one fingerprint record - missing check out
                        calculation.FirstCheckIn = employeeAttendance[0].time;
                        calculation.LastCheckIn = "-";
                        calculation.Status = "Missing Check-out";
                    }
                    else
                    {
                        // Multiple records - normal case
                        calculation.FirstCheckIn = employeeAttendance.First().time;
                        calculation.LastCheckIn = employeeAttendance.Last().time;
                        calculation.Status = "Present";

                        // Calculate working hours
                        if (TimeSpan.TryParse(calculation.FirstCheckIn, out TimeSpan firstTime) &&
                            TimeSpan.TryParse(calculation.LastCheckIn, out TimeSpan lastTime))
                        {
                            calculation.WorkingHours = lastTime - firstTime;
                        }
                    }

                    calculationResults.Add(calculation);
                }

                SystemServices.Log($"Employee attendance calculation completed - {calculationResults.Count} records for {employee.name} ({employeeId})");
            }
            catch (Exception ex)
            {
                SystemServices.Log($"Employee attendance calculation error: {ex.Message}");
            }

            return calculationResults;
        }

        public static Dictionary<string, int> GetAttendanceStatistics(List<AttendanceCalculation> calculations)
        {
            var stats = new Dictionary<string, int>
            {
                ["TotalEmployees"] = calculations.Count,
                ["Present"] = calculations.Count(c => c.Status == "Present"),
                ["Absent"] = calculations.Count(c => c.Status == "Absent"),
                ["MissingCheckout"] = calculations.Count(c => c.Status == "Missing Check-out")
            };

            return stats;
        }

        public static Dictionary<string, object> GetEmployeeAttendanceStatistics(List<AttendanceCalculation> calculations)
        {
            var totalDays = calculations.Count;
            var presentDays = calculations.Count(c => c.Status == "Present");
            var absentDays = calculations.Count(c => c.Status == "Absent");
            var missingCheckoutDays = calculations.Count(c => c.Status == "Missing Check-out");

            var attendancePercentage = totalDays > 0 ? (double)(presentDays + missingCheckoutDays) / totalDays * 100 : 0;

            // Calculate total working hours
            var totalWorkingHours = TimeSpan.Zero;
            foreach (var calc in calculations.Where(c => c.WorkingHours.HasValue))
            {
                totalWorkingHours = totalWorkingHours.Add(calc.WorkingHours.Value);
            }

            var stats = new Dictionary<string, object>
            {
                ["TotalDays"] = totalDays,
                ["PresentDays"] = presentDays,
                ["AbsentDays"] = absentDays,
                ["MissingCheckoutDays"] = missingCheckoutDays,
                ["AttendancePercentage"] = Math.Round(attendancePercentage, 2),
                ["TotalWorkingHours"] = totalWorkingHours
            };

            return stats;
        }
    }
}