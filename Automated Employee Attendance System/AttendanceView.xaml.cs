using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Automated_Employee_Attendance_System.Services;
using Automated_Employee_Attendance_System.Models;

namespace Automated_Employee_Attendance_System
{
    public partial class AttendanceView : UserControl
    {
        private ESP_Services _espServices;

        public AttendanceView()
        {
            InitializeComponent();
            DataContext = this;

            _espServices = new ESP_Services();

            Loaded += AttendanceView_Loaded;
        }

        private async void AttendanceView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAttendanceData();
        }

        private async Task LoadAttendanceData()
        {
            try
            {
                List<Attendance> attendanceRecords = null;

                await _espServices.ConnectToSavedDevice();

                // ✅ TRY TO LOAD FROM ESP FIRST
                if (!string.IsNullOrEmpty(_espServices.espBaseUrl))
                {
                    try
                    {
                        attendanceRecords = await _espServices.GetAttendanceRecords();

                        // ✅ SYNC ESP DATA TO DATABASE
                        if (attendanceRecords != null && attendanceRecords.Count > 0)
                        {
                            DatabaseService.SaveAttendanceBulk(attendanceRecords);
                            SystemServices.Log($"Loaded {attendanceRecords.Count} attendance records from ESP and synced to database");
                        }
                    }
                    catch (Exception ex)
                    {
                        SystemServices.Log($"ESP attendance load failed, loading from database: {ex.Message}");
                    }
                }

                // ✅ FALLBACK TO DATABASE IF ESP FAILED OR NOT CONNECTED
                if (attendanceRecords == null || attendanceRecords.Count == 0)
                {
                    attendanceRecords = DatabaseService.GetAllAttendance();
                    SystemServices.Log($"Loaded {attendanceRecords.Count} attendance records from database (ESP not available)");
                }

                AttendanceGrid.ItemsSource = attendanceRecords;
            }
            catch (Exception ex)
            {
                SystemServices.Log($"Load attendance error: {ex.Message}");
                AttendanceGrid.ItemsSource = new List<Attendance>();
            }
        }

        private void DatePicker_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private async void RefreshAttendance_Click(object sender, RoutedEventArgs e)
        {
            await LoadAttendanceData();
        }
    }
}