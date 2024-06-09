using EPCSystemAPI.models;
using EPCSystemAPI.Services;
using EPCSystemAPI;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class EnergyProductionController : ControllerBase
{
    // Dependency injections for dbcontext
    private readonly ApplicationDbContext _context;
    private readonly EnergyProductionService _productionService;
    private readonly ILogger<EnergyProductionController> _logger;

    // Dependency constructor
    public EnergyProductionController(ApplicationDbContext context, EnergyProductionService productionService, ILogger<EnergyProductionController> logger)
    {
        _context = context; 
        _productionService = productionService; 
        _logger = logger;
    }

    // POST endpoint for creating a new energy production entry
    [HttpPost]
    public async Task<IActionResult> CreateEnergyProduction([FromBody] EnergyProductionDto model)
    {
        // Validation check on the incoming data
        if (!ModelState.IsValid)
        {
            // Return Badrequest response
            return BadRequest(ModelState);
        }

        try
        {
            // Call to service layer to the energyproductionservice
            var result = await _productionService.CreateEnergyProduction(model, false, "PRODUCTION");
            var energyProduction = result.Item1; 
            var certificate = result.Item2; 

            // Return success
            return Ok(new { EnergyProduction = energyProduction, Certificate = certificate });
        }
        //Return an error
        catch (Exception ex)
        {            
            _logger.LogError(ex, "An error occurred while processing the request.");            
            return StatusCode(500, $"An error occurred while processing your request: {ex.Message}. See the inner exception for details.");
        }
    }
}