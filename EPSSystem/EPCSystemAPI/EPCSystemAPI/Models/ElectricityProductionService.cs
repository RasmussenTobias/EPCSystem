using EPCSystemAPI;
using EPCSystemAPI.models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

public class ElectricityProductionService
{
    private readonly ApplicationDbContext _context;

    public ElectricityProductionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(ElectricityProduction, Certificate)> CreateElectricityProduction(ElectricityProductionDto model, bool useExistingTransaction = false, string eventType = "PRODUCTION")
    {
        var transaction = useExistingTransaction ? null : await _context.Database.BeginTransactionAsync();

        try
        {
            var device = await _context.Devices.FindAsync(model.DeviceId);
            if (device == null)
            {
                throw new Exception("Device not found");
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
                Volume = model.AmountWh,
                CurrentVolume = model.AmountWh
            };
            _context.Certificates.Add(certificate);

            var eventRecord = new Event
            {
                Event_Type = eventType,
                Reference_Id = electricityProduction.Id,
                User_Id = device.UserId,
                Timestamp = DateTime.Now
            };
            _context.Events.Add(eventRecord);
            await _context.SaveChangesAsync();

            var productionEvent = new ProduceEvent
            {
                Event_Id = eventRecord.Id,
                ProductionTime = model.ProductionTime,
                DeviceId = model.DeviceId,
                ElectricityProductionId = electricityProduction.Id
            };
            _context.ProduceEvents.Add(productionEvent);
            await _context.SaveChangesAsync();

            eventRecord.Reference_Id = productionEvent.Id;
            await _context.SaveChangesAsync();

            if (transaction != null)
            {
                await transaction.CommitAsync();
            }

            return (electricityProduction, certificate);
        }
        catch (Exception)
        {
            if (transaction != null)
            {
                await transaction.RollbackAsync();
            }
            throw;
        }
    }
}
