using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Automated_Employee_Attendance_System.Services;
using System.Windows;
using System.Windows.Controls;
using Automated_Employee_Attendance_System.Models;
using System.Windows.Input;
using System.Text.RegularExpressions;
using XamlAnimatedGif;

namespace Automated_Employee_Attendance_System
{
    public partial class EmployeeWindow : UserControl
    {
        private TextBlock Status;
        private int? tempFingerId = null; // Store finger ID from scan

        private ESP_Services _espServices;
        HttpClient client => _espServices.client;
        string espBaseUrl => _espServices.espBaseUrl;
        public Action<string>? OnStatusChanged;

        public EmployeeWindow()
        {
            InitializeComponent();
            Status = this.FindName("Status") as TextBlock;

            _espServices = new ESP_Services();
            _espServices.OnStatusChanged += (msg) => OnStatusChanged?.Invoke(msg);

            Loaded += LoadingWindow_Loaded;

            _ = InitializeESP();
        }

        private async Task InitializeESP()
        {
            await _espServices.DetectESP();
            await LoadEmployees();
        }

        private async void LoadingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                string baseFolder = AppDomain.CurrentDomain.BaseDirectory;
                string gifPath = System.IO.Path.Combine(baseFolder, "UI", "Fingerprint_biometric_scan.gif");
                var gifUri = new Uri(gifPath, UriKind.Absolute);

