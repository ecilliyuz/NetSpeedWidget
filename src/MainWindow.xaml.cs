using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Forms = System.Windows.Forms;
using WpfCursors = System.Windows.Input.Cursors;

namespace NetSpeedWidget
{
    public partial class MainWindow : Window
    {
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private readonly AppSettings _settings;
        private readonly NetworkMonitor _networkMonitor;
        private readonly DispatcherTimer _timer;
        private Forms.NotifyIcon? _notifyIcon;
        private StatsWindow? _statsWindow;
        private NetworkSpeedEventArgs? _lastArgs;

        public MainWindow()
        {
            InitializeComponent();

            _settings = AppSettings.Load();
            _networkMonitor = new NetworkMonitor
            {
                SpeedUnit = _settings.SpeedUnit,
                SelectedAdapterId = _settings.SelectedAdapterId
            };

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(_settings.RefreshIntervalSeconds)
            };
            _timer.Tick += (s, e) => _networkMonitor.Update();

            Loaded += MainWindow_Loaded;
            SourceInitialized += MainWindow_SourceInitialized;
            LocationChanged += MainWindow_LocationChanged;
            MouseDown += MainWindow_MouseDown;
        }

        private void MainWindow_SourceInitialized(object? sender, EventArgs e)
        {
            var helper = new WindowInteropHelper(this);
            int exStyle = GetWindowLong(helper.Handle, GWL_EXSTYLE);
            SetWindowLong(helper.Handle, GWL_EXSTYLE, exStyle | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitNotifyIcon();
            ApplySettings();

            if (_settings.WindowLeft < 0 || _settings.WindowTop < 0)
            {
                SnapToClock();
            }
            else
            {
                EnsureWithinScreenBounds();
            }

            _networkMonitor.SpeedUpdated += NetworkMonitor_SpeedUpdated;
            _networkMonitor.Start();
            _timer.Start();
        }

        private void InitNotifyIcon()
        {
            _notifyIcon = new Forms.NotifyIcon();

            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
                if (File.Exists(iconPath))
                {
                    _notifyIcon.Icon = new Icon(iconPath);
                }
                else
                {
                    _notifyIcon.Icon = SystemIcons.Application;
                }
            }
            catch
            {
                _notifyIcon.Icon = SystemIcons.Application;
            }

            _notifyIcon.Text = "NetSpeed Widget - Ağ Hızı";
            _notifyIcon.Visible = true;

            _notifyIcon.MouseClick += (s, e) =>
            {
                if (e.Button == Forms.MouseButtons.Left)
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (Visibility == Visibility.Visible)
                        {
                            Activate();
                        }
                        else
                        {
                            Visibility = Visibility.Visible;
                        }
                    });
                }
                else if (e.Button == Forms.MouseButtons.Right)
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (WidgetContextMenu != null)
                        {
                            WidgetContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
                            WidgetContextMenu.IsOpen = true;
                        }
                    });
                }
            };

            _notifyIcon.DoubleClick += (s, e) =>
            {
                Dispatcher.Invoke(ShowStatsWindow);
            };
        }

        private void ApplySettings()
        {
            // Position lock
            MenuLockPosition.IsChecked = _settings.IsLocked;
            RootBorder.Cursor = _settings.IsLocked ? WpfCursors.Arrow : WpfCursors.SizeAll;

            // Startup registration
            MenuStartup.IsChecked = AppSettings.IsStartupRegistered();

            // Interval selection checks
            MenuIntervalFast.IsChecked = Math.Abs(_settings.RefreshIntervalSeconds - 0.5) < 0.05;
            MenuIntervalNormal.IsChecked = Math.Abs(_settings.RefreshIntervalSeconds - 1.0) < 0.05;
            MenuIntervalSlow.IsChecked = Math.Abs(_settings.RefreshIntervalSeconds - 2.0) < 0.05;

            // Unit selection checks
            MenuUnitAuto.IsChecked = _settings.SpeedUnit == "auto";
            MenuUnitKb.IsChecked = _settings.SpeedUnit == "kb";
            MenuUnitMb.IsChecked = _settings.SpeedUnit == "mb";

            // Compact mode
            MenuCompactMode.IsChecked = _settings.CompactMode;
            SetCompactMode(_settings.CompactMode);

            // Theme
            ApplyTheme(_settings.Theme);
        }

        private void ApplyTheme(string theme)
        {
            _settings.Theme = theme;
            MenuThemeDark.IsChecked = theme == "dark";
            MenuThemeTransparent.IsChecked = theme == "transparent";
            MenuThemeLight.IsChecked = theme == "light";

            var converter = new BrushConverter();

            switch (theme)
            {
                case "transparent":
                    RootBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(1, 0, 0, 0));
                    RootBorder.BorderBrush = System.Windows.Media.Brushes.Transparent;
                    RootBorder.BorderThickness = new Thickness(0);
                    TxtDownload.Foreground = (System.Windows.Media.Brush)converter.ConvertFromString("#FFFFFF")!;
                    TxtUpload.Foreground = (System.Windows.Media.Brush)converter.ConvertFromString("#FFFFFF")!;
                    TxtDownloadCompact.Foreground = (System.Windows.Media.Brush)converter.ConvertFromString("#FFFFFF")!;
                    TxtUploadCompact.Foreground = (System.Windows.Media.Brush)converter.ConvertFromString("#FFFFFF")!;
                    break;

                case "light":
                    RootBorder.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#EEF1F5F9")!;
                    RootBorder.BorderBrush = (System.Windows.Media.Brush)converter.ConvertFromString("#30000000")!;
                    RootBorder.BorderThickness = new Thickness(1);
                    TxtDownload.Foreground = (System.Windows.Media.Brush)converter.ConvertFromString("#0F172A")!;
                    TxtUpload.Foreground = (System.Windows.Media.Brush)converter.ConvertFromString("#0F172A")!;
                    TxtDownloadCompact.Foreground = (System.Windows.Media.Brush)converter.ConvertFromString("#0F172A")!;
                    TxtUploadCompact.Foreground = (System.Windows.Media.Brush)converter.ConvertFromString("#0F172A")!;
                    break;

                default: // "dark"
                    RootBorder.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#D6181920")!;
                    RootBorder.BorderBrush = (System.Windows.Media.Brush)converter.ConvertFromString("#25FFFFFF")!;
                    RootBorder.BorderThickness = new Thickness(1);
                    TxtDownload.Foreground = (System.Windows.Media.Brush)converter.ConvertFromString("#F8FAFC")!;
                    TxtUpload.Foreground = (System.Windows.Media.Brush)converter.ConvertFromString("#F8FAFC")!;
                    TxtDownloadCompact.Foreground = (System.Windows.Media.Brush)converter.ConvertFromString("#F8FAFC")!;
                    TxtUploadCompact.Foreground = (System.Windows.Media.Brush)converter.ConvertFromString("#F8FAFC")!;
                    break;
            }

            _settings.Save();
        }

        private void SetCompactMode(bool compact)
        {
            _settings.CompactMode = compact;
            if (compact)
            {
                GridTwoRows.Visibility = Visibility.Collapsed;
                StackOneRow.Visibility = Visibility.Visible;
                Height = 26;
                Width = 116;
            }
            else
            {
                StackOneRow.Visibility = Visibility.Collapsed;
                GridTwoRows.Visibility = Visibility.Visible;
                Height = 36;
                Width = 112;
            }
            _settings.Save();
        }

        public void SnapToClock()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double workHeight = SystemParameters.WorkArea.Height;
            double taskbarHeight = screenHeight - workHeight;

            double targetWidth = Width > 0 ? Width : 112;
            double targetHeight = Height > 0 ? Height : 36;

            // In Windows 11, taskbar notification icons & clock occupy ~220-280px on the right
            Left = screenWidth - 275 - targetWidth;

            if (taskbarHeight > 10)
            {
                Top = screenHeight - taskbarHeight + ((taskbarHeight - targetHeight) / 2.0);
            }
            else
            {
                Top = screenHeight - targetHeight - 6;
            }

            _settings.WindowLeft = Left;
            _settings.WindowTop = Top;
            _settings.Save();
        }

        private void EnsureWithinScreenBounds()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            Left = Math.Max(0, Math.Min(_settings.WindowLeft, screenWidth - Width));
            Top = Math.Max(0, Math.Min(_settings.WindowTop, screenHeight - Height));
        }

        private void NetworkMonitor_SpeedUpdated(object? sender, NetworkSpeedEventArgs e)
        {
            _lastArgs = e;

            Dispatcher.InvokeAsync(() =>
            {
                TxtDownload.Text = e.DownloadFormatted;
                TxtUpload.Text = e.UploadFormatted;

                TxtDownloadCompact.Text = e.DownloadFormatted.Replace("/s", "").Trim();
                TxtUploadCompact.Text = e.UploadFormatted.Replace("/s", "").Trim();

                if (_notifyIcon != null)
                {
                    string summary = $"NetSpeed\n↓ {e.DownloadFormatted}\n↑ {e.UploadFormatted}";
                    if (summary.Length > 63) summary = $"↓ {e.DownloadFormatted} | ↑ {e.UploadFormatted}";
                    _notifyIcon.Text = summary;
                }

                _statsWindow?.UpdateStats(e);
            });
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (e.ClickCount == 2)
                {
                    ShowStatsWindow();
                }
                else if (!_settings.IsLocked)
                {
                    DragMove();
                }
            }
        }

        private void MainWindow_LocationChanged(object? sender, EventArgs e)
        {
            if (IsLoaded)
            {
                _settings.WindowLeft = Left;
                _settings.WindowTop = Top;
                _settings.Save();
            }
        }

        private void ShowStatsWindow()
        {
            if (_statsWindow == null || !_statsWindow.IsLoaded)
            {
                _statsWindow = new StatsWindow();
            }

            if (_lastArgs != null)
            {
                _statsWindow.UpdateStats(_lastArgs);
            }

            _statsWindow.Show();
            _statsWindow.Activate();
        }

        private void MenuLockPosition_Click(object sender, RoutedEventArgs e)
        {
            _settings.IsLocked = MenuLockPosition.IsChecked;
            RootBorder.Cursor = _settings.IsLocked ? WpfCursors.Arrow : WpfCursors.SizeAll;
            _settings.Save();
        }

        private void MenuSnapToClock_Click(object sender, RoutedEventArgs e)
        {
            SnapToClock();
        }

        private void MenuTheme_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is string themeTag)
            {
                ApplyTheme(themeTag);
            }
        }

        private void MenuCompactMode_Click(object sender, RoutedEventArgs e)
        {
            SetCompactMode(MenuCompactMode.IsChecked);
        }

        private void MenuUnit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is string unitTag)
            {
                _settings.SpeedUnit = unitTag;
                _networkMonitor.SpeedUnit = unitTag;
                ApplySettings();
                _settings.Save();
            }
        }

        private void MenuInterval_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is string intervalStr && double.TryParse(intervalStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double interval))
            {
                _settings.RefreshIntervalSeconds = interval;
                _timer.Interval = TimeSpan.FromSeconds(interval);
                ApplySettings();
                _settings.Save();
            }
        }

        private void MenuStats_Click(object sender, RoutedEventArgs e)
        {
            ShowStatsWindow();
        }

        private void MenuStartup_Click(object sender, RoutedEventArgs e)
        {
            bool enable = MenuStartup.IsChecked;
            _settings.StartWithWindows = enable;
            AppSettings.SetStartupRegistration(enable);
            _settings.Save();
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }
            _timer.Stop();
            System.Windows.Application.Current.Shutdown();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }
            base.OnClosed(e);
        }
    }
}
