using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MES.Common;
using MES.Data;

namespace MES.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PartDataController : ControllerBase
{
    private readonly DataContext _context;

    public PartDataController(DataContext context)
    {
        _context = context;
    }

    // GET: api/PartData
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PartData>>> GetParts()
    {
        return await _context.Parts.ToListAsync();
    }

    // GET: api/PartData/5
    [HttpGet("{id}")]
    public async Task<ActionResult<PartData>> GetPartData(int id)
    {
        var partData = await _context.Parts.FindAsync(id);

        if (partData == null)
        {
            return NotFound();
        }

        return partData;
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
        catch (DbUpdateConcurrencyException)
        {
            if (!PartDataExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
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

    private bool PartDataExists(int id)
    {
        return _context.Parts.Any(e => e.PartId == id);
    }
}
