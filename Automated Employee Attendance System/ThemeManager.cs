using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System;

namespace Automated_Employee_Attendance_System.Helpers
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
            #region Home Window Styles

            // HomeUi border background
            if (LogicalTreeHelper.FindLogicalNode(parent, "HomeUi") is Border homeUi)
            {
                homeUi.Background = isLight
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f3f3f7")) // Light
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0c0c0c")); // Dark
            }
         
            
            // TitleBar grid background
            if (LogicalTreeHelper.FindLogicalNode(parent, "TitleBar") is Border titleBar)
                
            {
                titleBar.Background = isLight
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f3f3f7")) // Light
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0c0c0c")); // Dark
            }
           

           
            // Maximize button foreground

            if (LogicalTreeHelper.FindLogicalNode(parent, "Maximize") is Control maximizeBtn)
                maximizeBtn.Foreground = isLight ? Brushes.Black : Brushes.White;

            // Minimize button foreground
            if (LogicalTreeHelper.FindLogicalNode(parent, "Minimize") is Control minimizeBtn)
                minimizeBtn.Foreground = isLight ? Brushes.Black : Brushes.White;

            // Close button foreground
            if (LogicalTreeHelper.FindLogicalNode(parent, "Colose") is Control closeBtn)
                closeBtn.Foreground = isLight ? Brushes.Black : Brushes.White;

            // Application-wide RadioButton background color
            Application.Current.Resources["RadioButtonBackground"] = isLight ? Brushes.Gray : Brushes.White;

            #endregion

            #region HomeUi Styles
            // Panels background
            if (LogicalTreeHelper.FindLogicalNode(parent, "Panel_1") is Panel panel1)
                panel1.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#FFFFFF" : "#0F0F0F"));

            if (LogicalTreeHelper.FindLogicalNode(parent, "Panel_2") is Panel panel2)
                panel2.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#FFFFFF" : "#0F0F0F"));

            // Birthday border
            if (LogicalTreeHelper.FindLogicalNode(parent, "BirthdayBorder") is Border birthdayBorder)
                birthdayBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#FFFFFF" : "#0F0F0F"));

            // Foreground colors
            if (LogicalTreeHelper.FindLogicalNode(parent, "Countofjob") is TextBlock countOfJob)
                countOfJob.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#0F0F0F" : "#FFFFFF"));

            if (LogicalTreeHelper.FindLogicalNode(parent, "countjobname") is TextBlock countJobName)
                countJobName.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#0F0F0F" : "#FFFFFF"));

            if (LogicalTreeHelper.FindLogicalNode(parent, "birthdayname") is TextBlock birthdayName)
                birthdayName.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#0F0F0F" : "#FFFFFF"));

            // Panels background
            Application.Current.Resources["PanelBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#FFFFFF" : "#0d0d0d"));

            // Birthday border
            Application.Current.Resources["BirthdayBoderBackColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#FFFFFF" : "#0F0F0F"));

            // Foreground colors
            Application.Current.Resources["CountofjobForeground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#0F0F0F" : "#FFFFFF"));
            Application.Current.Resources["BirthdayNameForeground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#0F0F0F" : "#FFFFFF"));

            // Other resources
            Application.Current.Resources["Barcolor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#0F0F0F" : "#FFFFFF"));
            Application.Current.Resources["BirthdayBoderColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#0F0F0F" : "#FFFFFF"));
            Application.Current.Resources["BirthdayBoderHoverColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "WhiteSmoke" : "#19000000"));

            #endregion

            #region Setting Winodow Styles

            // Settings Window background
            


                 if (LogicalTreeHelper.FindLogicalNode(parent, "SettingsWindowForground") is Window SettingsWindowForground)
            {
                SettingsWindowForground.Foreground = isLight
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#111111")) // Light
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF")); // Dark
            }


            Application.Current.Resources["BorderBrushColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#111111" : "#FFFFFF"));
            Application.Current.Resources["SettingsBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#FFFFFF" : "#111111"));

            #endregion

            #region All Colors


            Application.Current.Resources["ThemeBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#f3f3f7" : "#0c0c0c"));
            Application.Current.Resources["Background"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? "#FFFFFF" : "#111111"));
        
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
