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


    [HttpGet("generation/three-days")]
    public async Task<IActionResult> GetThreeDay()
    {
        var data = await _service.GetThreeDayGenerationAsync();
        return Ok(data);
    }

    [HttpGet("charging-window/{hours}")]
    public async Task<IActionResult> GetBestChargingWindow(int hours)
    {
        var data = await _service.GetBestChargingWindowAsync(hours);
        return Ok(data);
    }
}