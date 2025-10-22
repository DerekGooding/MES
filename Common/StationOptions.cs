namespace MES.Common;

public class StationOptions
{
    public string StationName { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Port { get; set; } = string.Empty;
    public Dictionary<string, string> Results { get; set; } = [];
}
