using System.Net;
using System.Net.Sockets;
using System.Text;


class Program
{
    static DateTime lastHeartBeat = DateTime.MinValue;
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .UseWindowsService() // Add this line to run as a Windows Service
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddControllers(); // Add controllers
                });

                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers(); // Map controller routes
                    });
                });

                webBuilder.UseUrls("http://0.0.0.0:9000"); // Listen on all network interfaces
            })
            .Build();

        // Start the UDP listener in a new thread
        var udpListenerThread = new Thread(UdpListener);
        udpListenerThread.IsBackground = true;
        udpListenerThread.Start();

        Console.WriteLine("Web server is running at http://0.0.0.0:9000");
        Console.WriteLine("Press Ctrl+C to exit.");

        await host.RunAsync();
    }

    private static void UdpListener()
    {
        using (var udpClient = new UdpClient(9123))
        {
            var endPoint = new IPEndPoint(IPAddress.Any, 9123);
            Console.WriteLine("Listening for UDP broadcasts on port 9123...");

            while (true)
            {
                var receivedBytes = udpClient.Receive(ref endPoint);
                var receivedMessage = Encoding.UTF8.GetString(receivedBytes);
                lastHeartBeat = DateTime.Now;
                Console.WriteLine($"{DateTime.Now}: {endPoint}: {receivedMessage}");
            }
        }
    }
}
