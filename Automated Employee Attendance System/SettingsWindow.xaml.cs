using Automated_Employee_Attendance_System;
using Automated_Employee_Attendance_System.Services;
using Automated_Employee_Attendance_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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
using System.Windows.Threading;

namespace Automated_Employee_Attendance_System
{
    public partial class SettingsWindow : UserControl
    {
        private ESP_Services _espServices;

        public SettingsWindow()
        {
            InitializeComponent();

            _espServices = new ESP_Services();

            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };

            timer.Tick += (s, e) =>
            {
                StatusBox.Text = SystemServices.ReadAll();
            };

            timer.Start();

            LoadStatus();
            ThemeManager.ApplyTheme(this);
            this.Loaded += Window_Loaded;

            LoadSavedDevice();
        }

        private void LoadSavedDevice()
        {
            var device = DeviceService.Load();

            DeviceList.Items.Clear();

            if (device != null)
            {
                // ================= CARD BORDER =================
                var container = new Border
                {
                    Background = (Brush)Application.Current.Resources["CardBackground"],
                    BorderBrush = (Brush)Application.Current.Resources["BorderBrush"],
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(15),
                    Margin = new Thickness(0, 0, 0, 10)
                };

                // ================= MAIN GRID =================
                var mainGrid = new Grid();
                mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Icon
                mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Info
                mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Spacer
                mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Buttons

                // ================= ICON =================
                var iconText = new TextBlock
                {
                    Text = "\uE78B",
                    FontFamily = new FontFamily("Segoe MDL2 Assets"),
                    FontSize = 24,
                    Foreground = (Brush)Application.Current.Resources["Forground"],
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 15, 0)
                };
                Grid.SetColumn(iconText, 0);
                mainGrid.Children.Add(iconText);

                // ================= DEVICE INFO =================
                var infoPanel = new StackPanel
                {
                    VerticalAlignment = VerticalAlignment.Center
                };

                var nameText = new TextBlock
                {
                    Text = device.Name,
                    FontSize = 14,
                    FontFamily = new FontFamily("Microsoft Sans Serif"),
                    FontWeight = FontWeights.SemiBold,
                    Foreground = (Brush)Application.Current.Resources["Forground"]
                };
                infoPanel.Children.Add(nameText);

                var ipText = new TextBlock
                {
                    Text = device.IpAddress,
                    FontSize = 12,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(0, 2, 0, 0)
                };
                infoPanel.Children.Add(ipText);

                var idText = new TextBlock
                {
                    Text = $"ID: {device.DeviceId}",
                    FontSize = 11,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(0, 2, 0, 0)
                };
                infoPanel.Children.Add(idText);

                Grid.SetColumn(infoPanel, 1);
                mainGrid.Children.Add(infoPanel);

                // ================= BUTTONS (RIGHT SIDE) =================
                var buttonsPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right
                };

                var timeSyncBtn = new Button
                {
                    Content = "Time Sync",
                    Width = 100,
                    Height = 32,
                    Margin = new Thickness(0, 0, 8, 0),
                    Style = (Style)Application.Current.Resources["ActionButton"]
                };
                timeSyncBtn.Click += TimeSync_Click;

                var deleteBtn = new Button
                {
                    Content = "Delete",
                    Width = 90,
                    Height = 32,
                    Style = (Style)Application.Current.Resources["NavigationButtonStyle"]
                };
                deleteBtn.Click += DeleteDevice_Click;

                buttonsPanel.Children.Add(timeSyncBtn);
                buttonsPanel.Children.Add(deleteBtn);

                Grid.SetColumn(buttonsPanel, 3);
                mainGrid.Children.Add(buttonsPanel);

