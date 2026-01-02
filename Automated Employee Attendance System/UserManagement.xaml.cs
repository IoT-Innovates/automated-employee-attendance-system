using Automated_Employee_Attendance_System.Models;
using Automated_Employee_Attendance_System.Services;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Automated_Employee_Attendance_System
{
    /// <summary>
    /// Interaction logic for UserManagement.xaml
    /// </summary>
    public partial class UserManagement : UserControl
    {
        private List<User> _users;
        private User _currentUser;
        private bool _isEditMode = false;

        public UserManagement()
        {
            InitializeComponent();
            LoadUsers();
            SetEditMode(false);
        }

        private void LoadUsers()
        {
            _users = UserService.Load();
            UsersListView.ItemsSource = null;
            UsersListView.ItemsSource = _users;
        }

        private void UsersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UsersListView.SelectedItem is User user)
            {
                _currentUser = user;
                _isEditMode = true;
                LoadUserDetails(user);
                SetEditMode(true);
            }
        }

        private void LoadUserDetails(User user)
        {
            UsernameTextBox.Text = user.Username;
            PasswordBox.Password = "";
            ConfirmPasswordBox.Password = "";

            DashboardCheckBox.IsChecked = user.Dashbord;
            EmployeeCheckBox.IsChecked = user.Employee;
            AttendanceCheckBox.IsChecked = user.Attendance;
            ReportCheckBox.IsChecked = user.Report;
            SettingsCheckBox.IsChecked = user.Settings;

            PanelTitle.Text = $"Edit User: {user.Username}";
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            _currentUser = new User();
            _isEditMode = false;
            ClearForm();
            SetEditMode(true);
            PanelTitle.Text = "Add New User";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                CustomMessageBox.Show("Please enter a username.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check for duplicate username (only when adding new user)
            if (!_isEditMode && _users.Any(u => u.Username.Equals(UsernameTextBox.Text,
                System.StringComparison.OrdinalIgnoreCase)))
            {
                CustomMessageBox.Show("Username already exists. Please choose a different username.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Password validation (only required for new users or when changing password)
            if (!_isEditMode || !string.IsNullOrEmpty(PasswordBox.Password))
            {
                if (string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    CustomMessageBox.Show("Please enter a password.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (PasswordBox.Password != ConfirmPasswordBox.Password)
                {
                    CustomMessageBox.Show("Passwords do not match.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (PasswordBox.Password.Length < 4)
                {
                    CustomMessageBox.Show("Password must be at least 4 characters long.",
                        "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            // Check if at least one permission is selected
            if (!DashboardCheckBox.IsChecked.GetValueOrDefault() &&
                !EmployeeCheckBox.IsChecked.GetValueOrDefault() &&
                !AttendanceCheckBox.IsChecked.GetValueOrDefault() &&
                !ReportCheckBox.IsChecked.GetValueOrDefault() &&
                !SettingsCheckBox.IsChecked.GetValueOrDefault())
            {
                CustomMessageBox.Show("Please select at least one tab access permission.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Update user object
            _currentUser.Username = UsernameTextBox.Text;

            // Only update password if changed
            if (!string.IsNullOrEmpty(PasswordBox.Password))
            {
                _currentUser.PasswordHash = UserService.Hash(PasswordBox.Password);
            }

            _currentUser.Dashbord = DashboardCheckBox.IsChecked.GetValueOrDefault();
            _currentUser.Employee = EmployeeCheckBox.IsChecked.GetValueOrDefault();
            _currentUser.Attendance = AttendanceCheckBox.IsChecked.GetValueOrDefault();
            _currentUser.Report = ReportCheckBox.IsChecked.GetValueOrDefault();
            _currentUser.Settings = SettingsCheckBox.IsChecked.GetValueOrDefault();

            // Add to list if new user
            if (!_isEditMode)
            {
                _users.Add(_currentUser);
            }

            // Save to file
            UserService.Save(_users);

            CustomMessageBox.Show($"User '{_currentUser.Username}' saved successfully!",
                "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            LoadUsers();
            ClearForm();
            SetEditMode(false);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null || !_isEditMode)
                return;

            var result = CustomMessageBox.Show(
                $"Are you sure you want to delete user '{_currentUser.Username}'?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _users.Remove(_currentUser);
                UserService.Save(_users);

                CustomMessageBox.Show($"User '{_currentUser.Username}' deleted successfully!",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadUsers();
                ClearForm();
                SetEditMode(false);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            SetEditMode(false);
            UsersListView.SelectedItem = null;
        }

        private void ClearForm()
        {
            UsernameTextBox.Text = "";
            PasswordBox.Password = "";
            ConfirmPasswordBox.Password = "";
            DashboardCheckBox.IsChecked = false;
            EmployeeCheckBox.IsChecked = false;
            AttendanceCheckBox.IsChecked = false;
            ReportCheckBox.IsChecked = false;
            SettingsCheckBox.IsChecked = false;
            PanelTitle.Text = "User Details";
        }

        private void SetEditMode(bool enabled)
        {
            UsernameTextBox.IsEnabled = enabled;
            PasswordBox.IsEnabled = enabled;
            ConfirmPasswordBox.IsEnabled = enabled;
            DashboardCheckBox.IsEnabled = enabled;
            EmployeeCheckBox.IsEnabled = enabled;
            AttendanceCheckBox.IsEnabled = enabled;
            ReportCheckBox.IsEnabled = enabled;
            SettingsCheckBox.IsEnabled = enabled;
            SaveButton.IsEnabled = enabled;
            DeleteButton.IsEnabled = enabled && _isEditMode;
            CancelButton.IsEnabled = enabled;

            EmptyStateMessage.Visibility = enabled ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}