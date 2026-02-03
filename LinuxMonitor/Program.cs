// csharp
using System.Threading.Tasks;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LinuxMonitor
{
    internal static class Program
    {
        public static IHost AppHost { get; private set; } = null!;

        public static async Task Main(string[] args)
        {
            AppHost = Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Services.UdpListenerService>();
                    services.AddSingleton<ViewModels.MainWindowViewModel>();
                    services.AddSingleton<MainWindow>();
                })
                .Build();

            await AppHost.StartAsync();

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);

            await AppHost.StopAsync();
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
    }
}