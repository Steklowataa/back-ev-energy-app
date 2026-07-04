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
        if(hours < 1 || hours > 6)
        {
            return BadRequest(new { error = "Input value should be between 1 and 6" });
        }
        var data = await _service.GetBestChargingWindowAsync(hours);
        return Ok(data);
    }

    [HttpGet("charging-window/top/{hours}")]
    public async Task<IActionResult> BestesChargingWindow(int hours)
    {
        var data = await _service.BestesChargingWindowAsync(hours);
        return Ok(data);
    }

    [HttpGet("intensity/three-days")]
    public async Task<IActionResult> GetThreeDayIntensity()
    {
        var data = await _service.GetThreeDayIntensityAsync();
        return Ok(data);
    }

}