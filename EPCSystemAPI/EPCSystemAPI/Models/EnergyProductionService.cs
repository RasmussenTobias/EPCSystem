using EPCSystemAPI.models;
using EPCSystemAPI;

public class EnergyProductionService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EnergyProductionService> _logger;

    public EnergyProductionService(ApplicationDbContext context, ILogger<EnergyProductionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(EnergyProduction, Certificate)> CreateEnergyProduction(EnergyProductionDto model, bool isTransform, string eventType)
    {
        try
        {
            var device = await _context.Devices.FindAsync(model.DeviceId);
            if (device == null)
            {
                throw new Exception("Device not found");
            }

            var energyProduction = new EnergyProduction
            {
                ProductionTime = model.ProductionTime,
                AmountWh = model.AmountWh,
                DeviceId = model.DeviceId
            };

            _context.Energy_Production.Add(energyProduction);
            await _context.SaveChangesAsync();

            var certificate = new Certificate
            {
                UserId = device.UserId,
                EnergyProductionId = energyProduction.Id,
                CreatedAt = DateTime.Now,
                Volume = model.AmountWh,
                CurrentVolume = model.AmountWh
            };
            _context.Certificates.Add(certificate);

            var produceEvent = new Event
            {
                Event_Type = eventType,
                Reference_Id = energyProduction.Id,
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
                EnergyProductionId = energyProduction.Id
            };
            _context.ProduceEvents.Add(productionEvent);

            await _context.SaveChangesAsync();

            produceEvent.Reference_Id = productionEvent.Id;
            await _context.SaveChangesAsync();

            return (energyProduction, certificate);
        }
        catch (Exception ex)
        {
            LogDetailedError(ex);
            throw;
        }
    }

    private void LogDetailedError(Exception ex)
    {
        var baseException = ex.GetBaseException();
        _logger.LogError(baseException, "An error occurred while creating energy production: {Message}", baseException.Message);
    }
}
