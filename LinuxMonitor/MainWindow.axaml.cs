// csharp
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LinuxMonitor.ViewModels;

namespace LinuxMonitor;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}