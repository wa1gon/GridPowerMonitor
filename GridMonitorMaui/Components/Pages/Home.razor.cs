namespace GridMonitorMaui.Components.Pages;
using System.Net;
using System.Net.Sockets;
using System.Text;

class DeviceInfo
{
    public DateTime LastUpdate { get; set; }
}

public partial class Home
{
    DateTime lastHeartBeat = DateTime.MinValue;
    private Dictionary<string, DeviceInfo> devices = new Dictionary<string, DeviceInfo>();

    public Home()
    {
        Task.Run(UdpListener);
    }

    private void UdpListener()
    {
        using (var udpClient = new UdpClient(9123))
        {
            var endPoint = new IPEndPoint(IPAddress.Any, 9123);
            Console.WriteLine("Listening for UDP broadcasts on port 9123...");

            while (true)
            {
                var receivedBytes = udpClient.Receive(ref endPoint);
                var receivedMessage = Encoding.UTF8.GetString(receivedBytes);
                var deviceName = receivedMessage; // Assuming the message contains the device name

                InvokeAsync(() =>
                {
                    devices[deviceName] = new DeviceInfo { LastUpdate = DateTime.Now };
                    StateHasChanged();
                });
            }
        }
    }

    private string GetDeviceStatus(DeviceInfo deviceInfo)
    {
        var timeSinceLastUpdate = DateTime.Now - deviceInfo.LastUpdate;
        if (timeSinceLastUpdate < TimeSpan.FromMinutes(2))
        {
            return "Online";
        }
        else if (timeSinceLastUpdate < TimeSpan.FromMinutes(10))
        {
            return "Idle";
        }
        else
        {
            return "Offline";
        }
    }

    private string GetStatusIcon(string status)
    {
        return status switch
        {
            "Online" => "&#x1F7E2;", // Green circle
            "Idle" => "&#x1F7E1;", // Yellow circle
            "Offline" => "&#x1F534;", // Red circle
            _ => "&#x2B1C;" // White large square
        };
    }
}