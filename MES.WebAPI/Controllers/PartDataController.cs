
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MES.Common;
using MES.Common.Config.Enums;
using MES.Data.Data;
using MES.Common.Models;

namespace MES.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PartDataController(DataContext context, List<StationOptions> stationConfig) : ControllerBase
{
    private readonly DataContext _context = context;
    private readonly List<StationOptions> _stationConfig = stationConfig;


    // GET: api/PartData
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PartData>>> GetParts() => await _context.Parts.ToListAsync();

    // GET: api/PartData/5
    [HttpGet("{id}")]
    public async Task<ActionResult<PartData>> GetPartData(int id)
    {
        var partData = await _context.Parts.FindAsync(id);

        return partData == null ? (ActionResult<PartData>)NotFound() : (ActionResult<PartData>)partData;
    }

    [HttpGet("PartCounts")]
    public async Task<ActionResult<PartCountDto>> GetPartCounts()
    {
        var lastStationName = _stationConfig[^1].StationName;

        var goodParts = await _context.Parts
            .Where(p => p.Status == nameof(PLCOperationsEnum.Good) && p.LastStationComplete == lastStationName)
            .CountAsync();

        var badParts = await _context.Parts
            .Where(p => p.Status == nameof(PLCOperationsEnum.Bad))
            .CountAsync();

        var inProcessParts = await _context.Parts
            .Where(p => p.Status == nameof(PLCOperationsEnum.Good) && p.LastStationComplete != lastStationName)
            .CountAsync();

        var totalParts = goodParts + badParts;

        var partCount = new PartCountDto
        {
            TotalParts = totalParts,
            GoodParts = goodParts,
            BadParts = badParts,
            InProcessParts = inProcessParts
        };

        return partCount;
    }

    [HttpGet("GetBySerial/{serialNumber}")]
    public async Task<ActionResult<PartDataDto>> GetPartBySerialNumber(string serialNumber)
    {
        var partData = await _context.Parts
            .Where(p => p.SerialNumber == serialNumber)
            .OrderByDescending(p => p.Timestamp)
            .FirstOrDefaultAsync();

        if (partData == null)
        {
            return NotFound();
        }
        var partDataDto = new PartDataDto
        {
            SerialNumber = partData.SerialNumber,
            LastStationComplete = partData.LastStationComplete,
            Status = partData.Status,
            Timestamp = partData.Timestamp,
            VisionMeasurement = partData.VisionMeasurement,
            PasteDispenseWeight = partData.PasteDispenseWeight,
            CircuitBoardSerialNumber = partData.CircuitBoardSerialNumber,
            Screw1Torque = partData.Screw1Torque,
            Screw2Torque = partData.Screw2Torque,
            Screw3Torque = partData.Screw3Torque,
            Screw4Torque = partData.Screw4Torque,
            VoltageOutput = partData.VoltageOutput,
            FinalWeight = partData.FinalWeight
        };
        return partDataDto;
    }

    // PUT: api/PartData/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutPartData(int id, PartData partData)
    {
        if (id != partData.PartId)
        {
            return BadRequest();
        }

        _context.Entry(partData).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException) when (!PartDataExists(id))
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPut("UpdateBySerial/{serialNumber}")]
    public async Task<IActionResult> UpdatePartBySerialNumber(string serialNumber, PartDataDto updatedPartData)
    {
        var existingPartData = await _context.Parts
            .Where(p => p.SerialNumber == serialNumber)
            .OrderByDescending(p => p.Timestamp)
            .FirstOrDefaultAsync();
        if (existingPartData == null)
        {
            return NotFound();
        }
        // Update fields
        existingPartData.LastStationComplete = updatedPartData.LastStationComplete;
        existingPartData.Status = updatedPartData.Status;
        existingPartData.Timestamp = updatedPartData.Timestamp;
        existingPartData.VisionMeasurement = updatedPartData.VisionMeasurement;
        existingPartData.PasteDispenseWeight = updatedPartData.PasteDispenseWeight;
        existingPartData.CircuitBoardSerialNumber = updatedPartData.CircuitBoardSerialNumber;
        existingPartData.Screw1Torque = updatedPartData.Screw1Torque;
        existingPartData.Screw2Torque = updatedPartData.Screw2Torque;
        existingPartData.Screw3Torque = updatedPartData.Screw3Torque;
        existingPartData.Screw4Torque = updatedPartData.Screw4Torque;
        existingPartData.VoltageOutput = updatedPartData.VoltageOutput;
        existingPartData.FinalWeight = updatedPartData.FinalWeight;
        _context.Entry(existingPartData).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException) when (!PartDataExists(existingPartData.PartId))
        {
            return NotFound();
        }
        return NoContent();
    }

    // POST: api/PartData
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<PartData>> PostPartData(PartData partData)
    {
        _context.Parts.Add(partData);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetPartData", new { id = partData.PartId }, partData);
    }

    // DELETE: api/PartData/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePartData(int id)
    {
        var partData = await _context.Parts.FindAsync(id);
        if (partData == null)
        {
            return NotFound();
        }

        _context.Parts.Remove(partData);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool PartDataExists(int id) => _context.Parts.Any(e => e.PartId == id);
}
