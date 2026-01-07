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
            await _espServices.ConnectToSavedDevice();
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
                    // Don't start the animation immediately - stop it initially
                    AnimationBehavior.SetRepeatBehavior(MyGifImage, System.Windows.Media.Animation.RepeatBehavior.Forever);
                    StopGifAnimation();
                });
            });
        }

        private void StartGifAnimation()
        {
            // AnimationBehavior does not have SetIsPaused, so use AutoStart property
            AnimationBehavior.SetAutoStart(MyGifImage, true);
        }

        private void StopGifAnimation()
        {
            // AnimationBehavior does not have SetIsPaused, so use AutoStart property
            AnimationBehavior.SetAutoStart(MyGifImage, false);
        }



        private async void ScanFingerprint_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(espBaseUrl))
            {
                Fingerinfo.Text = "ESP not connected. Please check the device connection.";
                return;
            }

            if (string.IsNullOrWhiteSpace(EmpId.Text) ||
                 string.IsNullOrWhiteSpace(EmpFirstName.Text) ||
                 string.IsNullOrWhiteSpace(EmpLastName.Text) ||
                 string.IsNullOrWhiteSpace(EmpEmail.Text))
            {
                Fingerinfo.Text = "Please enter Employee ID, Name, and Email first before scanning fingerprint.";
                return;
            }

            // Start GIF animation and update info text
            StartGifAnimation();
            Fingerinfo.Text = "Scanning in progress... Place finger on sensor and hold steady.";

            try
            {
                // ✅ FIX: Create separate HttpClient with longer timeout for fingerprint scanning
                using (var fingerprintClient = new HttpClient())
                {
                    // 30 seconds timeout for fingerprint enrollment (2 scans)
                    fingerprintClient.Timeout = TimeSpan.FromSeconds(30);

                    SystemServices.Log("Starting fingerprint enrollment...");

                    var res = await fingerprintClient.GetAsync($"{espBaseUrl}/scanFingerprint");

                    if (!res.IsSuccessStatusCode)
                    {
                        StopGifAnimation();
                        Fingerinfo.Text = "Fingerprint enrollment failed. Please try again.";
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

                        StopGifAnimation();
                        Fingerinfo.Text = $"✅ Fingerprint enrolled successfully!\n\nFinger ID: {tempFingerId}\nEmployee: {EmpFirstName.Text} {EmpLastName.Text}\n\nNow click 'Add Employee' to save the registration.";
                        SystemServices.Log($"Fingerprint enrolled with ID: {tempFingerId}");
                    }
                    else
                    {
                        tempFingerId = null;
                        StopGifAnimation();
                        Fingerinfo.Text = "❌ Invalid response from ESP - no finger_id returned. Please try again.";
                        SystemServices.Log($"ESP Response: {json}");
                    }
                }
            }
            catch (TaskCanceledException)
            {
                tempFingerId = null;
                StopGifAnimation();
                Fingerinfo.Text = "⏱️ Fingerprint scan timeout!\n\nPlease ensure:\n• Finger is placed correctly on sensor\n• Sensor is working properly\n• ESP is responding\n\nTry scanning again.";
                SystemServices.Log("Fingerprint scan timeout");
            }
            catch (Exception ex)
            {
                tempFingerId = null;
                StopGifAnimation();
                Fingerinfo.Text = $"❌ Scan Error: {ex.Message}\n\nPlease check the device connection and try again.";
                SystemServices.Log($"Fingerprint scan error: {ex.Message}");
            }
        }


        private async void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            // Validate fingerprint was scanned first
            if (tempFingerId == null)
            {
                CustomMessageBox.Show("Please scan fingerprint first!\n\nFingerprint must be enrolled before saving employee.");
                return;
            }
            if (string.IsNullOrWhiteSpace(EmpId.Text) ||
                string.IsNullOrWhiteSpace(EmpFirstName.Text) ||
                string.IsNullOrWhiteSpace(EmpLastName.Text) ||
                string.IsNullOrWhiteSpace(EmpEmail.Text))
            {
                CustomMessageBox.Show("Please fill all fields (ID, First Name, Last Name, Email)");
                return;
            }


            string fullName = $"{EmpFirstName.Text.Trim()} {EmpLastName.Text.Trim()}";


            try
            {
                // Create employee object with finger_id
                var employee = new Employee
                {
                    emp_id = EmpId.Text.Trim(),
                    name = fullName,
                    email = EmpEmail.Text.Trim(),
                    finger_id = tempFingerId.Value
                };

                // ✅ SAVE TO ESP (if connected)
                if (!string.IsNullOrEmpty(espBaseUrl))
                {
                    var json = JsonSerializer.Serialize(employee);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var res = await client.PostAsync($"{espBaseUrl}/addEmployee", content);

                    if (!res.IsSuccessStatusCode)
                    {
                        var errorBody = await res.Content.ReadAsStringAsync();
                        CustomMessageBox.Show($"Failed to save employee to ESP\n{errorBody}");
                        SystemServices.Log($"Employee save to ESP failed: {errorBody}");
                        return;
                    }

                    SystemServices.Log($"Employee saved to ESP: {employee.name} (ID: {employee.emp_id})");
                }

                // ✅ SAVE TO DATABASE
                DatabaseService.SaveEmployee(employee);

                CustomMessageBox.Show($"Employee registered successfully!\n\nName: {employee.name}\nID: {employee.emp_id}\nFinger ID: {employee.finger_id}\n\n✓ Saved to database\n✓ Saved to ESP");

                // Clear form and temp data
                EmpId.Text = "";
                EmpFirstName.Text = "";
                EmpLastName.Text = "";
                EmpEmail.Text = "";
                tempFingerId = null;

                // Reset fingerprint info text
                Fingerinfo.Text = "Place the employee's finger on the hardware sensor.";

                // Reload employee list
                await LoadEmployees();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Error saving employee: {ex.Message}");
                SystemServices.Log($"Employee save error: {ex.Message}");
            }
        }


        private async void DeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            var emp = (sender as Button)?.CommandParameter as Employee;
            if (emp == null)
            {
                CustomMessageBox.Show("No employee selected");
                return;
            }

            var confirm = MessageBox.Show(
                $"Delete employee {emp.name}?\n\nThis will also delete their fingerprint from the sensor and database.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
                return;

            try
            {
                bool deletedFromESP = false;

                // ✅ DELETE FROM ESP (only if connected)
                if (!string.IsNullOrEmpty(espBaseUrl))
                {
                    try
                    {
                        var json = JsonSerializer.Serialize(new { id = emp.emp_id.Trim() });
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        var res = await client.PostAsync($"{espBaseUrl}/deleteEmployee", content);
                        var body = await res.Content.ReadAsStringAsync();

                        if (res.IsSuccessStatusCode)
                        {
                            using var doc = JsonDocument.Parse(body);
                            var status = doc.RootElement.GetProperty("status").GetString();

                            if (status == "ok")
                            {
                                deletedFromESP = true;
                                SystemServices.Log($"Employee deleted from ESP: {emp.name}");
                            }
                        }
                        else
                        {
                            SystemServices.Log($"ESP delete failed (continuing with database): {body}");
                        }
                    }
                    catch (Exception espEx)
                    {
                        SystemServices.Log($"ESP delete error (continuing with database): {espEx.Message}");
                    }
                }

                // ✅ DELETE FROM DATABASE
                DatabaseService.DeleteEmployee(emp.emp_id);

                // Show appropriate success message based on ESP connection
                string message = deletedFromESP
                    ? $"Employee deleted successfully\n\nName: {emp.name}\n\n✓ Removed from database\n✓ Removed from ESP"
                    : $"Employee deleted from database\n\nName: {emp.name}\n\n✓ Removed from database\n⚠ ESP not connected - fingerprint remains on sensor";

                CustomMessageBox.Show(message);

                await LoadEmployees();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Error: {ex.Message}");
                SystemServices.Log($"Delete error: {ex.Message}");
            }
        }


        public async Task LoadEmployees()
        {
            try
            {
                // ✅ LOAD ONLY FROM DATABASE (no ESP sync)
                var employees = DatabaseService.GetAllEmployees();

                SystemServices.Log($"Loaded {employees.Count} employees from database");

                // ✅ Update UI on main thread
                await Dispatcher.InvokeAsync(() =>
                {
                    EmployeeGrid.ItemsSource = employees;
                });
            }
            catch (Exception ex)
            {
                SystemServices.Log($"Load employees error: {ex.Message}");

                await Dispatcher.InvokeAsync(() =>
                {
                    EmployeeGrid.ItemsSource = new List<Employee>();
                });
            }
        }


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

            EmpId.Text = "";
            EmpFirstName.Text = "";
            EmpLastName.Text = "";
            EmpEmail.Text = "";
            tempFingerId = null;

            // Reset fingerprint info text and stop animation
            StopGifAnimation();
            Fingerinfo.Text = "Place the employee's finger on the hardware sensor.";

            CustomMessageBox.Show("Registration cancelled");
            SystemServices.Log("Employee registration cancelled");
        }
    }
}