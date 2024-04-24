using EPCSystemAPI.models;
using EPCSystemAPI;
using Microsoft.AspNetCore.Mvc;
using System;

[ApiController]
[Route("[controller]")]
public class ElectricityProductionController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ElectricityProductionController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateElectricityProduction([FromBody] ElectricityProductionDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Get the device associated with the provided DeviceId
        var device = await _context.Devices.FindAsync(model.DeviceId);
        if (device == null)
        {
            return NotFound("Device not found");
        }
        
        var electricityProduction = new ElectricityProduction
        {
            ProductionTime = model.ProductionTime,
            AmountWh = model.AmountWh,
            DeviceId = model.DeviceId
        };

        _context.ElectricityProductions.Add(electricityProduction);
        
        await _context.SaveChangesAsync();
        
        var certificate = new Certificate
        {
            UserId = device.UserId,
            ElectricityProductionId = electricityProduction.Id,
            CreatedAt = DateTime.Now,
            volume = model.AmountWh
        
        };
        _context.Certificates.Add(certificate);
                
        var produceEvent = new ProduceEvent
        {
            ProductionTime = model.ProductionTime,
            DeviceId = model.DeviceId,
            ElectricityProductionId = electricityProduction.Id            
        };
        _context.ProduceEvents.Add(produceEvent);
        
        await _context.SaveChangesAsync();
        
        var ledgerEntry = new ProduceLedger
        {
            Id = produceEvent.Id, // ProduceEvent ID is the Ledger ID
            EventType = "PRODUCE", 
            ElectricityProductionId = electricityProduction.Id,
            TransactionDate = DateTime.Now,
            Volume = model.AmountWh 
        };
        _context.produceLedger.Add(ledgerEntry);

        await _context.SaveChangesAsync();

        return Ok(electricityProduction);
    }
}
