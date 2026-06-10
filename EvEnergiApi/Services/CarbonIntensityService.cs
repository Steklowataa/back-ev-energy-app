namespace EvEnergyApi.Services;

using EvEnergyApi.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

public class CarbonIntensityService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://api.carbonintensity.org.uk";
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions 
    { 
        PropertyNameCaseInsensitive = true 
    };

    public CarbonIntensityService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    //funkcja pomocnicza
    private async Task<T> GetAsync<T>(string url)
    {
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, _jsonOptions)!;
    }

    public async Task<IntensityData> GetCurrentIntensityAsync()

    {
       var result = await GetAsync<CurrentIntensityResponse>($"{BaseUrl}/intensity");
       return result.Data[0];
    
    }

    public async Task<Forecast[]> GetForecastAsync()
    {
        var startDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mmZ");
        var result = await GetAsync<ForecastResponse>($"{BaseUrl}/forecast/{startDate}/pt24h");
        return result.Data;
    }
        

    public async Task<GenerationMix> GetGenerationAsync()
    {
        var from = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mmZ");
        var result = await GetAsync<GenerationResponse>($"{BaseUrl}/generation/{from}/pt24h");
        return result!.Data[0];

    }

  public async Task<DailyGenerationSummary[]> GetThreeDayGenerationAsync()
{
    var from = DateTime.UtcNow.Date.ToString("yyyy-MM-ddT00:00Z");
    var to = DateTime.UtcNow.Date.AddDays(2).ToString("yyyy-MM-ddT23:30Z");

    var result = await GetAsync<GenerationResponse>($"{BaseUrl}/generation/{from}/{to}");
    var slots = result.Data;

    var grouped = slots
        .GroupBy(slot => DateTime.Parse(slot.From).Date)
        .OrderBy(g => g.Key)
        .ToList();

    var cleanFuels = new[] { "biomass", "nuclear", "hydro", "wind", "solar" };

    var summaries = grouped.Select(dayGroup =>
    {
        var date = dayGroup.Key.ToString("yyyy-MM-dd");
        var slotsInDay = dayGroup.ToList();

        var fuels = slotsInDay
            .SelectMany(s => s.Generationmix)
            .GroupBy(m => m.Fuel)
            .Select(fuelGroup => new GenerationMixItem(
                fuelGroup.Key,
                Math.Round(fuelGroup.Average(m => m.Perc), 1)
            ))
            .ToArray();


        var cleanPercent = Math.Round(
            fuels
                .Where(f => cleanFuels.Contains(f.Fuel))
                .Sum(f => f.Perc),
            1
        );

        return new DailyGenerationSummary(date, fuels, cleanPercent);
    }).ToArray();

    return summaries;
}
}
