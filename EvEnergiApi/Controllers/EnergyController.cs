namespace EvEnergyApi.Controllers;

using EvEnergyApi.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class EnergyController: ControllerBase
{
    private readonly CarbonIntensityService _service;

    public EnergyController(CarbonIntensityService service)
    {
        _service = service;
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentResult()
    {
        var data = await _service.GetCurrentIntensityAsync();
        return Ok(data);
    }

    [HttpGet("forecast")]
    public async Task<IActionResult> GetForecast()
    {
        var data = await _service.GetForecastAsync();
        return Ok(data);
    }

    [HttpGet("generation")]
    public async Task<IActionResult> GetGeneration()
    {
        var data = await _service.GetGenerationAsync();
        return Ok(data);
    }

    [HttpGet("generation/three-days")]
    public async Task<IActionResult> GetThreeDay()
    {
        var data = await _service.GetThreeDayGenerationAsync();
        return Ok(data);
    }
}