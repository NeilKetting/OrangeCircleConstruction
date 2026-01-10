using System;
using Avalonia;
using Serilog;

namespace OCC.Client.Desktop
{
    internal sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            // Configure Serilog
            var logPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                "OCC", "logs", "log-.txt");

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 2)
                .CreateLogger();

            // Global Exception Handlers
            AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
            {
                Log.Fatal(error.ExceptionObject as Exception, "AppDomain Unhandled Exception");
                Log.CloseAndFlush();
            };

            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (sender, error) =>
            {
                Log.Error(error.Exception, "TaskScheduler Unobserved Task Exception");
                error.SetObserved();
            };

            try
            {
                Velopack.VelopackApp.Build().Run();

                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application StartWithClassicDesktopLifetime Crash");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
