using System;
using System.IO;
using System.Windows;

namespace AcVelocity
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log");

            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                try
                {
                    File.WriteAllText(logPath, args.ExceptionObject.ToString());
                }
                catch { }
            };

            DispatcherUnhandledException += (s, args) =>
            {
                try
                {
                    File.WriteAllText(logPath, args.Exception.ToString());
                }
                catch { }
                args.Handled = true;
            };

            base.OnStartup(e);
        }
    }
}
