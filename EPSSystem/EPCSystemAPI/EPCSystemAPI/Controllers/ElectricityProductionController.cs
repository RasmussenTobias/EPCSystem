using EPCSystemAPI.models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("[controller]")]
public class ElectricityProductionController : ControllerBase
{
    private readonly ElectricityProductionService _productionService;

    public ElectricityProductionController(ElectricityProductionService productionService)
    {
        _productionService = productionService;
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
            var result = await _productionService.CreateElectricityProduction(model);
            return Ok(new { ElectricityProduction = result.Item1, Certificate = result.Item2 });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while processing your request: {ex.Message}");
        }
    }
}
