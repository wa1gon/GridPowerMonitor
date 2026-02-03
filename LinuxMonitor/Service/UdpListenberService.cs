// csharp
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Avalonia.Threading;
using LinuxMonitor.ViewModels;

namespace LinuxMonitor.Services
{
    public class UdpListenerService : BackgroundService
    {
        private readonly MainWindowViewModel _vm;
        private readonly int _port = 9123;

        public UdpListenerService(MainWindowViewModel vm)
        {
            _vm = vm;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var udp = new UdpClient(_port);
            var remoteEp = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var result = await udp.ReceiveAsync().WithCancellation(stoppingToken);
                    var text = Encoding.UTF8.GetString(result.Buffer);

                    // Marshal update to Avalonia UI thread
                    Dispatcher.UIThread.Post(() =>
                    {
                        _vm.LastMessage = text;
                        _vm.LastRemote = result.RemoteEndPoint?.ToString() ?? "";
                        _vm.LastReceivedAt = DateTime.Now;
                    });
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(() => _vm.LastMessage = $"Listener error: {ex.Message}");
            }
        }
    }

    // small helper to support cancellation with ReceiveAsync
    internal static class UdpExtensions
    {
        public static async Task<UdpReceiveResult> WithCancellation(this Task<UdpReceiveResult> task, CancellationToken cancellationToken)
        {
            using var tcs = new CancellationTokenTaskSource<UdpReceiveResult>(cancellationToken);
            var completed = await Task.WhenAny(task, tcs.Task).ConfigureAwait(false);
            if (completed == tcs.Task) throw new OperationCanceledException(cancellationToken);
            return await task.ConfigureAwait(false);
        }

        private sealed class CancellationTokenTaskSource<T> : IDisposable
        {
            private readonly TaskCompletionSource<T> _tcs = new();
            private readonly CancellationTokenRegistration _reg;

            public CancellationTokenTaskSource(CancellationToken ct)
            {
                _reg = ct.Register(() => _tcs.TrySetResult(default!));
            }

            public Task<T> Task => _tcs.Task;

            public void Dispose() => _reg.Dispose();
        }
    }
}
