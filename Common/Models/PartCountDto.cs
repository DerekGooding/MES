namespace MES.Common.Models;

public class PartCountDto
{
    public int TotalParts { get; set; }
    public int GoodParts { get; set; }
    public int BadParts { get; set; }
    public int InProcessParts { get; set; }
}
