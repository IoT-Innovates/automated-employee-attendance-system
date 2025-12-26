using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System;

namespace Automated_Employee_Attendance_System
{
    public enum ThemeMode
    {
        SystemDefault,
        Light,
        Dark
    }

    public static class ThemeManager
    {
        #region Constructor and System Event
        static ThemeManager()
        {
            // Subscribe to system theme changes
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        }

        private static void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            // Update all windows when general user preferences change
            if (e.Category == UserPreferenceCategory.General)
                UpdateAllWindows();
        }
        #endregion

        #region Current Theme Property
        public static ThemeMode CurrentTheme
        {
            get
            {
                // Read saved theme from settings
                string saved = Properties.Settings.Default.AppTheme;
                if (string.IsNullOrEmpty(saved))
                    return ThemeMode.SystemDefault;

                if (Enum.TryParse(saved, out ThemeMode mode))
                    return mode;

                return ThemeMode.SystemDefault;
            }
            set
            {
                // Save theme to settings
                Properties.Settings.Default.AppTheme = value.ToString();
                Properties.Settings.Default.Save();
            }
        }
        #endregion

        #region Apply Theme to Window
        public static void ApplyTheme(DependencyObject obj)
        {
            ThemeMode mode = CurrentTheme;
            bool isLight;

            if (mode == ThemeMode.SystemDefault)
            {
                var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");

                isLight = true;
                if (key != null)
                {
                    var value = key.GetValue("SystemUsesLightTheme");
                    isLight = value != null && (int)value == 1;
                }
            }
            else
            {
                isLight = (mode == ThemeMode.Light);
            }

            ApplyThemeColorsAndImages(obj, isLight);
        }

        #endregion

        #region Apply Colors and Images
        private static void ApplyThemeColorsAndImages(DependencyObject parent, bool isLight)
        {


            #region All Colors


            Application.Current.Resources["ContBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#f3f3f7" : "#000000"));
            Application.Current.Resources["ThemeBackgroundDotsColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#33252525" : "#30FFFFFF"));
            Application.Current.Resources["Background"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#FFFFFF" : "#0a0a0a"));

            Application.Current.Resources["SystemGridForground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#111111" : "#FFFFFF"));
            Application.Current.Resources["Forground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#111111" : "#FFFFFF"));

            Application.Current.Resources["IsMouseOver"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#f3f3f7" : "#1a1a1a"));
            Application.Current.Resources["BorderBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#f3f3f7" : "#2b2b2b"));

            #endregion




            // Recursively update all images in window
            UpdateImagesRecursive(parent, isLight);

        }

        #endregion

        #region Update Images Recursively
        private static void UpdateImagesRecursive(DependencyObject parent, bool isLight)
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is Image img && img.Tag != null)
                {
                    // Tag format: "Light:Path;Dark:Path"
                    string[] paths = img.Tag.ToString().Split(';');
                    string selectedPath = isLight
                        ? paths[0].Replace("Light:", "")
                        : paths[1].Replace("Dark:", "");

                    img.Source = new BitmapImage(new System.Uri(selectedPath, System.UriKind.Relative));
                }

                UpdateImagesRecursive(child, isLight);
            }
        }
        #endregion

        #region Update All Open Windows
        public static void UpdateAllWindows()
        {
            foreach (var win in Application.Current.Windows.OfType<Window>())
                ApplyTheme(win);
        }
        #endregion
    }
}
