

namespace MES.Common;

public class StationOptions
{
    public string StationName { get; set; }
    public string IpAddress { get; set; }
    public string Port { get; set; }
    public Dictionary<string, string> Results { get; set; }
}
