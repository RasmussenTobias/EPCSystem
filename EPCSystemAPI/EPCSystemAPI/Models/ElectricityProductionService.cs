using EPCSystemAPI.models;
using EPCSystemAPI;

public class ElectricityProductionService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ElectricityProductionService> _logger;

    public ElectricityProductionService(ApplicationDbContext context, ILogger<ElectricityProductionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(ElectricityProduction, Certificate)> CreateElectricityProduction(ElectricityProductionDto model, bool isTransform, string eventType)
    {
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

            var produceEvent = new Event
            {
                Event_Type = eventType,
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

            await _context.SaveChangesAsync();

            produceEvent.Reference_Id = productionEvent.Id;
            await _context.SaveChangesAsync();

            return (electricityProduction, certificate);
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
        _logger.LogError(baseException, "An error occurred while creating electricity production: {Message}", baseException.Message);
    }
}
