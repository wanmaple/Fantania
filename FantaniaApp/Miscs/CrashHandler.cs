using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using FantaniaLib;
using Fantania.ViewModels;

namespace Fantania
{
    public static class CrashHandler
    {
        static readonly string LogFolder = AppContext.BaseDirectory;

        public static void Init()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Exception ex = e.ExceptionObject as Exception ?? new Exception("Non-Exception object thrown");
                LogException(ex, "AppDomain.CurrentDomain.UnhandledException");
            };
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogException(e.Exception, "TaskScheduler.UnobservedTaskException");
                e.SetObserved();
            };
        }

        public static void LogException(Exception ex, string? source = null)
        {
            try
            {
                string filename = Path.Combine(LogFolder, $"crash_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                var sb = new StringBuilder();
                sb.AppendLine("=== Fantania Crash Report ===");
                sb.AppendLine($"Timestamp: {DateTime.Now:O}");
                if (!string.IsNullOrEmpty(source)) sb.AppendLine($"Source: {source}");
                sb.AppendLine("--- Exception ---");
                sb.AppendLine(ex.ToString());
                sb.AppendLine();
                File.AppendAllText(filename, sb.ToString());
                // Also append to a rolling crash.log for quick inspection
                var rolling = Path.Combine(LogFolder, "crash.log");
                File.AppendAllText(rolling, sb.ToString());
                // Try to post into in-app LogModule if available (safe, non-blocking)
                try
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        try
                        {
                            var top = AvaloniaHelper.GetTopWindow();
                            if (top?.DataContext is MainWindowViewModel vm && vm.Workspace != null)
                            {
                                vm.Workspace.LogError($"Unhandled exception: {ex}");
                            }
                        }
                        catch { }
                    });
                }
                catch { }
            }
            catch
            {
                // swallow any logging errors to avoid recursive crashes
            }
        }
    }
}
