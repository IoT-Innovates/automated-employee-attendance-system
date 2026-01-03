using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Automated_Employee_Attendance_System.Models;
using Automated_Employee_Attendance_System.Services;

namespace Automated_Employee_Attendance_System
{
    public partial class DashboardWindow : UserControl, INotifyPropertyChanged
    {
        private readonly ESP_Services _espServices;
        private DispatcherTimer _refreshTimer;

        // Dashboard Statistics
        private int _totalEmployees;
        public int TotalEmployees
        {
            get => _totalEmployees;
            set
            {
                _totalEmployees = value;
                OnPropertyChanged();
            }
        }

        private int _presentToday;
        public int PresentToday
        {
            get => _presentToday;
            set
            {
                _presentToday = value;
                OnPropertyChanged();
            }
        }

        private int _absentToday;
        public int AbsentToday
        {
            get => _absentToday;
            set
            {
                _absentToday = value;
                OnPropertyChanged();
            }
        }

        // Chart Properties
        public SeriesCollection AttendanceSeries { get; set; }
        public string[] Labels { get; set; }

        // System Status
        private string _systemStatusText;
        public string SystemStatusText
        {
            get => _systemStatusText;
            set
            {
                _systemStatusText = value;
                OnPropertyChanged();
            }
        }

        private Brush _systemStatusColor;
        public Brush SystemStatusColor
        {
            get => _systemStatusColor;
            set
            {
                _systemStatusColor = value;
                OnPropertyChanged();
            }
        }
        // Device connection state (FOR GIF SWITCH)
        private bool _isDeviceConnected;
        public bool IsDeviceConnected
        {
            get => _isDeviceConnected;
            set
            {
                _isDeviceConnected = value;
                OnPropertyChanged();
            }
        }

        public DashboardWindow()
        {
            InitializeComponent();
            DataContext = this;

            _espServices = new ESP_Services();

            // Initialize default values
            SystemStatusText = "Checking...";
            SystemStatusColor = Brushes.Gray;

            // Initialize chart with empty data
            InitializeChart();

            // Load data when control is loaded
            Loaded += DashboardWindow_Loaded;
        }

        private void InitializeChart()
        {
            // Initialize with last 7 days labels
            Labels = GetLast7DaysLabels();

            AttendanceSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Attendance",
                    Values = new ChartValues<int> { 0, 0, 0, 0, 0, 0, 0 },
                    StrokeThickness = 3,
                    PointGeometry = null,
                    LineSmoothness = 1,
                    Stroke = Brushes.DodgerBlue,
                    Fill = new SolidColorBrush(Color.FromArgb(40, 30, 144, 255))
                }
            };
        }

        private async void DashboardWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDashboardData();

            // Set up auto-refresh timer (refresh every 30 seconds)
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            _refreshTimer.Tick += async (s, args) => await LoadDashboardData();
            _refreshTimer.Start();
        }

        private async Task LoadDashboardData()
        {
            try
            {
                // Load statistics
                await LoadStatistics();

                // Load chart data
                await LoadAttendanceChart();

                // Check device status
                await CheckSystemStatus();

                SystemServices.Log("Dashboard data loaded successfully");
            }
            catch (Exception ex)
            {
                SystemServices.Log($"Dashboard load error: {ex.Message}");
            }
        }

        private async Task LoadStatistics()
        {
            await Task.Run(() =>
            {
                try
                {
                    // Get all employees
                    var allEmployees = DatabaseService.GetAllEmployees();
                    TotalEmployees = allEmployees.Count;

                    // Get today's date in yyyy-MM-dd format
                    string today = DateTime.Now.ToString("yyyy-MM-dd");

                    // Get attendance for today
                    var todayAttendance = DatabaseService.GetAttendanceByDate(today);

                    // Count unique employees who attended today
                    var presentEmployeeIds = todayAttendance
                        .Select(a => a.emp_id)
                        .Distinct()
                        .Count();

                    PresentToday = presentEmployeeIds;
                    AbsentToday = TotalEmployees - PresentToday;

                    SystemServices.Log($"Dashboard stats - Total: {TotalEmployees}, Present: {PresentToday}, Absent: {AbsentToday}");
                }
                catch (Exception ex)
                {
                    SystemServices.Log($"Load statistics error: {ex.Message}");
                    TotalEmployees = 0;
                    PresentToday = 0;
                    AbsentToday = 0;
                }
            });
        }

        private async Task LoadAttendanceChart()
        {
            await Task.Run(() =>
            {
                try
                {
                    var chartData = new List<int>();
                    var last7Days = GetLast7Days();

                    foreach (var date in last7Days)
                    {
                        var attendance = DatabaseService.GetAttendanceByDate(date);
                        var uniqueCount = attendance
                            .Select(a => a.emp_id)
                            .Distinct()
                            .Count();
                        chartData.Add(uniqueCount);
                    }

                    Dispatcher.Invoke(() =>
                    {
                        var values = AttendanceSeries[0].Values as ChartValues<int>;
                        values?.Clear();
                        values?.AddRange(chartData);
                    });

                    SystemServices.Log($"Chart loaded with 7 days data: {string.Join(", ", chartData)}");
                }
                catch (Exception ex)
                {
                    SystemServices.Log($"Load chart error: {ex.Message}");
                }
            });
        }

        private async Task CheckSystemStatus()
        {
            try
            {
                bool isConnected = await _espServices.ConnectToSavedDevice();
                var device = _espServices.GetCurrentDevice();

                if (isConnected && device != null)
                {
                    SystemStatusText = $"Connected: {device.Name}";
                    SystemStatusColor = Brushes.LimeGreen;
                    IsDeviceConnected = true;
                    SystemServices.Log($"System status: Connected to {device.Name}");
                }
                else if (device != null)
                {
                    SystemStatusText = "Device Not Reachable";
                    SystemStatusColor = Brushes.Orange;
                    IsDeviceConnected = false;
                    SystemServices.Log("System status: Device not reachable");
                }
                else
                {
                    SystemStatusText = "No Device Configured";
                    SystemStatusColor = Brushes.Red;
                    IsDeviceConnected = false;
                    SystemServices.Log("System status: No device configured");
                }
            }
            catch (Exception ex)
            {
                SystemStatusText = "Connection Error";
                SystemStatusColor = Brushes.Red;
                SystemServices.Log($"System status check error: {ex.Message}");
            }
        }

        private string[] GetLast7DaysLabels()
        {
            var labels = new string[7];
            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.Now.AddDays(-i);
                labels[6 - i] = date.ToString("ddd"); // Mon, Tue, Wed, etc.
            }
            return labels;
        }

        private List<string> GetLast7Days()
        {
            var dates = new List<string>();
            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.Now.AddDays(-i);
                dates.Add(date.ToString("yyyy-MM-dd"));
            }
            return dates;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}