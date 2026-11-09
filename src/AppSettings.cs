using System;
using System.IO;
using System.Text.Json;
using Microsoft.Win32;

namespace NetSpeedWidget
{
    public class AppSettings
    {
        public double WindowLeft { get; set; } = -1;
        public double WindowTop { get; set; } = -1;
        public bool IsLocked { get; set; } = false;
        public string Theme { get; set; } = "dark"; // "dark", "transparent", "light"
        public string SpeedUnit { get; set; } = "auto"; // "auto", "kb", "mb"
        public double RefreshIntervalSeconds { get; set; } = 1.0;
        public bool StartWithWindows { get; set; } = true;
        public string SelectedAdapterId { get; set; } = ""; // Empty = all active
        public bool CompactMode { get; set; } = false;

        private static readonly string SettingsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "NetSpeedWidget"
        );
        private static readonly string SettingsFile = Path.Combine(SettingsFolder, "config.json");
        private const string StartupRunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "NetSpeedWidget";

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    string json = File.ReadAllText(SettingsFile);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null)
                    {
                        return settings;
                    }
                }
            }
            catch
            {
                // Fallback to default
            }

            return new AppSettings();
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(SettingsFolder);
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFile, json);
            }
            catch
            {
                // Silently ignore or log
            }
        }

        public static void SetStartupRegistration(bool enable)
        {
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(StartupRunKey, true);
                if (key == null) return;

                if (enable)
                {
                    string? exePath = Environment.ProcessPath;
                    if (!string.IsNullOrEmpty(exePath))
                    {
                        key.SetValue(AppName, $"\"{exePath}\"");
                    }
                }
                else
                {
                    key.DeleteValue(AppName, false);
                }
            }
            catch
            {
                // Registry access error handling
            }
        }

        public static bool IsStartupRegistered()
        {
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(StartupRunKey, false);
                return key?.GetValue(AppName) != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
