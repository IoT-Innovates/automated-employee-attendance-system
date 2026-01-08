using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public partial class AttendanceView : UserControl, INotifyPropertyChanged
    {
        private ESP_Services _espServices;
        private List<Attendance> _allAttendanceRecords;
        private List<Employee> _allEmployees;
        private List<AttendanceCalculation> _calculationResults;

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

        // Calculation Statistics Properties
        private int _calculationTotalEmployees;
        public int CalculationTotalEmployees
        {
            get => _calculationTotalEmployees;
            set
            {
                _calculationTotalEmployees = value;
                OnPropertyChanged();
            }
        }

        private int _calculationPresent;
        public int CalculationPresent
        {
            get => _calculationPresent;
            set
            {
                _calculationPresent = value;
                OnPropertyChanged();
            }
        }

        private int _calculationAbsent;
        public int CalculationAbsent
        {
            get => _calculationAbsent;
            set
            {
                _calculationAbsent = value;
                OnPropertyChanged();
            }
        }

        private int _calculationMissingCheckout;
        public int CalculationMissingCheckout
        {
            get => _calculationMissingCheckout;
            set
            {
                _calculationMissingCheckout = value;
                OnPropertyChanged();
            }
        }

        public AttendanceView()
        {
            InitializeComponent();
            DataContext = this;

            _espServices = new ESP_Services();
            _allAttendanceRecords = new List<Attendance>();
            _allEmployees = new List<Employee>();
            _calculationResults = new List<AttendanceCalculation>();

            // Initialize default values
            InitializeDefaultValues();

            Loaded += AttendanceView_Loaded;
        }

        private void InitializeDefaultValues()
        {
            // Set default date to today
            DpEntryDate.SelectedDate = DateTime.Today;
            CalcDatePicker.SelectedDate = DateTime.Today;

            // Set default dates for employee search (last 7 days)
            EmpEndDatePicker.SelectedDate = DateTime.Today;
            EmpStartDatePicker.SelectedDate = DateTime.Today.AddDays(-6);

            // Set default time to current time
            TxtEntryTime.Text = DateTime.Now.ToString("HH:mm");
        }

        private async void AttendanceView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAttendanceData();
            LoadEmployees();
            
            // Load today's calculation by default
            await CalculateAttendanceForDate(DateTime.Today.ToString("yyyy-MM-dd"));
        }

        private void LoadEmployees()
        {
            try
            {
                _allEmployees = DatabaseService.GetAllEmployees();

                // Create display items for combo box
                var employeeDisplayItems = _allEmployees.Select(emp => new
                {
                    emp_id = emp.emp_id,
                    DisplayText = $"{emp.emp_id} - {emp.name}"
                }).ToList();

                CmbEmployee.ItemsSource = employeeDisplayItems;
                SystemServices.Log($"Loaded {_allEmployees.Count} employees for manual entry");
            }
            catch (Exception ex)
            {
                SystemServices.Log($"Load employees error: {ex.Message}");
                MessageBox.Show($"Error loading employees: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

                _allAttendanceRecords = attendanceRecords;
                AttendanceGrid.ItemsSource = _allAttendanceRecords;

                // ✅ UPDATE STATISTICS
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                SystemServices.Log($"Load attendance error: {ex.Message}");
                _allAttendanceRecords = new List<Attendance>();
                AttendanceGrid.ItemsSource = _allAttendanceRecords;
                UpdateStatistics();
            }
        }

        private void UpdateStatistics()
        {
            try
            {
                // Get all employees
                var allEmployees = DatabaseService.GetAllEmployees();
                TotalEmployees = allEmployees.Count;

                // Get today's date in the same format as your attendance records
                string today = DateTime.Now.ToString("yyyy-MM-dd");

                // Get attendance for today
                var todayAttendance = DatabaseService.GetAttendanceByDate(today);

                // Get unique employees who attended today
                var presentEmployeeIds = todayAttendance
                    .Select(a => a.emp_id)
                    .Distinct()
                    .ToList();

                PresentToday = presentEmployeeIds.Count;
                AbsentToday = TotalEmployees - PresentToday;

                SystemServices.Log($"Stats updated - Total: {TotalEmployees}, Present: {PresentToday}, Absent: {AbsentToday}");
            }
            catch (Exception ex)
            {
                SystemServices.Log($"Update statistics error: {ex.Message}");
                TotalEmployees = 0;
                PresentToday = 0;
                AbsentToday = 0;
            }
        }

        private async Task CalculateAttendanceForDate(string date)
        {
            try
            {
                _calculationResults = AttendanceCalculationService.CalculateAttendanceForDate(date);
                CalculationGrid.ItemsSource = _calculationResults;

                // Update statistics
                var stats = AttendanceCalculationService.GetAttendanceStatistics(_calculationResults);
                CalculationTotalEmployees = stats["TotalEmployees"];
                CalculationPresent = stats["Present"];
                CalculationAbsent = stats["Absent"];
                CalculationMissingCheckout = stats["MissingCheckout"];

                SystemServices.Log($"Calculation completed for {date} - Total: {CalculationTotalEmployees}, Present: {CalculationPresent}, Absent: {CalculationAbsent}, Missing: {CalculationMissingCheckout}");
            }
            catch (Exception ex)
            {
                SystemServices.Log($"Calculate attendance error: {ex.Message}");
                MessageBox.Show($"Error calculating attendance: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilters()
        {
            try
            {
                var filteredRecords = _allAttendanceRecords.AsEnumerable();

                // Filter by date range
                if (StartDatePicker.SelectedDate.HasValue && EndDatePicker.SelectedDate.HasValue)
                {
                    DateTime startDate = StartDatePicker.SelectedDate.Value;
                    DateTime endDate = EndDatePicker.SelectedDate.Value;

                    filteredRecords = filteredRecords.Where(a =>
                    {
                        if (DateTime.TryParse(a.date, out DateTime recordDate))
                        {
                            return recordDate >= startDate && recordDate <= endDate;
                        }
                        return false;
                    });
                }
                else if (StartDatePicker.SelectedDate.HasValue)
                {
                    DateTime startDate = StartDatePicker.SelectedDate.Value;
                    filteredRecords = filteredRecords.Where(a =>
                    {
                        if (DateTime.TryParse(a.date, out DateTime recordDate))
                        {
                            return recordDate >= startDate;
                        }
                        return false;
                    });
                }
                else if (EndDatePicker.SelectedDate.HasValue)
                {
                    DateTime endDate = EndDatePicker.SelectedDate.Value;
                    filteredRecords = filteredRecords.Where(a =>
                    {
                        if (DateTime.TryParse(a.date, out DateTime recordDate))
                        {
                            return recordDate <= endDate;
                        }
                        return false;
                    });
                }

                // Filter by search text
                string searchText = TxtSearch.Text?.Trim().ToLower();
                if (!string.IsNullOrEmpty(searchText))
                {
                    filteredRecords = filteredRecords.Where(a =>
                        a.emp_id.ToString().Contains(searchText) ||
                        a.finger_id.ToString().Contains(searchText) ||
                        a.date.ToLower().Contains(searchText) ||
                        a.time.ToLower().Contains(searchText)
                    );
                }

                AttendanceGrid.ItemsSource = filteredRecords.ToList();
                SystemServices.Log($"Filters applied - {filteredRecords.Count()} records found");
            }
            catch (Exception ex)
            {
                SystemServices.Log($"Apply filters error: {ex.Message}");
            }
        }

        private async void AddManualEntry_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CmbEmployee.SelectedValue == null)
                {
                    MessageBox.Show("Please select an employee.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!DpEntryDate.SelectedDate.HasValue)
                {
                    MessageBox.Show("Please select a date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(TxtEntryTime.Text))
                {
                    MessageBox.Show("Please enter a time.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validate time format
                if (!TimeSpan.TryParse(TxtEntryTime.Text, out TimeSpan time))
                {
                    MessageBox.Show("Please enter a valid time (HH:mm format).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string empId = CmbEmployee.SelectedValue.ToString() ?? "";
                var selectedEmployee = _allEmployees.FirstOrDefault(e => e.emp_id == empId);

                if (selectedEmployee == null)
                {
                    MessageBox.Show("Selected employee not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var attendance = new Attendance
                {
                    emp_id = selectedEmployee.emp_id,
                    finger_id = selectedEmployee.finger_id,
                    date = DpEntryDate.SelectedDate.Value.ToString("yyyy-MM-dd"),
                    time = time.ToString(@"hh\:mm")
                };

                DatabaseService.SaveAttendance(attendance);

                // Refresh the view
                await LoadAttendanceData();

                // Clear the form and reset to default values
                CmbEmployee.SelectedValue = null;
                DpEntryDate.SelectedDate = DateTime.Today;
                TxtEntryTime.Text = DateTime.Now.ToString("HH:mm");

                MessageBox.Show("Attendance entry added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                SystemServices.Log($"Manual attendance entry added for employee {empId}");
            }
            catch (Exception ex)
            {
                SystemServices.Log($"Add manual entry error: {ex.Message}");
                MessageBox.Show($"Error adding attendance entry: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void EditAttendance_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.DataContext is Attendance attendance)
                {
                    var editWindow = new AttendanceEditWindow(attendance, _allEmployees);
                    if (editWindow.ShowDialog() == true)
                    {
                        // Refresh the view after editing
                        await LoadAttendanceData();
                    }
                }
            }
            catch (Exception ex)
            {
                SystemServices.Log($"Edit attendance error: {ex.Message}");
                MessageBox.Show($"Error editing attendance: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteAttendance_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.DataContext is Attendance attendance)
                {
                    var result = MessageBox.Show($"Are you sure you want to delete this attendance record?\n\nEmployee: {attendance.emp_id}\nDate: {attendance.date}\nTime: {attendance.time}",
                        "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        DatabaseService.DeleteAttendance(attendance);
                        await LoadAttendanceData();

                        MessageBox.Show("Attendance record deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        SystemServices.Log($"Attendance record deleted for employee {attendance.emp_id}");
                    }
                }
            }
            catch (Exception ex)
            {
                SystemServices.Log($"Delete attendance error: {ex.Message}");
                MessageBox.Show($"Error deleting attendance: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AttendanceGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (AttendanceGrid.SelectedItem is Attendance attendance)
            {
                var editWindow = new AttendanceEditWindow(attendance, _allEmployees);
                if (editWindow.ShowDialog() == true)
                {
                    await LoadAttendanceData();
                }
            }
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SearchAttendance_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            // Clear all filter controls
            StartDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;
            TxtSearch.Text = string.Empty;

            // Reset to show all records
            AttendanceGrid.ItemsSource = _allAttendanceRecords;
            SystemServices.Log("All filters cleared");
        }

        private void DatePicker_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private async void RefreshAttendance_Click(object sender, RoutedEventArgs e)
        {
            // Clear filters before refreshing
            StartDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;
            TxtSearch.Text = string.Empty;

            await LoadAttendanceData();
        }

        // ================= CALCULATION EVENT HANDLERS =================

        private async void CalculateAttendance_Click(object sender, RoutedEventArgs e)
        {
            if (!CalcDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select a date for calculation.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string selectedDate = CalcDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd");
            await CalculateAttendanceForDate(selectedDate);
        }

        private async void CalculateToday_Click(object sender, RoutedEventArgs e)
        {
            CalcDatePicker.SelectedDate = DateTime.Today;
            string todayDate = DateTime.Today.ToString("yyyy-MM-dd");
            await CalculateAttendanceForDate(todayDate);
        }

        // ================= EMPLOYEE SPECIFIC SEARCH EVENT HANDLERS =================

        private async void SearchEmployeeAttendance_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(TxtEmployeeId.Text))
                {
                    MessageBox.Show("Please enter an Employee ID.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtSearchInfo.Text = "❌ Please enter an Employee ID";
                    TxtSearchInfo.Foreground = Brushes.Red;
                    return;
                }

                if (!EmpStartDatePicker.SelectedDate.HasValue || !EmpEndDatePicker.SelectedDate.HasValue)
                {
                    MessageBox.Show("Please select both start and end dates.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtSearchInfo.Text = "❌ Please select both start and end dates";
                    TxtSearchInfo.Foreground = Brushes.Red;
                    return;
                }

                DateTime startDate = EmpStartDatePicker.SelectedDate.Value;
                DateTime endDate = EmpEndDatePicker.SelectedDate.Value;

                if (startDate > endDate)
                {
                    MessageBox.Show("Start date cannot be later than end date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtSearchInfo.Text = "❌ Start date cannot be later than end date";
                    TxtSearchInfo.Foreground = Brushes.Red;
                    return;
                }

                string employeeId = TxtEmployeeId.Text.Trim();

                // Check if employee exists
                var employee = _allEmployees.FirstOrDefault(e => e.emp_id.Equals(employeeId, StringComparison.OrdinalIgnoreCase));
                if (employee == null)
                {
                    MessageBox.Show($"Employee with ID '{employeeId}' not found.", "Employee Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtSearchInfo.Text = $"❌ Employee '{employeeId}' not found";
                    TxtSearchInfo.Foreground = Brushes.Red;
                    return;
                }

                TxtSearchInfo.Text = "🔍 Searching...";
                TxtSearchInfo.Foreground = Brushes.Orange;

                // Calculate employee attendance for date range
                var results = AttendanceCalculationService.CalculateEmployeeAttendanceForDateRange(employeeId, startDate, endDate);
                CalculationGrid.ItemsSource = results;

                // Update statistics for this specific search
                var empStats = AttendanceCalculationService.GetEmployeeAttendanceStatistics(results);
                CalculationTotalEmployees = 1; // Only one employee
                CalculationPresent = (int)empStats["PresentDays"];
                CalculationAbsent = (int)empStats["AbsentDays"];
                CalculationMissingCheckout = (int)empStats["MissingCheckoutDays"];

                // Update search info
                int totalDays = (int)empStats["TotalDays"];
                double attendancePercentage = (double)empStats["AttendancePercentage"];
                var totalWorkingHours = (TimeSpan)empStats["TotalWorkingHours"];

                TxtSearchInfo.Text = $"✅ Found {results.Count} records for {employee.name} | " +
                                   $"Attendance: {attendancePercentage:F1}% | " +
                                   $"Present: {CalculationPresent}/{totalDays} days | " +
                                   $"Total Hours: {totalWorkingHours:hh\\:mm}";
                TxtSearchInfo.Foreground = Brushes.Green;

                SystemServices.Log($"Employee search completed - {employeeId}: {results.Count} records from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                SystemServices.Log($"Employee search error: {ex.Message}");
                MessageBox.Show($"Error searching employee attendance: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TxtSearchInfo.Text = "❌ Search failed";
                TxtSearchInfo.Foreground = Brushes.Red;
            }
        }

        private async void ClearEmployeeSearch_Click(object sender, RoutedEventArgs e)
        {
            // Clear employee search inputs
            TxtEmployeeId.Text = string.Empty;
            EmpStartDatePicker.SelectedDate = DateTime.Today.AddDays(-6);
            EmpEndDatePicker.SelectedDate = DateTime.Today;

            // Reset search info
            TxtSearchInfo.Text = "Enter Employee ID and date range to search specific attendance records";
            TxtSearchInfo.Foreground = Brushes.Gray;

            // Reload today's calculation
            await CalculateAttendanceForDate(DateTime.Today.ToString("yyyy-MM-dd"));

            SystemServices.Log("Employee search cleared");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}   