                // ================= FINAL =================
                container.Child = mainGrid;
                DeviceList.Items.Add(container);
                ScanStatusText.Text = "";
            }
            else
            {
                // ================= NO DEVICE CARD =================
                var noDeviceContainer = new Border
                {
                    Background = (Brush)Application.Current.Resources["CardBackground"],
                    BorderBrush = (Brush)Application.Current.Resources["BorderBrush"],
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(20),
                    Margin = new Thickness(0, 0, 0, 10)
                };

                var noDevicePanel = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var noDeviceIcon = new TextBlock
                {
                    Text = "📵",
                    FontSize = 32,
                    Foreground = (Brush)Application.Current.Resources["Forground"],
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                noDevicePanel.Children.Add(noDeviceIcon);

                var noDeviceText = new TextBlock
                {
                    Text = "No device configured",
                    Foreground = Brushes.Gray,
                    FontStyle = FontStyles.Italic,
                    FontSize = 13,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                noDevicePanel.Children.Add(noDeviceText);

                noDeviceContainer.Child = noDevicePanel;
                DeviceList.Items.Add(noDeviceContainer);
                ScanStatusText.Text = "";
            }
        }


        private async void AddDevice_Click(object sender, RoutedEventArgs e)
        {
            ScanStatusText.Text = "Scanning...";
            AddDeviceBtn.IsEnabled = false;

            var devices = await _espServices.ScanForDevices((progress) =>
            {
                Dispatcher.Invoke(() => ScanStatusText.Text = progress);
            });

            AddDeviceBtn.IsEnabled = true;

            if (devices.Count == 0)
            {
                CustomMessageBox.Show("No ESP devices found.\n\nMake sure the ESP is powered on and connected to the network.");
                ScanStatusText.Text = "No devices found";
                return;
            }

            var apDevice = devices.FirstOrDefault(d => d.Mode == "AP");

            if (apDevice != null)
            {
                var result = MessageBox.Show(
                    "ESP found in AP mode.\n\nDo you want to configure WiFi?",
                    "Setup Required",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var wifiWindow = new WiFiScanWindow();
                    if (wifiWindow.ShowDialog() == true && wifiWindow.WifiConfigured)
                    {
                        var device = wifiWindow.ConfiguredDevice;

                        if (device != null)
                        {
                            DeviceService.Save(device);
                            LoadSavedDevice();

                            // ✅ Auto sync time after adding device
                            await SyncDeviceTime(device);

                            CustomMessageBox.Show($"Device saved successfully!\n\nName: {device.Name}\nIP: {device.IpAddress}\nDevice ID: {device.DeviceId}\n\nTime synchronized!");
                            SystemServices.Log($"Device saved: {device.IpAddress} (ID: {device.DeviceId})");
                        }
                    }
                }

                ScanStatusText.Text = "";
                return;
            }

            if (devices.Count == 1)
            {
                var device = devices[0];
                DeviceService.Save(device);
                LoadSavedDevice();

                // ✅ Auto sync time after adding device
                await SyncDeviceTime(device);

                CustomMessageBox.Show($"Device saved successfully!\n\nIP: {device.IpAddress}\nDevice ID: {device.DeviceId}\n\nTime synchronized!");
                SystemServices.Log($"Device saved: {device.IpAddress} (ID: {device.DeviceId})");
                ScanStatusText.Text = "";
            }
            else
            {
                // Multiple devices found - show selection dialog
                var deviceWindow = new Window
                {
                    Title = "Select Device",
                    Width = 500,
                    Height = 300,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                var listBox = new ListBox();
                foreach (var dev in devices)
                {
                    listBox.Items.Add($"{dev.Name} ({dev.IpAddress}) - ID: {dev.DeviceId}");
                }

                var btn = new Button { Content = "Select", Margin = new Thickness(10) };
                btn.Click += async (s, ev) =>
                {
                    if (listBox.SelectedIndex >= 0)
                    {
                        var selected = devices[listBox.SelectedIndex];
                        DeviceService.Save(selected);
                        deviceWindow.Close();

                        // ✅ Auto sync time after selecting device
                        await SyncDeviceTime(selected);
                        LoadSavedDevice();
                        CustomMessageBox.Show("Device saved and time synchronized!");
                    }
                };

                var panel = new StackPanel();
                panel.Children.Add(listBox);
                panel.Children.Add(btn);
                deviceWindow.Content = panel;

                deviceWindow.ShowDialog();
                LoadSavedDevice();
                ScanStatusText.Text = "";
            }
        }

        // ✅ NEW: Time Sync Button Click Handler
        private async void TimeSync_Click(object sender, RoutedEventArgs e)
        {
            var device = DeviceService.Load();

            if (device == null)
            {
                CustomMessageBox.Show("No device configured");
                return;
            }

            ScanStatusText.Text = "Synchronizing time...";

            var success = await SyncDeviceTime(device);

            if (success)
            {
                CustomMessageBox.Show("Time synchronized successfully!");
                ScanStatusText.Text = "Time synced";
            }
            else
            {
                CustomMessageBox.Show("Failed to sync time.\n\nPlease check:\n- ESP is online\n- Network connection is stable");
                ScanStatusText.Text = "Time sync failed";
            }
        }

        // ✅ NEW: Sync Device Time Method
        private async Task<bool> SyncDeviceTime(Device device)
        {
            try
            {
                SystemServices.Log($"Syncing time to device: {device.IpAddress}");

                var now = DateTime.Now;

                var timeData = new
                {
                    y = now.Year,
                    mo = now.Month,
                    d = now.Day,
                    h = now.Hour,
                    mi = now.Minute,
                    s = now.Second
                };

                var json = JsonSerializer.Serialize(timeData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);

                    var response = await client.PostAsync($"{device.IpAddress}/settime", content);

                    if (response.IsSuccessStatusCode)
                    {
                        SystemServices.Log($"Time synced successfully: {now:yyyy-MM-dd HH:mm:ss}");
                        return true;
                    }
                    else
                    {
                        var errorBody = await response.Content.ReadAsStringAsync();
                        SystemServices.Log($"Time sync failed: {response.StatusCode} - {errorBody}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                SystemServices.Log($"Time sync error: {ex.Message}");
                return false;
            }
        }

        private void DeleteDevice_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to delete this device?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                DeviceService.Delete();
                LoadSavedDevice();
                CustomMessageBox.Show("Device deleted successfully");
                SystemServices.Log("Device deleted");
            }
        }

        #region Theme Management

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            EmailBox.Text = Properties.Settings.Default.SavedEmail ?? "";
            AppPasswordBox.Password = Properties.Settings.Default.SavedAppPassword ?? "";

            SystemServices.Log("Email settings loaded from local settings");


            switch (ThemeManager.CurrentTheme)
            {
                case ThemeMode.Light:
                    LightRadio.IsChecked = true;
                    break;
                case ThemeMode.Dark:
                    DarkRadio.IsChecked = true;
                    break;
                case ThemeMode.SystemDefault:
                    SystemRadio.IsChecked = true;
                    break;
            }
        }

        private void LightRadio_Checked(object sender, RoutedEventArgs e)
        {
            ThemeManager.CurrentTheme = ThemeMode.Light;
            ThemeManager.UpdateAllWindows();
            LoadSavedDevice();
        }

        private void DarkRadio_Checked(object sender, RoutedEventArgs e)
        {
            ThemeManager.CurrentTheme = ThemeMode.Dark;
            ThemeManager.UpdateAllWindows();
            LoadSavedDevice();
        }

        private void SystemRadio_Checked(object sender, RoutedEventArgs e)
        {
            ThemeManager.CurrentTheme = ThemeMode.SystemDefault;
            ThemeManager.UpdateAllWindows();
            LoadSavedDevice();
        }

        #endregion

        void LoadStatus()
        {
            if (StatusBox == null)
                return;

            StatusBox.Text = SystemServices.ReadAll();
        }

        private async void SaveEmail_Click(object sender, RoutedEventArgs e)
        {
            var device = DeviceService.Load();
            if (device == null)
            {
                CustomMessageBox.Show("No ESP device connected");
                return;
            }

            // ✅ Validate inputs
            if (string.IsNullOrWhiteSpace(EmailBox.Text))
            {
                CustomMessageBox.Show("Please enter an email address");
                return;
            }

            if (string.IsNullOrWhiteSpace(AppPasswordBox.Password))
            {
                CustomMessageBox.Show("Please enter an app password");
                return;
            }

            var email = EmailBox.Text.Trim();
            var password = AppPasswordBox.Password.Trim();

            // ================== 🔹 SAVE TO PC SETTINGS ==================
            Properties.Settings.Default.SavedEmail = email;
            Properties.Settings.Default.SavedAppPassword = password;
            Properties.Settings.Default.Save();

            SystemServices.Log("Email config saved to local settings");

            // ================== 🔹 SEND TO ESP ==================
            var data = new
            {
                email = email,
                app_password = password
            };

            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);

                    var res = await client.PostAsync($"{device.IpAddress}/setEmailConfig", content);

                    if (res.IsSuccessStatusCode)
                    {
                        CustomMessageBox.Show("Email settings saved successfully!");
                        SystemServices.Log($"Email config saved to ESP: {email}");
                    }
                    else
                    {
                        var errorBody = await res.Content.ReadAsStringAsync();
                        CustomMessageBox.Show($"Failed to save email settings.\n\nStatus: {res.StatusCode}\nError: {errorBody}");
                        SystemServices.Log($"Email config save failed: {res.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Connection error:\n\n{ex.Message}");
                SystemServices.Log($"Email config error: {ex.Message}");
            }
        }



    }
}﻿
