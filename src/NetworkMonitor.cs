using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;

namespace NetSpeedWidget
{
    public class NetworkSpeedEventArgs : EventArgs
    {
        public double DownloadSpeedBytesPerSec { get; set; }
        public double UploadSpeedBytesPerSec { get; set; }
        public string DownloadFormatted { get; set; } = string.Empty;
        public string UploadFormatted { get; set; } = string.Empty;
        public long TotalSessionDownloadBytes { get; set; }
        public long TotalSessionUploadBytes { get; set; }
        public string ActiveAdapterName { get; set; } = string.Empty;
    }

    public class NetworkMonitor
    {
        private long _lastBytesReceived = -1;
        private long _lastBytesSent = -1;
        private readonly Stopwatch _stopwatch = new();

        private long _sessionStartBytesReceived = -1;
        private long _sessionStartBytesSent = -1;

        public event EventHandler<NetworkSpeedEventArgs>? SpeedUpdated;

        public string SelectedAdapterId { get; set; } = "";
        public string SpeedUnit { get; set; } = "auto";

        public void Start()
        {
            _stopwatch.Restart();
            GetTotalBytes(out _lastBytesReceived, out _lastBytesSent);
            _sessionStartBytesReceived = _lastBytesReceived;
            _sessionStartBytesSent = _lastBytesSent;
        }

        public void Update()
        {
            GetTotalBytes(out long currentReceived, out long currentSent);

            double elapsedSeconds = _stopwatch.Elapsed.TotalSeconds;
            _stopwatch.Restart();

            if (elapsedSeconds <= 0) return;

            double dlSpeed = 0;
            double ulSpeed = 0;

            if (_lastBytesReceived >= 0 && currentReceived >= _lastBytesReceived)
            {
                dlSpeed = (currentReceived - _lastBytesReceived) / elapsedSeconds;
            }

            if (_lastBytesSent >= 0 && currentSent >= _lastBytesSent)
            {
                ulSpeed = (currentSent - _lastBytesSent) / elapsedSeconds;
            }

            _lastBytesReceived = currentReceived;
            _lastBytesSent = currentSent;

            long sessionDl = Math.Max(0, currentReceived - _sessionStartBytesReceived);
            long sessionUl = Math.Max(0, currentSent - _sessionStartBytesSent);

            string activeName = GetPrimaryAdapterName();

            var args = new NetworkSpeedEventArgs
            {
                DownloadSpeedBytesPerSec = dlSpeed,
                UploadSpeedBytesPerSec = ulSpeed,
                DownloadFormatted = FormatSpeed(dlSpeed, SpeedUnit),
                UploadFormatted = FormatSpeed(ulSpeed, SpeedUnit),
                TotalSessionDownloadBytes = sessionDl,
                TotalSessionUploadBytes = sessionUl,
                ActiveAdapterName = activeName
            };

            SpeedUpdated?.Invoke(this, args);
        }

        private void GetTotalBytes(out long totalRecv, out long totalSent)
        {
            totalRecv = 0;
            totalSent = 0;

            try
            {
                var interfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (var ni in interfaces)
                {
                    if (ni.OperationalStatus != OperationalStatus.Up)
                        continue;

                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                        ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                        continue;

                    if (!string.IsNullOrEmpty(SelectedAdapterId) && ni.Id != SelectedAdapterId)
                        continue;

                    var stats = ni.GetIPStatistics();
                    totalRecv += stats.BytesReceived;
                    totalSent += stats.BytesSent;
                }
            }
            catch
            {
                // Network query error safeguard
            }
        }

        public string GetPrimaryAdapterName()
        {
            try
            {
                var interfaces = NetworkInterface.GetAllNetworkInterfaces();

                if (!string.IsNullOrEmpty(SelectedAdapterId))
                {
                    var match = interfaces.FirstOrDefault(i => i.Id == SelectedAdapterId);
                    if (match != null) return match.Name;
                }

                var active = interfaces.FirstOrDefault(i =>
                    i.OperationalStatus == OperationalStatus.Up &&
                    i.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    i.NetworkInterfaceType != NetworkInterfaceType.Tunnel);

                return active?.Name ?? "Tüm Ağlar";
            }
            catch
            {
                return "Bilinmiyor";
            }
        }

        public List<(string Id, string Name)> GetAvailableAdapters()
        {
            var list = new List<(string Id, string Name)>
            {
                ("", "Tüm Bağdaştırıcılar (Otomatik)")
            };

            try
            {
                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                        ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                        continue;

                    list.Add((ni.Id, $"{ni.Name} ({ni.Description})"));
                }
            }
            catch { }

            return list;
        }

        public static string FormatSpeed(double bytesPerSec, string unitMode)
        {
            if (bytesPerSec < 0) bytesPerSec = 0;

            switch (unitMode.ToLowerInvariant())
            {
                case "kb":
                    return $"{bytesPerSec / 1024.0:F1} KB/s";
                case "mb":
                    return $"{bytesPerSec / (1024.0 * 1024.0):F2} MB/s";
                default: // "auto"
                    if (bytesPerSec < 1024.0)
                    {
                        return $"{bytesPerSec:F0} B/s";
                    }
                    else if (bytesPerSec < 1024.0 * 1024.0)
                    {
                        return $"{bytesPerSec / 1024.0:F1} KB/s";
                    }
                    else if (bytesPerSec < 1024.0 * 1024.0 * 1024.0)
                    {
                        return $"{bytesPerSec / (1024.0 * 1024.0):F1} MB/s";
                    }
                    else
                    {
                        return $"{bytesPerSec / (1024.0 * 1024.0 * 1024.0):F2} GB/s";
                    }
            }
        }

        public static string FormatDataSize(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes} B";
            if (bytes < 1024 * 1024)
                return $"{bytes / 1024.0:F1} KB";
            if (bytes < 1024 * 1024 * 1024)
                return $"{bytes / (1024.0 * 1024.0):F2} MB";
            return $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
        }
    }
}
