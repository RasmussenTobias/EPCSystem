using EPCSystemAPI.models;
using EPCSystemAPI.Services;
using EPCSystemAPI;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class ElectricityProductionController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ElectricityProductionService _productionService;
    private readonly ILogger<ElectricityProductionController> _logger;

    public ElectricityProductionController(ApplicationDbContext context, ElectricityProductionService productionService, ILogger<ElectricityProductionController> logger)
    {
        _context = context;
        _productionService = productionService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateElectricityProduction([FromBody] ElectricityProductionDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _productionService.CreateElectricityProduction(model, false, "PRODUCTION");
            var electricityProduction = result.Item1;
            var certificate = result.Item2;

            return Ok(new { ElectricityProduction = electricityProduction, Certificate = certificate });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request.");
            return StatusCode(500, $"An error occurred while processing your request: {ex.Message}. See the inner exception for details.");
        }
    }
}
