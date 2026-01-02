namespace Automated_Employee_Attendance_System.Models
{
    public class Device
    {
        public string Name { get; set; } = "ESP Device";
        public string IpAddress { get; set; } = "";
        public string DeviceId { get; set; } = ""; // Unique device identifier
        public string Mode { get; set; } = "STA"; // AP or STA
        public bool IsConnected { get; set; } = false;
    }

    public class Attendance
    {
        public string emp_id { get; set; } = "";
        public int finger_id { get; set; }
        public string date { get; set; } = "";
        public string time { get; set; } = "";
    }
}
