using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;

namespace LinuxMonitor
{
    public partial class App : Application
    {

            public override void Initialize() => AvaloniaXamlLoader.Load(this);

            public override void OnFrameworkInitializationCompleted()
            {
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var mainWindow = Program.AppHost.Services.GetRequiredService<MainWindow>();
                    desktop.MainWindow = mainWindow;
                }

                base.OnFrameworkInitializationCompleted();
            }
        
    }
}