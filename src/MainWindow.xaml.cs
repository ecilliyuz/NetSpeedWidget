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
using Microsoft.Win32;
using Forms = System.Windows.Forms;
using WpfCursors = System.Windows.Input.Cursors;

namespace NetSpeedWidget
{
    public partial class MainWindow : Window
    {
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private readonly AppSettings _settings;
        private readonly NetworkMonitor _networkMonitor;
        private readonly DispatcherTimer _timer;
        private Forms.NotifyIcon? _notifyIcon;
        private StatsWindow? _statsWindow;
        private NetworkSpeedEventArgs? _lastArgs;
        private bool _isFullyInitialized = false;

        public MainWindow()
        {
            InitializeComponent();

            _settings = AppSettings.Load();

            // Set startup location and coordinates immediately in constructor
            WindowStartupLocation = WindowStartupLocation.Manual;
            if (_settings.WindowLeft >= 0 && _settings.WindowTop >= 0)
            {
                Left = _settings.WindowLeft;
                Top = _settings.WindowTop;
            }
            else
            {
                CalculateClockPosition(out double defaultLeft, out double defaultTop);
                Left = defaultLeft;
                Top = defaultTop;
                _settings.WindowLeft = defaultLeft;
                _settings.WindowTop = defaultTop;
                _settings.Save();
            }

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
            Deactivated += (s, e) => EnsureTopmost();
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

            // Check if saved coordinates are on a connected monitor, otherwise snap
            EnsureWithinScreenBounds();

            _networkMonitor.SpeedUpdated += NetworkMonitor_SpeedUpdated;
            _networkMonitor.Start();
            _timer.Start();

            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
            EnsureTopmost();

            // Initialization completed - enable position saving
            _isFullyInitialized = true;
        }

        private void SystemEvents_DisplaySettingsChanged(object? sender, EventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                EnsureWithinScreenBounds();
                EnsureTopmost();
            });
        }

        public void EnsureTopmost()
        {
            try
            {
                var helper = new WindowInteropHelper(this);
                if (helper.Handle != IntPtr.Zero)
                {
                    SetWindowPos(helper.Handle, HWND_TOPMOST, 0, 0, 0, 0,
                        SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
                }
            }
            catch { }
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
                        if (WindowState == WindowState.Minimized)
                        {
                            WindowState = WindowState.Normal;
                        }

                        Visibility = Visibility.Visible;
                        EnsureWithinScreenBounds();
                        EnsureTopmost();
                        Activate();
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
            SetCompactModeInternal(_settings.CompactMode);

            // Theme
            ApplyThemeInternal(_settings.Theme);
        }

        private void ApplyThemeInternal(string theme)
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
        }

        private void SetCompactModeInternal(bool compact)
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
        }

        public void CalculateClockPosition(out double clockLeft, out double clockTop)
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double workHeight = SystemParameters.WorkArea.Height;
            double taskbarHeight = screenHeight - workHeight;

            double targetWidth = Width > 0 ? Width : 112;
            double targetHeight = Height > 0 ? Height : 36;

            // In Windows 11, taskbar notification icons & clock occupy ~220-280px on the right
            clockLeft = screenWidth - 275 - targetWidth;

            if (taskbarHeight > 10)
            {
                clockTop = screenHeight - taskbarHeight + ((taskbarHeight - targetHeight) / 2.0);
            }
            else
            {
                clockTop = screenHeight - targetHeight - 6;
            }
        }

        public void SnapToClock()
        {
            CalculateClockPosition(out double clockLeft, out double clockTop);
            Left = clockLeft;
            Top = clockTop;

            SavePosition();
            EnsureTopmost();
        }

        private void EnsureWithinScreenBounds()
        {
            double virtualLeft = SystemParameters.VirtualScreenLeft;
            double virtualTop = SystemParameters.VirtualScreenTop;
            double virtualWidth = SystemParameters.VirtualScreenWidth;
            double virtualHeight = SystemParameters.VirtualScreenHeight;

            double targetLeft = Left;
            double targetTop = Top;

            if (_settings.WindowLeft >= 0 && _settings.WindowTop >= 0)
            {
                targetLeft = _settings.WindowLeft;
                targetTop = _settings.WindowTop;
            }

            // Check if coordinates are completely outside any connected screen
            bool isOffScreen = double.IsNaN(targetLeft) || double.IsNaN(targetTop) ||
                              targetLeft < virtualLeft - 10 || targetLeft > (virtualLeft + virtualWidth - 10) ||
                              targetTop < virtualTop - 10 || targetTop > (virtualTop + virtualHeight - 10);

            if (isOffScreen)
            {
                SnapToClock();
                return;
            }

            Left = targetLeft;
            Top = targetTop;
        }

        private void SavePosition()
        {
            if (!_isFullyInitialized || WindowState != WindowState.Normal)
                return;

            if (!double.IsNaN(Left) && !double.IsNaN(Top))
            {
                _settings.WindowLeft = Left;
                _settings.WindowTop = Top;
                _settings.Save();
            }
        }

        private void NetworkMonitor_SpeedUpdated(object? sender, NetworkSpeedEventArgs e)
        {
            _lastArgs = e;

            Dispatcher.InvokeAsync(() =>
            {
                // Fullscreen check: hide while video is fullscreen, restore as soon as exited!
                bool isFullScreen = FullScreenHelper.IsForegroundWindowFullScreen();

                if (isFullScreen)
                {
                    if (Visibility != Visibility.Collapsed)
                    {
                        Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    if (Visibility != Visibility.Visible)
                    {
                        Visibility = Visibility.Visible;
                    }

                    if (WindowState == WindowState.Minimized)
                    {
                        WindowState = WindowState.Normal;
                    }

                    // Keep widget strictly on top of Taskbar
                    EnsureTopmost();
                }

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
                    SavePosition();
                }
            }
        }

        private void MainWindow_LocationChanged(object? sender, EventArgs e)
        {
            if (_isFullyInitialized && WindowState == WindowState.Normal)
            {
                SavePosition();
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
                ApplyThemeInternal(themeTag);
                _settings.Save();
            }
        }

        private void MenuCompactMode_Click(object sender, RoutedEventArgs e)
        {
            SetCompactModeInternal(MenuCompactMode.IsChecked);
            _settings.Save();
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
            SavePosition();
            SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
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
            SavePosition();
            SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }
            base.OnClosed(e);
        }
    }
}
