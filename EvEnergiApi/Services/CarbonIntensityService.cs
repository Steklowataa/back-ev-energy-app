namespace EvEnergyApi.Services;

using EvEnergyApi.Models;
using System.Text.Json;
using TimeZoneConverter;

public class CarbonIntensityService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://api.carbonintensity.org.uk";
    private readonly string[] _cleanFuels = { "biomass", "nuclear", "hydro", "wind", "solar" };
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public CarbonIntensityService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["CarbonIntensityApi:BaseUrl"] ?? throw new InvalidOperationException("Brak konfiguracji z baseurl");
    }

    //funkcja pmocnicza
    private async Task<T> GetAsync<T>(string url)
    {
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, _jsonOptions)!;
    }

    //jeden dzien, indx, forcast i actual
    public async Task<IntensityResponse> GetCurrentIntensityAsync()
    {
        var result = await GetAsync<IntensityResponse>($"{BaseUrl}/intensity");
        return result;
    }
   

    //zadanie 1, trzy dni, czysta energia + srednie
    public async Task<DailyGenerationSummary[]> GetThreeDayGenerationAsync()
    {
        var from = DateTime.UtcNow.Date.ToString("yyyy-MM-ddT00:00Z");
        var to = DateTime.UtcNow.Date.AddDays(2).ToString("yyyy-MM-ddT23:30Z");
        var londonZone = TZConvert.GetTimeZoneInfo("Europe/London");
        var result = await GetAsync<GenerationResponse>($"{BaseUrl}/generation/{from}/{to}");

        return result.Data
            .GroupBy(slot => TimeZoneInfo.ConvertTimeFromUtc(DateTimeOffset.Parse(slot.From).UtcDateTime, londonZone).Date)
            .OrderBy(g => g.Key)
            .Select(dayGroup =>
            {
                var fuels = dayGroup
                    .SelectMany(s => s.Generationmix)
                    .GroupBy(m => m.Fuel)
                    .Select(g => new GenerationMixItem(
                        g.Key,
                        Math.Round(g.Average(m => m.Perc), 1)
                    ))
                    .ToArray();

                var cleanPercent = Math.Round(
                    fuels.Where(f => _cleanFuels.Contains(f.Fuel)).Sum(f => f.Perc), 1
                );

                return new DailyGenerationSummary(
                    dayGroup.Key.ToString("yyyy-MM-dd"),
                    fuels,
                    cleanPercent
                );
            })
            .ToArray();
    }

    //zadanie 2 okno
    public async Task<ChargingWindow> GetBestChargingWindowAsync(int hours)
    {
        var from = DateTime.UtcNow.Date.AddDays(1).ToString("yyyy-MM-ddT00:00Z");
        var to = DateTime.UtcNow.Date.AddDays(2).ToString("yyyy-MM-ddT23:30Z");

        var result = await GetAsync<GenerationResponse>($"{BaseUrl}/generation/{from}/{to}");
        var slots = result.Data;

        var windowSize = hours * 2;

        var cleanPerSlot = slots
            .Select(slot => slot.Generationmix
                .Where(m => _cleanFuels.Contains(m.Fuel))
                .Sum(m => m.Perc))
            .ToArray();

        var bestIndex = 0;
        var bestAverage = 0.0;

        for (int i = 0; i <= slots.Length - windowSize; i++)
        {
            var average = cleanPerSlot.Skip(i).Take(windowSize).Average();
            if (average > bestAverage)
            {
                bestAverage = average;
                bestIndex = i;
            }
        }

        return new ChargingWindow(
            slots[bestIndex].From,
            slots[bestIndex + windowSize - 1].To,
            Math.Round(bestAverage, 1)
        );
    }


    //dodatkowe metody nie dotyczące zadania
    //drugie i trzecie miejsce

    public async Task<ChargingWindowNotTop[]> BestesChargingWindowAsync(int hours)
{
    var from = DateTime.UtcNow.Date.AddDays(1).ToString("yyyy-MM-ddT00:00Z");
    var to = DateTime.UtcNow.Date.AddDays(2).ToString("yyyy-MM-ddT23:30Z");

    var result = await GetAsync<GenerationResponse>($"{BaseUrl}/generation/{from}/{to}");

    if(result?.Data == null || result.Data.Length == 0)
    {   
        Console.WriteLine("no data");
        throw new Exception("no data");
    }


    var slots = result.Data;

    Console.WriteLine("slots: ", slots.Length);

    var windowSize = hours * 2;

    Console.WriteLine($"Requested hours: {hours}");
    Console.WriteLine($"Window size (30min slots): {windowSize}");

    if(slots == null || slots.Length == 0)
    {
        throw new Exception("no data");
    }
    
    var cleanPerSlot = slots
        .Select(slot => slot.Generationmix
            .Where(m => _cleanFuels.Contains(m.Fuel))
            .Sum(m => m.Perc))
        .ToArray();

    var allWindows = new List<(int Index, double Average)>();

    for (int i = 0; i <= slots.Length - windowSize; i++)
    {
        var average = cleanPerSlot.Skip(i).Take(windowSize).Average();
        allWindows.Add((i, average));
    }

    return allWindows
        .OrderByDescending(w => w.Average)
        .Take(3)
        .Select((w, rank) => new ChargingWindowNotTop(
            slots[w.Index].From,
            slots[w.Index + windowSize - 1].To,
            Math.Round(w.Average, 1),
            rank + 1
        )).ToArray();
    }

     public async Task<DailyIntensitySummary[]> GetThreeDayIntensityAsync()
    {
        var from = DateTime.UtcNow.Date.ToString("yyyy-MM-ddT00:00Z");
        var to = DateTime.UtcNow.Date.AddDays(2).ToString("yyyy-MM-ddT23:30Z");
        var londonZone = TZConvert.GetTimeZoneInfo("Europe/London");
        var result = await GetAsync<IntensityResponse>($"{BaseUrl}/intensity/{from}/{to}");

        return result.Data
            .GroupBy(slot => TimeZoneInfo.ConvertTimeFromUtc(DateTimeOffset.Parse(slot.From).UtcDateTime, londonZone).Date)
            .OrderBy(g => g.Key)
            .Select(dayGroup =>
            {
            var slots = dayGroup.ToList();
            var avgForecast = Math.Round(
                slots.Where(s => s.Intensity.Forecast.HasValue)
                     .Average(s => s.Intensity.Forecast!.Value), 1
            );
            var index = slots
                .GroupBy(s => s.Intensity.Index)
                .OrderByDescending(g => g.Count())
                .First().Key;

            return new DailyIntensitySummary(
                dayGroup.Key.ToString("yyyy-MM-dd"),
                avgForecast,
                index
            );
        }).ToArray();
    }
}