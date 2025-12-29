using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automated_Employee_Attendance_System.Services
{
    public static class SystemServices
    {
        private static readonly string LogFolder =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SystemLogs");

        private static readonly string LogFile =
            Path.Combine(LogFolder, "system_status.txt");

        // ---------------- SAVE STATUS ----------------
        public static void Log(string message)
        {
            try
            {
                if (!Directory.Exists(LogFolder))
                    Directory.CreateDirectory(LogFolder);

                ClearOldLogs(); // auto clear old data

                string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}";
                File.AppendAllText(LogFile, line + Environment.NewLine);
            }
            catch
            {
                // silent fail (system service ekak nisa)
            }
        }

        // ---------------- READ ALL STATUS ----------------
        public static string ReadAll()
        {
            try
            {
                if (!File.Exists(LogFile))
                    return "";

                return File.ReadAllText(LogFile);
            }
            catch
            {
                return "";
            }
        }

        // ---------------- AUTO CLEAR AFTER 7 DAYS ----------------
        private static void ClearOldLogs()
        {
            if (!File.Exists(LogFile))
                return;

            var lines = File.ReadAllLines(LogFile);

            var validLines = lines.Where(l =>
            {
                var parts = l.Split('|');
                if (DateTime.TryParse(parts[0], out DateTime time))
                {
                    return (DateTime.Now - time).TotalDays <= 7;
                }
                return false;
            });

            File.WriteAllLines(LogFile, validLines);
        }
    }
}
