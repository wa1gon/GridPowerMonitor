namespace GridMonitorMaui.Components.Pages;
using System.Net;
using System.Net.Sockets;
using System.Text;

class DeviceInfo
{
    public DateTime LastUpdate { get; set; }
    public TimeSpan Uptime { get; set; }
    public DateTime? DarkStart { get; set; }
    public DateTime? DarkEnd { get; set; }
}

public partial class Home
{
    DateTime lastHeartBeat = DateTime.MinValue;
    private Dictionary<string, DeviceInfo> devices = new Dictionary<string, DeviceInfo>();
    private List<string> darkPeriodsLog = new List<string>();

    public Home()
    {
        Task.Run(UdpListener);
    }

    private void UdpListener()
    {
        using (var udpClient = new UdpClient(9124))
        {
            var endPoint = new IPEndPoint(IPAddress.Any, 9124);
            Console.WriteLine("Listening for UDP broadcasts on port 9124...");

            while (true)
            {
                var receivedBytes = udpClient.Receive(ref endPoint);
                var receivedMessage = Encoding.UTF8.GetString(receivedBytes);
                var parts = receivedMessage.Split('|');
                if (parts.Length == 2)
                {
                    var deviceName = parts[0];
                    var uptime = TimeSpan.Parse(parts[1]);

                    InvokeAsync(() =>
                    {
                        if (!devices.ContainsKey(deviceName))
                        {
                            devices[deviceName] = new DeviceInfo { LastUpdate = DateTime.Now, Uptime = uptime };
                        }
                        else
                        {
                            var device = devices[deviceName];
                            var timeSinceLastUpdate = DateTime.Now - device.LastUpdate;

                            if (timeSinceLastUpdate > TimeSpan.FromSeconds(3))
                            {
                                if (device.DarkStart == null)
                                {
                                    device.DarkStart = device.LastUpdate;
                                }
                                device.DarkEnd = DateTime.Now;
                                darkPeriodsLog.Add($"{deviceName} was dark from {device.DarkStart} to {device.DarkEnd}");
                                device.DarkStart = null;
                                device.DarkEnd = null;
                            }

                            device.LastUpdate = DateTime.Now;
                            device.Uptime = uptime;
                        }

                        StateHasChanged();
                    });
                }
            }
        }
    }

    private string GetDeviceStatus(DeviceInfo deviceInfo)
    {
        var timeSinceLastUpdate = DateTime.Now - deviceInfo.LastUpdate;
        if (timeSinceLastUpdate < TimeSpan.FromMinutes(3))
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