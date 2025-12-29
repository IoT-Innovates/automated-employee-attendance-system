using Automated_Employee_Attendance_System;
using Automated_Employee_Attendance_System.Services;
using Automated_Employee_Attendance_System.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json; // Add this using directive
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
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : UserControl
    {

        HttpClient client = new ESP_Services().client;
        string espBaseUrl = new ESP_Services().espBaseUrl;


        public SettingsWindow()
        {


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
            InitializeComponent();
            ThemeManager.ApplyTheme(this);
            this.Loaded += Window_Loaded;
        }



        #region Theme Management

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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
        }

        private void DarkRadio_Checked(object sender, RoutedEventArgs e)
        {
            ThemeManager.CurrentTheme = ThemeMode.Dark;
            ThemeManager.UpdateAllWindows();
        }

        private void SystemRadio_Checked(object sender, RoutedEventArgs e)
        {
            ThemeManager.CurrentTheme = ThemeMode.SystemDefault;
            ThemeManager.UpdateAllWindows();
        }

        #endregion


        #region Time

        private async void SyncTime_Click(object sender, RoutedEventArgs e)
        {
            var now = DateTime.Now;

            var json = new
            {
                y = now.Year,
                mo = now.Month,
                d = now.Day,
                h = now.Hour,
                mi = now.Minute,
                s = now.Second
            };

            string body = JsonSerializer.Serialize(json);

            var res = await client.PostAsync(
                espBaseUrl + "/settime",
                new StringContent(body, Encoding.UTF8, "application/json")
            );

            MessageBox.Show("Time Synced");
        }

        #endregion



        void LoadStatus()
        {
            if (StatusBox == null)
                return;

            StatusBox.Text = SystemServices.ReadAll();
        }


    }
}
