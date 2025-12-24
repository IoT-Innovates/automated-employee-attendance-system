using Automated_Employee_Attendance_System;
using System;
using System.Collections.Generic;
using System.Linq;
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


namespace Automated_Employee_Attendance_System
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : UserControl
    {
        public SettingsWindow()
        {
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

    }
}
