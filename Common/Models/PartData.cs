

namespace MES.Common;

public class PartData
{
    public int PartId { get; set; }
    public string SerialNumber { get; set; }
    public string LastStationComplete { get; set; }
    public string Status { get; set; }
    public DateTime Timestamp { get; set; }
    public float? VisionMeasurement { get; set; }
    public float? PasteDispenseWeight { get; set; }
    public string? CircuitBoardSerialNumber { get; set; }
    public float? Screw1Torque { get; set; }
    public float? Screw2Torque { get; set; }
    public float? Screw3Torque { get; set; }
    public float? Screw4Torque { get; set; }
    public float? VoltageOutput { get; set; }
    public float? FinalWeight { get; set; }

}
