using Microsoft.EntityFrameworkCore;
using MES.Common;

namespace MES.Data;

public class PartDataRepository : IDisposable
{
    private DataContext _context;

    public PartDataRepository()
    {
        _context = new DataContext();
    }

    public async Task AddPartDataAsync(PartData partData)
    {
        await _context.Parts.AddAsync(partData);
        await _context.SaveChangesAsync();
    }


    public async Task<PartData?> GetPartDataByIdAsync(int partId)
    {
        return await _context.Parts.FindAsync(partId);
    }

    public async Task<PartData?> GetPartDataBySerialNumberAsync(string serialNumber)
    {
        //return await _context.Parts.FirstOrDefaultAsync(p => p.SerialNumber == serialNumber);
        return await _context.Parts.FirstOrDefaultAsync(p => p.SerialNumber == serialNumber);
    }

    public async Task UpdatePartDataAsync(PartData partData)
    {
        PartData existingPart = await _context.Parts.FirstOrDefaultAsync(p => p.SerialNumber == partData.SerialNumber);
        if (existingPart == null)
        {
            await _context.Parts.AddAsync(partData);

        }
        else
        {
            existingPart.LastStationComplete = partData.LastStationComplete;
            existingPart.Status = partData.Status;
            existingPart.Timestamp = partData.Timestamp;
            if(partData.VisionMeasurement != null)
                existingPart.VisionMeasurement = partData.VisionMeasurement;
            if (partData.PasteDispenseWeight != null)
                existingPart.PasteDispenseWeight = partData.PasteDispenseWeight;
            if (partData.CircuitBoardSerialNumber != null)
                existingPart.CircuitBoardSerialNumber = partData.CircuitBoardSerialNumber;
            if (partData.Screw1Torque != null)
                existingPart.Screw1Torque = partData.Screw1Torque;
            if (partData.Screw2Torque != null)
                existingPart.Screw2Torque = partData.Screw2Torque;
            if (partData.Screw3Torque != null)
                existingPart.Screw3Torque = partData.Screw3Torque;
            if (partData.Screw4Torque != null)
                existingPart.Screw4Torque = partData.Screw4Torque;
            if (partData.VoltageOutput != null)
                existingPart.VoltageOutput = partData.VoltageOutput;
            if (partData.FinalWeight != null)
                existingPart.FinalWeight = partData.FinalWeight;

        }

       await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
