using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Automated_Employee_Attendance_System.Models;
using Automated_Employee_Attendance_System.Services;

namespace Automated_Employee_Attendance_System
{
    public partial class AttendanceEditWindow : Window
    {
        private Attendance _originalAttendance;
        private List<Employee> _employees;

        public AttendanceEditWindow(Attendance attendance, List<Employee> employees)
        {
            InitializeComponent();
            
            _originalAttendance = attendance;
            _employees = employees;
            
            LoadEmployees();
            LoadAttendanceData();
        }

        private void LoadEmployees()
        {
            var employeeDisplayItems = _employees.Select(emp => new
            {
                emp_id = emp.emp_id,
                DisplayText = $"{emp.emp_id} - {emp.name}"
            }).ToList();

            CmbEmployee.ItemsSource = employeeDisplayItems;
        }

        private void LoadAttendanceData()
        {
            try
            {
                // Set employee
                CmbEmployee.SelectedValue = _originalAttendance.emp_id;
                
                // Set date
                if (DateTime.TryParse(_originalAttendance.date, out DateTime date))
                {
                    DpDate.SelectedDate = date;
                }
                
                // Set time
                TxtTime.Text = _originalAttendance.time;
            }
            catch (Exception ex)
            {
                SystemServices.Log($"Load attendance data error: {ex.Message}");
                MessageBox.Show($"Error loading attendance data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate inputs
                if (CmbEmployee.SelectedValue == null)
                {
                    MessageBox.Show("Please select an employee.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!DpDate.SelectedDate.HasValue)
                {
                    MessageBox.Show("Please select a date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(TxtTime.Text))
                {
                    MessageBox.Show("Please enter a time.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validate time format
                if (!TimeSpan.TryParse(TxtTime.Text, out TimeSpan time))
                {
                    MessageBox.Show("Please enter a valid time (HH:mm format).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string empId = CmbEmployee.SelectedValue.ToString();
                var selectedEmployee = _employees.FirstOrDefault(e => e.emp_id == empId);

                if (selectedEmployee == null)
                {
                    MessageBox.Show("Selected employee not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Create updated attendance record
                var updatedAttendance = new Attendance
                {
                    emp_id = selectedEmployee.emp_id,
                    finger_id = selectedEmployee.finger_id,
                    date = DpDate.SelectedDate.Value.ToString("yyyy-MM-dd"),
                    time = time.ToString(@"hh\:mm")
                };

                // Update in database
                DatabaseService.UpdateAttendance(_originalAttendance, updatedAttendance);
                
                DialogResult = true;
                Close();
                
                MessageBox.Show("Attendance record updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                SystemServices.Log($"Attendance record updated for employee {empId}");
            }
            catch (Exception ex)
            {
                SystemServices.Log($"Save attendance error: {ex.Message}");
                MessageBox.Show($"Error saving attendance record: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}