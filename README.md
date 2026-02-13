<div align="center">

# 🏢 Automated Employee Attendance System

**A modern IoT-based attendance management solution powered by ESP8266 & Fingerprint Sensor with a WPF desktop application.**

[![.NET](https://img.shields.io/badge/.NET%208.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/WPF-0078D4?style=for-the-badge&logo=windows&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/)
[![Arduino](https://img.shields.io/badge/Arduino-00878F?style=for-the-badge&logo=arduino&logoColor=white)](https://app.arduino.cc/sketches/0de4f97f-d7a1-42d7-9aef-7a3ac2f73e6a?view-mode=preview)
[![ESP8266](https://img.shields.io/badge/ESP8266-E7352C?style=for-the-badge&logo=espressif&logoColor=white)](https://www.espressif.com/)
[![SQLite](https://img.shields.io/badge/SQLite-003B57?style=for-the-badge&logo=sqlite&logoColor=white)](https://www.sqlite.org/)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)

</div>



## 📸 Screenshots

<div align="center">

<img width="960" alt="login" src="https://github.com/user-attachments/assets/1c12a5a8-6e64-4dd0-872d-038d7ac82410" />

<img width="960" alt="Register Employee page" src="https://github.com/user-attachments/assets/d5152100-28a0-48a3-adb3-79dae548ec3b" />

</div>

---

## 📖 Overview

The **Automated Employee Attendance System** is a complete end-to-end solution that automates employee attendance tracking using fingerprint biometric authentication. It bridges IoT hardware (ESP8266 + Fingerprint Sensor) with a feature-rich WPF desktop application, eliminating manual attendance processes and providing real-time insights.

---

## ✨ Features

### 🖥️ Desktop Application (WPF)
- **📊 Dashboard** - Real-time attendance statistics with interactive charts (LiveCharts)
- **👥 Employee Management** — Add, edit, and manage employee profiles with fingerprint enrollment
- **📋 Attendance Tracking** — Automatic check-in/check-out recording with working hour calculations
- **📄 Report Generation** — Generate professional PDF attendance reports (QuestPDF)
- **👤 User Management** — Role-based access control with granular permissions
- **⚙️ Settings & Device Management** — Scan, configure, and manage ESP devices over WiFi
- **🌙 Theme Support** — Light & Dark mode with smooth transitions
- **🔔 Desktop Notifications** — Real-time toast notifications for attendance events
- **📡 WiFi Configuration Wizard** — Seamless ESP device WiFi setup from the app

### 🔧 IoT Hardware (ESP8266)
- **Fingerprint scanning** via biometric sensor
- **LCD display** for real-time user feedback
- **AP Mode** for initial WiFi configuration
- **STA Mode** for normal operation on local network
- **REST API** endpoints for communication with desktop app
- **WiFi network scanning** for easy setup

---

## 🏗️ System Architecture

```
┌─────────────────────┐        HTTP/REST        ┌──────────────────────┐
│   WPF Desktop App   │ ◄────────────────────► │   ESP8266 Module     │
│                     │      (JSON API)         │                      │
│  • Dashboard        │                         │  • Fingerprint       │
│  • Employee Mgmt    │                         │    Sensor            │
│  • Attendance View  │                         │  • LCD Display       │
│  • Reports (PDF)    │                         │  • WiFi (AP/STA)     │
│  • Settings         │                         │  • REST API Server   │
│  • User Management  │                         │                      │
└────────┬────────────┘                         └──────────────────────┘
         │
         ▼
┌─────────────────────┐
│   SQLite Database   │
│                     │
│  • Employees        │
│  • Attendance       │
│  • Users            │
│  • Device Config    │
└─────────────────────┘
```

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| **Desktop App** | C# / .NET 8.0 / WPF |
| **UI Framework** | XAML with custom styles & animations |
| **Charts** | LiveCharts.WPF |
| **PDF Reports** | QuestPDF |
| **Database** | SQLite (Microsoft.Data.Sqlite) |
| **IoT Firmware** | Arduino / C++ (ESP8266) |
| **Communication** | HTTP REST API (JSON) |
| **Notifications** | Windows Toast Notifications |
| **Font** | Poppins (Google Fonts) |

---

## 📂 Project Structure

```
📦 Automated Employee Attendance System
├── 📁 Models/
│   ├── Device.cs              # ESP device & attendance models
│   ├── Employee.cs            # Employee data model
│   └── User.cs                # User authentication model
├── 📁 Services/
│   ├── AttendanceCalculationService.cs   # Working hours & status calculation
│   ├── DatabaseService.cs               # SQLite database operations
│   ├── DeviceService.cs                 # Device config save/load (JSON)
│   ├── ESP_Services.cs                  # ESP8266 HTTP communication
│   ├── SystemServices.cs               # Logging & system utilities
│   └── UserService.cs                  # User authentication & management
├── 📁 Notification/
│   └── NotificationHelper.cs  # Windows toast notifications
├── 📁 Style/
│   ├── MainStyle.xaml         # Global styles & themes
│   └── HomeButtonStyle.xaml   # Navigation button styles
├── 📁 UI/                     # Icons, images & animated GIFs
├── 📁 Poppins/                # Poppins font family
├── LoginWindow.xaml           # Login screen with animations
├── MainWindow.xaml            # Main shell with navigation
├── DashboardWindow.xaml       # Dashboard with charts
├── EmployeeWindow.xaml        # Employee management
├── AttendanceView.xaml        # Attendance records view
├── AttendanceEditWindow.xaml  # Edit attendance entries
├── SettingsWindow.xaml        # App & device settings
├── UserManagement.xaml        # User role management
├── WiFiScanWindow.xaml        # WiFi setup wizard
├── CustomMessageBox.xaml      # Themed message dialogs
├── ThemeManager.cs            # Light/Dark theme manager
└── App.xaml                   # Application entry point
```

---

## 🚀 Getting Started

### Prerequisites

- **Windows 10** (Build 17763+)
- [**.NET 8.0 SDK**](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Visual Studio 2022+** with WPF workload
- **ESP8266** board with Fingerprint Sensor (for hardware integration)
- **Arduino IDE** (for flashing ESP firmware)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/IoT-Innovates/automated-employee-attendance-system.git
   cd automated-employee-attendance-system
   ```

2. **Open the solution**
   ```
   Open "Automated Employee Attendance System.slnx" in Visual Studio
   ```

3. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

4. **Build & Run**
   ```bash
   dotnet run
   ```

5. **Default Login**
   ```
   Username: admin
   Password: admin
   ```

### ESP8266 Setup

1. Flash the Arduino firmware to your ESP8266 board
2. Power on the ESP — it starts in **AP Mode** (`Attendance-SETUP`)
3. Open the desktop app → **Settings** → **Add Device**
4. The app will detect the ESP in AP mode
5. Configure WiFi credentials through the built-in WiFi wizard
6. The ESP restarts in **STA Mode** and connects to your network
7. The app automatically detects and saves the device

> 📘 See [`ESP_FIRMWARE_UPDATE.md`](ESP_FIRMWARE_UPDATE.md) for firmware details and [`QUICK_START_GUIDE.md`](QUICK_START_GUIDE.md) for the full setup walkthrough.

---

## 📦 NuGet Packages

| Package | Purpose |
|---|---|
| `Microsoft.Data.Sqlite` | SQLite database access |
| `Newtonsoft.Json` | JSON serialization |
| `QuestPDF` | PDF report generation |
| `LiveCharts.Wpf` | Interactive dashboard charts |
| `WpfAnimatedGif` | Animated GIF support in UI |
| `XamlAnimatedGif` | XAML-based GIF animations |
| `Hardcodet.Wpf.TaskbarNotification` | System tray notifications |
| `Microsoft.Toolkit.Uwp.Notifications` | Windows toast notifications |
| `System.IO.Ports` | Serial port communication |

---

## 🤝 Contributing

Contributions are welcome! Feel free to open an issue or submit a pull request.

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/new-feature`)
3. Commit your changes (`git commit -m 'Add new feature'`)
4. Push to the branch (`git push origin feature/new-feature`)
5. Open a Pull Request

---

## 📝 License

This project is open source and available under the [MIT License](LICENSE).

---

<div align="center">

**Made by [IoT Innovates](https://github.com/IoT-Innovates)**

</div>
