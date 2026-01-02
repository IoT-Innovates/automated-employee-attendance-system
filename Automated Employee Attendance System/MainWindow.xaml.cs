using Automated_Employee_Attendance_System.Models;
using Automated_Employee_Attendance_System.Services;
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using XamlAnimatedGif;
using System.Drawing;

namespace Automated_Employee_Attendance_System
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TaskbarIcon _trayIcon;
        private User _user;
        private readonly ESP_Services _esp = new ESP_Services();

        public MainWindow(User user)
        {
            InitializeComponent();
            _user = user;
            
            _esp.OnStatusChanged = SetStatus;
            
            ApplyAccess();
            ThemeManager.ApplyTheme(this);

            Dashbord_Tab.IsChecked = true;
            LoadView(new DashboardWindow());

            Loaded += async (_, _) => await _esp.ConnectToSavedDevice();
            SetupTrayIcon();
        }








        void ApplyAccess()
        {
            Dashbord_Tab.Visibility = _user.Dashbord ? Visibility.Visible : Visibility.Collapsed;
            Employee_Tab.Visibility = _user.Employee ? Visibility.Visible : Visibility.Collapsed;
            Attendance_Tab.Visibility = _user.Attendance ? Visibility.Visible : Visibility.Collapsed;
            UserManagement_Tab.Visibility = _user.Report ? Visibility.Visible : Visibility.Collapsed;
            Settings_Tab.Visibility = _user.Settings ? Visibility.Visible : Visibility.Collapsed;
          
        }





        private void SetupTrayIcon()
        {
            _trayIcon = new TaskbarIcon();
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UI", "logo.ico");
            _trayIcon.Icon = new Icon(iconPath);
            _trayIcon.ToolTipText = "My WPF App";

            _trayIcon.TrayLeftMouseUp += (s, e) =>
            {
                this.Show();
                this.WindowState = WindowState.Normal;
                this.Activate();
            };

            var contextMenu = new ContextMenu();

            var menuOpen = new MenuItem { Header = "Open" };
            menuOpen.Click += (s, e) => ShowWindow();


            var menuExit = new MenuItem { Header = "Exit" };
            menuExit.Click += (s, e) =>
            {
                _trayIcon.Dispose();
                Application.Current.Shutdown();
            };

            contextMenu.Items.Add(menuOpen);
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(menuExit);

            _trayIcon.ContextMenu = contextMenu;
        }


        private void ShowWindow()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        #region Navigation

        private void Dashboard_Click(object sender, RoutedEventArgs e) => LoadView(new DashboardWindow());
        private void EmployeeWindow_Click(object sender, RoutedEventArgs e) => LoadView(new EmployeeWindow());
        private void Attendance_Click(object sender, RoutedEventArgs e) => LoadView(new AttendanceView());
        private void Report_Click(object sender, RoutedEventArgs e) => LoadView(new UserManagement());
        private void Settings_Click(object sender, RoutedEventArgs e) => LoadView(new SettingsWindow());

        private void LoadView(UserControl view)
        {
            TranslateTransform trans = new TranslateTransform();
            view.RenderTransform = trans;
            view.Opacity = 0;

            DoubleAnimation slideAnim = new DoubleAnimation
            {
                From = 50,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(400),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            DoubleAnimation fadeAnim = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(400)
            };

            MainContent.Content = view;

            trans.BeginAnimation(TranslateTransform.YProperty, slideAnim);
            view.BeginAnimation(UserControl.OpacityProperty, fadeAnim);
        }

        #endregion


        public void SetStatus(string text)
        {
            Dispatcher.Invoke(() => Status.Text = text);
        }



        #region Window Control

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;

        private bool _isMaximized = false;
        private double _previousLeft;
        private double _previousTop;
        private double _previousWidth;
        private double _previousHeight;

private void Maximize_Click(object sender, RoutedEventArgs e)
{
    if (_isMaximized)
    {
        // Restore to previous size and position
        this.Left = _previousLeft;
        this.Top = _previousTop;
        this.Width = _previousWidth;
        this.Height = _previousHeight;
        _isMaximized = false;
    }
    else
    {
        // get before maximizing
        _previousLeft = this.Left;
        _previousTop = this.Top;
        _previousWidth = this.Width;
        _previousHeight = this.Height;

        // Get the working area (screen minus taskbar)
        var workingArea = SystemParameters.WorkArea;

        // Set window position and size to working area
        this.Left = workingArea.Left;
        this.Top = workingArea.Top;
        this.Width = workingArea.Width;
        this.Height = workingArea.Height;
        
        _isMaximized = true;
    }
}

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            
            Hide();
        }

        #endregion
    }
}