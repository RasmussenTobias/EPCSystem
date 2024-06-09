using EPCSystemAPI.models;
using EPCSystemAPI.Services;
using EPCSystemAPI;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class ElectricityProductionController : ControllerBase
{
    // Dependency injections for dbcontext
    private readonly ApplicationDbContext _context;
    private readonly ElectricityProductionService _productionService;
    private readonly ILogger<ElectricityProductionController> _logger;

    // Dependency constructor
    public ElectricityProductionController(ApplicationDbContext context, ElectricityProductionService productionService, ILogger<ElectricityProductionController> logger)
    {
        _context = context; 
        _productionService = productionService; 
        _logger = logger;
    }

    // POST endpoint for creating a new electricity production entry
    [HttpPost]
    public async Task<IActionResult> CreateElectricityProduction([FromBody] ElectricityProductionDto model)
    {
        // Validation check on the incoming data
        if (!ModelState.IsValid)
        {
            // Return Badrequest response
            return BadRequest(ModelState);
        }

        try
        {
            // Call to service layer to the electricityproductionservice
            var result = await _productionService.CreateElectricityProduction(model, false, "PRODUCTION");
            var electricityProduction = result.Item1; 
            var certificate = result.Item2; 

            // Return success
            return Ok(new { ElectricityProduction = electricityProduction, Certificate = certificate });
        }
        //Return an error
        catch (Exception ex)
        {            
            _logger.LogError(ex, "An error occurred while processing the request.");            
            return StatusCode(500, $"An error occurred while processing your request: {ex.Message}. See the inner exception for details.");
        }
    }
}