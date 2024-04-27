using EPCSystemAPI;
using EPCSystemAPI.models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

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

        // Begin transaction to ensure data consistency across multiple writes
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
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

            _context.Electricity_Production.Add(electricityProduction);
            await _context.SaveChangesAsync();

            var certificate = new Certificate
            {
                UserId = device.UserId,
                ElectricityProductionId = electricityProduction.Id,
                CreatedAt = DateTime.Now,
                Volume = model.AmountWh
            };
            _context.Certificates.Add(certificate);

            var produceEvent = new Event
            {
                Event_Type = "PRODUCTION",
                Reference_Id = electricityProduction.Id,
                User_Id = device.UserId,
                Timestamp = DateTime.Now
            };
            _context.Events.Add(produceEvent);
            await _context.SaveChangesAsync();

            var productionEvent = new ProduceEvent
            {
                Event_Id = produceEvent.Id,
                ProductionTime = model.ProductionTime,
                DeviceId = model.DeviceId,
                ElectricityProductionId = electricityProduction.Id
            };
            _context.ProduceEvents.Add(productionEvent);

            // Save changes to get the auto-generated ID of the ProduceEvent
            await _context.SaveChangesAsync();

            // Update the Reference_Id of the Event to be the ID of the ProduceEvent
            produceEvent.Reference_Id = productionEvent.Id;

            // Save changes again to update the Reference_Id in the Event entity
            await _context.SaveChangesAsync();

            // Commit transaction if all operations succeed
            await transaction.CommitAsync();

            return Ok(new { ElectricityProduction = electricityProduction, Certificate = certificate });
        }
        catch (Exception ex)
        {
            // Rollback transaction on error
            await transaction.RollbackAsync();
            // Log the error (consider using a logging framework such as Serilog, NLog, etc.)
            return StatusCode(500, $"An error occurred while processing your request: {ex.Message}");
        }
    }


}
