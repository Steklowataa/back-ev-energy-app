namespace EvEnergyApi.Models;

public record IntensityData(
    string From,
    string To,
    Intensity Intensity
);

public record Intensity(
    int? Forecast,
    int? Actual,
    string Index
);

//kolejne 48h
public record Forecast(
    string From,
    string To,
    Intensity Intensity
);

//miks

public record GenerationMixItem(string Fuel, double Perc);

public record GenerationMix(
    string From,
    string To,
    GenerationMixItem[] Generationmix
);

public record DailyGenerationSummary(
    string Date,
    GenerationMixItem[] AverageMix,
    double CleanEnergyPercent
);


public record GenerationResponse(GenerationMix[] Data);

public record ForecastResponse(Forecast[] Data);

public record CurrentIntensityResponse(IntensityData[] Data);