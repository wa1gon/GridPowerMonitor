// csharp
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LinuxMonitor.ViewModels
{
public class MainWindowViewModel : INotifyPropertyChanged
{
private string _lastMessage = "(no messages)";
private string _lastRemote = "";
private DateTime? _lastReceivedAt;

public event PropertyChangedEventHandler? PropertyChanged;

public string LastMessage
{
get => _lastMessage;
set { if (_lastMessage != value) { _lastMessage = value; OnPropertyChanged(); } }
}

public string LastRemote
{
get => _lastRemote;
set { if (_lastRemote != value) { _lastRemote = value; OnPropertyChanged(); } }
}

public DateTime? LastReceivedAt
{
get => _lastReceivedAt;
set { if (_lastReceivedAt != value) { _lastReceivedAt = value; OnPropertyChanged(); } }
}

private void OnPropertyChanged([CallerMemberName] string? name = null)
=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
}