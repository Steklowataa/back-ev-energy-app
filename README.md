# EV Energy API

ASP.NET Core Web API providing energy mix and EV charging window data for Great Britain, based on the [Carbon Intensity API](https://api.carbonintensity.org.uk).

## Requirements

- .NET 8 SDK

## Getting started

```bash
cd EvEnergiApi
dotnet run
```

API runs on `http://localhost:5170`.

## Endpoints

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/energy/current` | Current CO₂ intensity |
| GET | `/api/energy/generation/three-days` | Energy mix for today, tomorrow and day after |
| GET | `/api/energy/intensity/three-days` | Average forecast intensity for 3 days |
| GET | `/api/energy/charging-window/{hours}` | Best charging window (1–6h) |
| GET | `/api/energy/charging-window/top3?hours={hours}` | Top 3 charging windows |

## Configuration

```json
{
  "CarbonIntensityApi": {
    "BaseUrl": "https://api.carbonintensity.org.uk"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173"]
  }
}
```

## Running tests

```bash
cd EvEnergyApi.Tests
dotnet test
```

## Poprawione rzeczy dotyczące feedbacku:

- Poprawiłam problem w grupowaniu dotyczący stref czasowych, wybrałam strefe Londyn, więc teraz nie musi być problemu z przeżucaniem na czwarty dzień
  
Dodano paczke dotnet add package TimeZoneConverter
- Dodano testy jednostkowe dla backendu ( grupowanie dni, liczenie procentu czystej energii, wybór najlepszego okna ładowania)
- Walidacja parametru godzin (1-6)
- Wyniesienie konfiguracji backendu poza kod
