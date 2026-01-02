using Microsoft.Toolkit.Uwp.Notifications; // NuGet package
using System;

namespace Automated_Employee_Attendance_System
{
    public static class NotificationHelper
    {
        public static void ShowNotification(string title, string message)
        {
            try
            {
                new ToastContentBuilder()
                    .AddText(title)
                    .AddText(message)
                    .Show();   // Windows 10/11 Action Center toast
            }
            catch (Exception ex)
            {
                // fallback if toast not supported
                System.Windows.
                    MessageBox.Show($"{title}\n{message}\n\n{ex.Message}");
            }
        }
    }
}
