namespace EvEnergyApi.Models;

public record IntensitySlot(
    string From,
    string To,
    Intensity Intensity
);

public record Intensity(
    int? Forecast,
    int? Actual,
    string Index
);

// miks energetyczny
public record GenerationMixItem(string Fuel, double Perc);

public record GenerationSlot(
    string From,
    string To,
    GenerationMixItem[] Generationmix
);

// odpowiedzi z API
public record IntensityResponse(IntensitySlot[] Data);
public record GenerationResponse(GenerationSlot[] Data);

// odpowiedzi na frontend
public record DailyGenerationSummary(
    string Date,
    GenerationMixItem[] AverageMix,
    double CleanEnergyPercent
);

public record ChargingWindow(
    string From,
    string To,
    double CleanEnergyPercent
);

//dodatkowe dane nie dotyczące zadania
//drugie i trzecie miejsce

public record ChargingWindowNotTop(
    string From,
    string To,
    double CleanEnergyPercent,
    int Rank
);

// jutro i pojutrze forecasr + index
public record DailyIntensitySummary(
    string Date,
    double AverageForecast,
    string Index
);
