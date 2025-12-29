using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Text.Json;



namespace Automated_Employee_Attendance_System.Services
{


    public class ESP_Services
    {
        public Action<string>? OnStatusChanged;

        public dynamic SSID;
       
        public dynamic PASS;
        public HttpClient client = new HttpClient();
        public string espBaseUrl;

        public async Task DetectESP()
        {
            OnStatusChanged?.Invoke("Searching ESP..");
            SystemServices.Log("Searching ESP..");
           

            if (await Ping("http://192.168.4.1/discover"))
            {
                OnStatusChanged?.Invoke("ESP Found (AP Mode)");
                SystemServices.Log("ESP Found (AP Mode)");
              
                espBaseUrl = "http://192.168.4.1";
               
                return;
            }


           
            OnStatusChanged?.Invoke("Scanning LAN...");

            // 🔹 LAN MODE
            for (int i = 1; i < 255; i++)
            {
                string ip = $"http://192.168.1.37/discover";
                if (await Ping(ip))
                {
                  
                    OnStatusChanged?.Invoke($"ESP Found on LAN ({ip})");
                    SystemServices.Log($"ESP Found on LAN ({ip})");
                    espBaseUrl = ip.Replace("/discover", "");
                    


                    return;
                }

            }
            OnStatusChanged?.Invoke("ESP Not Found");

           
        }


        async Task<bool> Ping(string url)
        {
            try
            {
                var res = await client.GetAsync(url);
                return res.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        private async void SendWiFi_Click(object sender, RoutedEventArgs e)
        {
            var data = new
            {
                ssid = SSID.Text,
                pass = PASS.Password
            };

            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            await client.PostAsync("http://192.168.4.1/setwifi", content);

            OnStatusChanged?.Invoke("WiFi Sent. Waiting restart...");
            SystemServices.Log("WiFi Sent. Waiting restart...");
            
            await Task.Delay(10000);
            await DetectESP();
        }

    }





}
