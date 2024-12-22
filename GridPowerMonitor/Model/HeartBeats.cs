namespace GridPowerMonitor.Model;

public class HeartBeats
{
    public DateTime LastHeartBeat { get; set; }
    public string DeviceId { get; set; } = string.Empty;
}
