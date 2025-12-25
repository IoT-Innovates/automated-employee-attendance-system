using Automated_Employee_Attendance_System.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Automated_Employee_Attendance_System
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private User _user;

        public MainWindow(User user)
        {
            InitializeComponent();
            _user = user;
            ApplyAccess();
            ThemeManager.ApplyTheme(this);

            Dashbord_Tab.IsChecked = true;
            LoadView(new DashboardWindow());
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            LoadView(new DashboardWindow());
        }




        void ApplyAccess()
        {
            Dashbord_Tab.Visibility = _user.Dashbord ? Visibility.Visible : Visibility.Collapsed;
            Employee_Tab.Visibility = _user.Employee ? Visibility.Visible : Visibility.Collapsed;
            Attendance_Tab.Visibility = _user.Attendance ? Visibility.Visible : Visibility.Collapsed;
            Report_Tab.Visibility = _user.Report ? Visibility.Visible : Visibility.Collapsed;
            Settings_Tab.Visibility = _user.Settings ? Visibility.Visible : Visibility.Collapsed;
          
        }





        #region Navigation

        private void EmployeeWindow_Click(object sender, RoutedEventArgs e) => LoadView(new EmployeeWindow());
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
    }
}