                Dispatcher.Invoke(() =>
                {
                    AnimationBehavior.SetSourceUri(MyGifImage, gifUri);
                    AnimationBehavior.SetRepeatBehavior(MyGifImage, System.Windows.Media.Animation.RepeatBehavior.Forever);
                });
            });
        }

        /// <summary>
        /// STEP 1: Scan fingerprint FIRST (enrollment mode)
        /// This stores the fingerprint in AS608 and returns finger_id
        /// </summary>
        private async void ScanFingerprint_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(espBaseUrl))
            {
                CustomMessageBox.Show("ESP not connected");
                return;
            }

            // Validate employee data before scanning
            if (string.IsNullOrWhiteSpace(EmpId.Text) ||
                string.IsNullOrWhiteSpace(EmpName.Text) ||
                string.IsNullOrWhiteSpace(EmpNIC.Text))
            {
                CustomMessageBox.Show("Please enter Employee ID, Name, and NIC first");
                return;
            }

            CustomMessageBox.Show("Place finger on sensor (scan 1/2)...");

            try
            {
                var res = await client.GetAsync($"{espBaseUrl}/scanFingerprint");

                if (!res.IsSuccessStatusCode)
                {
                    CustomMessageBox.Show("Fingerprint enrollment failed");
                    SystemServices.Log("Fingerprint enrollment failed");

                    // Clear temp data on failure
                    tempFingerId = null;
                    return;
                }

                var json = await res.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                // Check if finger_id was returned
                if (doc.RootElement.TryGetProperty("finger_id", out JsonElement fingerIdElement))
                {
                    tempFingerId = fingerIdElement.GetInt32();

                    CustomMessageBox.Show($"Fingerprint enrolled successfully!\nFinger ID: {tempFingerId}\n\nNow click 'Add Employee' to save.");
                    SystemServices.Log($"Fingerprint enrolled with ID: {tempFingerId}");
                }
                else
                {
                    tempFingerId = null;
                    CustomMessageBox.Show("Invalid response from ESP - no finger_id returned");
                    SystemServices.Log($"ESP Response: {json}");
                }
            }
            catch (Exception ex)
            {
                tempFingerId = null;
                CustomMessageBox.Show($"Error: {ex.Message}");
                SystemServices.Log($"Fingerprint scan error: {ex.Message}");
            }
        }

        /// <summary>
        /// STEP 2: Add employee ONLY if fingerprint scan was successful
        /// This saves employee data + finger_id to SD card
        /// </summary>
        private async void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            // Validate fingerprint was scanned first
            if (tempFingerId == null)
            {
                CustomMessageBox.Show("Please scan fingerprint first!\n\nFingerprint must be enrolled before saving employee.");
                return;
            }

            // Validate all fields
            if (string.IsNullOrWhiteSpace(EmpId.Text) ||
                string.IsNullOrWhiteSpace(EmpName.Text) ||
                string.IsNullOrWhiteSpace(EmpNIC.Text))
            {
                CustomMessageBox.Show("Please fill all employee fields");
                return;
            }

            try
            {
                // Create employee object with finger_id
                var emp = new
                {
                    emp_id = EmpId.Text.Trim(),
                    name = EmpName.Text.Trim(),
                    nic = EmpNIC.Text.Trim(),
                    finger_id = tempFingerId.Value
                };

                var json = JsonSerializer.Serialize(emp);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var res = await client.PostAsync($"{espBaseUrl}/addEmployee", content);

                if (!res.IsSuccessStatusCode)
                {
                    var errorBody = await res.Content.ReadAsStringAsync();
                    CustomMessageBox.Show($"Failed to save employee to SD card\n{errorBody}");
                    SystemServices.Log($"Employee save failed: {errorBody}");
                    return;
                }

                CustomMessageBox.Show($"Employee registered successfully!\n\nName: {emp.name}\nID: {emp.emp_id}\nFinger ID: {emp.finger_id}");
                SystemServices.Log($"Employee added: {emp.name} (ID: {emp.emp_id}, Finger: {emp.finger_id})");

                // Clear form and temp data
                EmpId.Text = "";
                EmpName.Text = "";
                EmpNIC.Text = "";
                tempFingerId = null;

                // Reload employee list
                await LoadEmployees();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Error saving employee: {ex.Message}");
                SystemServices.Log($"Employee save error: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete employee from SD card and fingerprint from sensor
        /// </summary>
        private async void DeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            var emp = (sender as Button)?.CommandParameter as Employee;
            if (emp == null)
            {
                CustomMessageBox.Show("No employee selected");
                return;
            }

            var confirm = MessageBox.Show(
                $"Delete employee {emp.name}?\n\nThis will also delete their fingerprint from the sensor.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
                return;

            try
            {
                var json = JsonSerializer.Serialize(new { id = emp.id.Trim() });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var res = await client.PostAsync($"{espBaseUrl}/deleteEmployee", content);
                var body = await res.Content.ReadAsStringAsync();

                if (!res.IsSuccessStatusCode)
                {
                    CustomMessageBox.Show($"Delete failed: {body}");
                    SystemServices.Log($"Delete failed: {body}");
                    return;
                }

                using var doc = JsonDocument.Parse(body);
                var status = doc.RootElement.GetProperty("status").GetString();

                if (status == "ok")
                {
                    CustomMessageBox.Show($"Employee deleted successfully\n\nName: {emp.name}");
                    SystemServices.Log($"Deleted employee: {emp.name} (ID: {emp.id})");
                    await LoadEmployees();
                }
                else
                {
                    CustomMessageBox.Show($"Delete failed: {status}");
                    SystemServices.Log($"Delete failed: {status}");
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Error: {ex.Message}");
                SystemServices.Log($"Delete error: {ex.Message}");
            }
        }

        /// <summary>
        /// Load all employees from SD card
        /// </summary>
        public async Task LoadEmployees()
        {
            if (string.IsNullOrEmpty(espBaseUrl))
                return;

            try
            {
                var res = await client.GetAsync($"{espBaseUrl}/employees");

                if (!res.IsSuccessStatusCode)
                {
                    CustomMessageBox.Show("Failed to load employees");
                    SystemServices.Log("Failed to load employees");
                    return;
                }

                var json = await res.Content.ReadAsStringAsync();
                var list = JsonSerializer.Deserialize<List<Employee>>(json);

                SystemServices.Log($"Loaded {list?.Count ?? 0} employees from ESP");

                EmployeeGrid.ItemsSource = list;
            }
            catch (Exception ex)
            {
                SystemServices.Log($"Load employees error: {ex.Message}");
            }
        }

        /// <summary>
        /// Only allow numeric input for Employee ID
        /// </summary>
        private void EmpId_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^\d+$");

            DataObject.AddPastingHandler(EmpId, (s, e) =>
            {
                if (e.DataObject.GetDataPresent(DataFormats.Text))
                {
                    string text = e.DataObject.GetData(DataFormats.Text) as string;
                    if (!Regex.IsMatch(text ?? "", @"^\d+$"))
                        e.CancelCommand();
                }
                else
                    e.CancelCommand();
            });
        }

        private void EmpId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Block Ctrl+V paste
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                e.Handled = true;
        }

        /// <summary>
        /// Cancel registration - clears form and temp fingerprint
        /// </summary>
        private void CancelRegistration_Click(object sender, RoutedEventArgs e)
        {
            if (tempFingerId != null)
            {
                var confirm = MessageBox.Show(
                    "You have scanned a fingerprint. Canceling will discard it.\n\nContinue?",
                    "Confirm Cancel",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirm != MessageBoxResult.Yes)
                    return;
            }

            // Clear all fields
            EmpId.Text = "";
            EmpName.Text = "";
            EmpNIC.Text = "";
            tempFingerId = null;

            CustomMessageBox.Show("Registration cancelled");
            SystemServices.Log("Employee registration cancelled");
        }
    }
}