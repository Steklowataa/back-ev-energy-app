# EV Energy API

ASP.NET Core Web API udostępnianie danych dotyczących miksu energetycznego i okien ładowania pojazdów elektrycznych dla Wielkiej Brytanii na podstawie: [Carbon Intensity API](https://api.carbonintensity.org.uk).

API runs on `http://localhost:5170`.

## Endpoints

| Metoda | Endpoint | Opis |
|---|---|---|
| GET | `/api/energy/current` | Current CO₂ intensity |
| GET | `/api/energy/generation/three-days` | Energy mix for today, tomorrow and day after |
| GET | `/api/energy/intensity/three-days` | Average forecast intensity for 3 days |
| GET | `/api/energy/charging-window/{hours}` | Best charging window (1–6h) |
| GET | `/api/energy/charging-window/top3?hours={hours}` | Top 3 charging windows |

## Konfiguracja

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

## Testy

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
