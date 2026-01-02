namespace Automated_Employee_Attendance_System.Models
{
    public class User
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }

        public bool Dashbord { get; set; }
        public bool Employee { get; set; }
        public bool Attendance { get; set; }
        public bool Report { get; set; }
        public bool Settings { get; set; }

        // Display property for UI
        public string AccessSummary
        {
            get
            {
                var accessList = new List<string>();
                if (Dashbord) accessList.Add("Dashboard");
                if (Employee) accessList.Add("Employee");
                if (Attendance) accessList.Add("Attendance");
                if (Report) accessList.Add("Reports");
                if (Settings) accessList.Add("Settings");

                return accessList.Count > 0 ? string.Join(", ", accessList) : "No access";
            }
        }
    }
}