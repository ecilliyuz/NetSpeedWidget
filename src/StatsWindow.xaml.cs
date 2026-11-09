using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Input;

namespace NetSpeedWidget
{
    public partial class StatsWindow : Window
    {
        public StatsWindow()
        {
            InitializeComponent();
            MouseDown += (s, e) => { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); };
        }

        public void UpdateStats(NetworkSpeedEventArgs args)
        {
            Dispatcher.Invoke(() =>
            {
                TxtAdapter.Text = string.IsNullOrEmpty(args.ActiveAdapterName) ? "Tüm Ağlar" : args.ActiveAdapterName;
                TxtIpAddress.Text = GetLocalIPv4();
                TxtDownload.Text = $"{args.DownloadFormatted} | Toplam: {NetworkMonitor.FormatDataSize(args.TotalSessionDownloadBytes)}";
                TxtUpload.Text = $"{args.UploadFormatted} | Toplam: {NetworkMonitor.FormatDataSize(args.TotalSessionUploadBytes)}";
            });
        }

        private string GetLocalIPv4()
        {
            try
            {
                var interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (var ni in interfaces)
                {
                    if (ni.OperationalStatus != OperationalStatus.Up ||
                        ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                        continue;

                    var ipProps = ni.GetIPProperties();
                    foreach (var addr in ipProps.UnicastAddresses)
                    {
                        if (addr.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return addr.Address.ToString();
                        }
                    }
                }
            }
            catch { }

            return "127.0.0.1";
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
