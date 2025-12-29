using Automated_Employee_Attendance_System.Services;
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
using XamlAnimatedGif;
using System.Windows.Shapes;

namespace Automated_Employee_Attendance_System
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private string UserNameText;

        public LoginWindow()
        {
            InitializeComponent();
            SeedAdmin();
            Loaded += LoadingWindow_Loaded; // window render වෙන විට
            ThemeManager.ApplyTheme(this);
        }

        void SeedAdmin()
        {
            var users = UserService.Load();
            if (!users.Any())
            {
                users.Add(new Models.User
                {
                    Username = "admin",
                    PasswordHash = UserService.Hash("admin"),
                    Dashbord = true,
                    Employee = true,
                    Attendance = true,
                    Report = true,
                    Settings = true

                });
                UserService.Save(users);
            }
        }



        private async void LoadingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // GIF load background thread
            await Task.Run(() =>
            {
                string baseFolder = AppDomain.CurrentDomain.BaseDirectory;
                string gifPath = System.IO.Path.Combine(baseFolder, "UI", "time_tracker.gif");
                var gifUri = new Uri(gifPath, UriKind.Absolute);

                Dispatcher.Invoke(() =>
                {
                    AnimationBehavior.SetSourceUri(MyGifImage, gifUri);
                    AnimationBehavior.SetRepeatBehavior(MyGifImage, System.Windows.Media.Animation.RepeatBehavior.Forever);
                });
            });




        }

        private void Close_Click (object sender, RoutedEventArgs e)
        {
            Close();
        }




        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            var users = UserService.Load();

            var user = users.FirstOrDefault(u =>
                u.Username == UserName.Text &&
                u.PasswordHash == UserService.Hash(Password.Password));

            UserNameText = UserName.Text;



            if (user == null)
            {
                

                CustomMessageBox.Show("Invalid login");
                SystemServices.Log($"Loging Fail");
                return;
            }

            ((Button)sender).IsEnabled = false;


            new MainWindow(user).Show();
            SystemServices.Log($"Login {UserNameText}");
            Close();
        }
    }
}