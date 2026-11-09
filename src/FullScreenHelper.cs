using System;
using System.Runtime.InteropServices;
using System.Text;

namespace NetSpeedWidget
{
    public static class FullScreenHelper
    {
        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern IntPtr GetShellWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public static bool IsForegroundWindowFullScreen()
        {
            try
            {
                IntPtr foregroundWnd = GetForegroundWindow();
                if (foregroundWnd == IntPtr.Zero)
                    return false;

                IntPtr desktopWnd = GetDesktopWindow();
                IntPtr shellWnd = GetShellWindow();

                if (foregroundWnd == desktopWnd || foregroundWnd == shellWnd)
                    return false;

                var sbClass = new StringBuilder(256);
                GetClassName(foregroundWnd, sbClass, 256);
                string className = sbClass.ToString();

                // Ignore Desktop and Taskbar
                if (className == "Progman" || className == "WorkerW" ||
                    className == "Shell_TrayWnd" || className == "Shell_SecondaryTrayWnd")
                {
                    return false;
                }

                if (GetWindowRect(foregroundWnd, out RECT rect))
                {
                    int screenWidth = GetSystemMetrics(SM_CXSCREEN);
                    int screenHeight = GetSystemMetrics(SM_CYSCREEN);

                    // Check if foreground window covers the screen
                    if (rect.Left <= 0 && rect.Top <= 0 &&
                        rect.Right >= screenWidth && rect.Bottom >= screenHeight)
                    {
                        return true;
                    }
                }
            }
            catch
            {
                // Safeguard against API exceptions
            }

            return false;
        }
    }
}
