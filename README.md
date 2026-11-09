# 🚀 NetSpeed Widget

> **A sleek, lightweight, native Windows taskbar widget displaying real-time Download and Upload network speeds right next to the system clock.**

[![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-0078D6?logo=windows)](https://microsoft.com)
[![Framework](https://img.shields.io/badge/.NET-8.0%20WPF-512BD4?logo=dotnet)](https://dotnet.microsoft.com)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Language](https://img.shields.io/badge/Language-C%23-239120?logo=csharp)](https://docs.microsoft.com/dotnet/csharp/)

[English](README.md) | [Türkçe Dokümantasyon](README.tr.md)

---

## 📸 Preview

```
┌────────────────────────┐
│ ↓ 14.2 MB/s            │  <- Real-time Download speed (Vibrant Green)
│ ↑  1.8 MB/s            │  <- Real-time Upload speed (Sky Blue)
└────────────────────────┘
  [ Placed seamlessly on Windows Taskbar right next to the System Clock ]
```

---

## ✨ Features

- **⚡ Real-time Speed Monitoring:** Live download (`↓`) and upload (`↑`) speeds with sub-second polling and zero jitter.
- **🕒 Seamless Taskbar Integration:** Automatically docks flush inside the taskbar next to the notification area and clock.
- **🖱️ Drag & Drop Anywhere:** Left-click and drag the widget to any position on your monitor.
- **🔒 Position Locking:** Lock position via right-click context menu to prevent accidental movement.
- **⏱️ Auto-Snap to Clock:** Instantly snaps back to its default taskbar position next to the clock with one click.
- **🎨 Custom Visual Themes:**
  - **Translucent Dark (Default):** Fluent acrylic dark theme designed to match Windows 11 dark taskbar.
  - **Full Transparent:** Background disappears completely, displaying speed numbers directly on the taskbar.
  - **Light Theme:** Optimized for light-mode Windows taskbars.
  - **Compact Mode:** Single-row layout (`↓ 14.2M  ↑ 1.8M`) for minimal vertical footprint.
- **📊 Automatic & Fixed Units:** Supports Auto-scaling (`B/s`, `KB/s`, `MB/s`, `GB/s`), Fixed `KB/s`, or Fixed `MB/s`.
- **🌐 Network Details & Statistics:** Double-click the widget or select *"Network Stats"* to inspect:
  - Active network adapter name and type (Wi-Fi, Ethernet, etc.)
  - Local IPv4 address
  - Cumulative session download and upload data totals
- **🔔 System Tray Icon:** Companion tray icon with tooltip live statistics and quick controls.
- **🚀 Windows Startup Support:** One-click toggle to launch automatically when you sign into Windows.
- **🍃 Minimal Resource Consumption:** Native C# .NET WPF app using ~0.0% CPU and lightweight memory footprint.

---

## 📦 Quick Installation

### Option 1: One-Click Installer (Recommended)
1. Clone or download this repository.
2. Double-click **`Kurulum.bat`** (or run `Kurulum.bat` in Command Prompt / PowerShell).
3. The script will:
   - Install the files into `%LocalAppData%\NetSpeedWidget`.
   - Register the application in Windows Startup.
   - Create Desktop and Start Menu shortcuts.
   - Launch the widget immediately on your taskbar!

### Option 2: Portable / Standalone
You can run the precompiled executable directly without installation:
- Simply launch `publish\NetSpeedWidget.exe`.

---

## 🛠️ Controls & Shortcuts

| Action | Result |
| :--- | :--- |
| **Left Click + Drag** | Move widget to any location on screen *(when unlocked)* |
| **Double Click** | Open **Network Statistics** window (IP, adapter, total transferred) |
| **Right Click** (Widget or Tray Icon) | Open Context Menu (Settings, Themes, Units, Startup) |

---

## ⚙️ Context Menu Options

Right-click on the widget or the tray icon to access:
- **🔒 Lock Position:** Prevents mouse dragging once positioned.
- **⏱️ Snap next to Clock:** Automatically docks to the right side of the taskbar.
- **🎨 Appearance:** Switch between Dark, Transparent, Light, and Compact modes.
- **⚡ Speed Unit:** Choose between Auto-detect, Fixed KB/s, or Fixed MB/s.
- **⏱️ Refresh Interval:** Adjust polling rate (0.5s Fast, 1.0s Default, 2.0s Power-saver).
- **🌐 Network Stats & Details:** View adapter information and session totals.
- **🚀 Run at Windows Startup:** Enable/disable auto-start on Windows login.
- **❌ Exit:** Safely closes the application.

---

## 🗑️ Uninstallation

To remove NetSpeed Widget cleanly from your system:
1. Double-click **`Kaldir.bat`**.
2. This stops the process, cleans up shortcuts, removes the startup entry, and deletes local files.

---

## 💻 Building from Source

### Prerequisites
- Windows 10 (1809+) or Windows 11
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later

### Build Instructions
```powershell
# Clone the repository
git clone https://github.com/ecilliyuz/NetSpeedWidget.git
cd NetSpeedWidget

# Build Release version
dotnet build src/NetSpeedWidget.csproj -c Release

# Publish ready-to-run folder
dotnet publish src/NetSpeedWidget.csproj -c Release -o publish
```

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).
