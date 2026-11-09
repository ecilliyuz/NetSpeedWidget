using System;
using System.Threading;
using System.Windows;

namespace NetSpeedWidget
{
    public partial class App : System.Windows.Application
    {
        private static Mutex? _mutex;
        private const string AppMutexName = "NetSpeedWidget_SingleInstance_Mutex_2026";

        protected override void OnStartup(StartupEventArgs e)
        {
            _mutex = new Mutex(true, AppMutexName, out bool createdNew);

            if (!createdNew)
            {
                // Already running
                Shutdown();
                return;
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_mutex != null)
            {
                try
                {
                    _mutex.ReleaseMutex();
                    _mutex.Dispose();
                }
                catch { }
            }
            base.OnExit(e);
        }
    }
}
