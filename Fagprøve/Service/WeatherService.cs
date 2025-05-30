using Fagprøve.Pages;
using System.Text.Json;
using System.Globalization;
using MyProject.Data;

public class WeatherService
{
    private readonly HttpClient _httpClient;
    private const string UserAgent = "FagproeveApp/1.0 (alex@proweb.no)";

    public WeatherService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Henter været akkurat nå standard fallback til oslo
    public async Task<WeatherNow> GetWeatherNowAsync(double lat = 59.91, double lon = 10.75)
    {
        var request = new HttpRequestMessage(HttpMethod.Get,
            $"https://api.met.no/weatherapi/locationforecast/2.0/compact?lat={lat}&lon={lon}");
        request.Headers.Add("User-Agent", UserAgent);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);

        var now = doc.RootElement
            .GetProperty("properties")
            .GetProperty("timeseries")[0];

        var details = now
            .GetProperty("data")
            .GetProperty("instant")
            .GetProperty("details");

        string symbolCode = "unknown";

        if (now.TryGetProperty("data", out var data) &&
            data.TryGetProperty("next_1_hours", out var nextHour) &&
            nextHour.TryGetProperty("summary", out var summary) &&
            summary.TryGetProperty("symbol_code", out var symbol))
        {
            symbolCode = symbol.GetString();
        }

        return new WeatherNow
        {
            Time = DateTime.Parse(now.GetProperty("time").GetString()),
            Temperature = details.GetProperty("air_temperature").GetDouble(),
            CloudCoverage = details.GetProperty("cloud_area_fraction").GetDouble(),
            Humidity = details.GetProperty("relative_humidity").GetDouble(),
            SymbolCode = symbolCode
        };
    }

    // Henter bynavn fra koordinater
    public async Task<string> GetCityNameAsync(double lat, double lon)
    {
        var url = $"https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat={lat}&lon={lon}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("User-Agent", UserAgent);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            // fallback visning
            return $"Lat: {lat:F2}, Lon: {lon:F2}";
        }

        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);

        if (doc.RootElement.TryGetProperty("address", out var address))
        {
            if (address.TryGetProperty("city", out var city))
                return city.GetString();
            if (address.TryGetProperty("town", out var town))
                return town.GetString();
            if (address.TryGetProperty("village", out var village))
                return village.GetString();
        }

        return $"Lat: {lat:F2}, Lon: {lon:F2}"; // fallback
    }

    // Hent ukesprognose med nedbør
    public async Task<List<WeatherNow>> GetWeeklyForecastAsync(double lat = 59.91, double lon = 10.75)
    {
        var request = new HttpRequestMessage(HttpMethod.Get,
            $"https://api.met.no/weatherapi/locationforecast/2.0/compact?lat={lat}&lon={lon}");
        request.Headers.Add("User-Agent", UserAgent);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);

        var timeseries = doc.RootElement
            .GetProperty("properties")
            .GetProperty("timeseries");

        var seenDates = new HashSet<DateTime>();
        var weeklyForecast = new List<WeatherNow>();

        foreach (var entry in timeseries.EnumerateArray())
        {
            var time = DateTime.Parse(entry.GetProperty("time").GetString());
            var dateOnly = time.Date;

            if (seenDates.Contains(dateOnly))
                continue;

            var data = entry.GetProperty("data");
            var details = data.GetProperty("instant").GetProperty("details");

            string symbolCode = "unknown";
            double precipitation = 0;

            if (data.TryGetProperty("next_1_hours", out var next1h) &&
                next1h.TryGetProperty("summary", out var sum1) &&
                sum1.TryGetProperty("symbol_code", out var sym1))
            {
                symbolCode = sym1.GetString();

                if (next1h.TryGetProperty("details", out var next1hDetails) &&
                    next1hDetails.TryGetProperty("precipitation_amount", out var precip))
                {
                    precipitation = precip.GetDouble();
                }
            }
            else if (data.TryGetProperty("next_6_hours", out var next6h) &&
                     next6h.TryGetProperty("summary", out var sum6) &&
                     sum6.TryGetProperty("symbol_code", out var sym6))
            {
                symbolCode = sym6.GetString();

                if (next6h.TryGetProperty("details", out var next6hDetails) &&
                    next6hDetails.TryGetProperty("precipitation_amount", out var precip))
                {
                    precipitation = precip.GetDouble();
                }
            }
            else if (data.TryGetProperty("next_12_hours", out var next12h) &&
                     next12h.TryGetProperty("summary", out var sum12) &&
                     sum12.TryGetProperty("symbol_code", out var sym12))
            {
                symbolCode = sym12.GetString();

                if (next12h.TryGetProperty("details", out var next12hDetails) &&
                    next12hDetails.TryGetProperty("precipitation_amount", out var precip))
                {
                    precipitation = precip.GetDouble();
                }
            }

            weeklyForecast.Add(new WeatherNow
            {
                Time = time,
                Temperature = details.GetProperty("air_temperature").GetDouble(),
                CloudCoverage = details.GetProperty("cloud_area_fraction").GetDouble(),
                Humidity = details.GetProperty("relative_humidity").GetDouble(),
                SymbolCode = symbolCode,
                Precipitation = precipitation
            });

            seenDates.Add(dateOnly);

            if (weeklyForecast.Count == 7)
                break;
        }

        // Søndag til slutt  bare så det ser ryddig ut
        weeklyForecast = weeklyForecast
            .OrderBy(d => d.Time.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)d.Time.DayOfWeek)
            .ToList();

        return weeklyForecast;
    }

    // Hent lat/lon basert på bynavn
    public async Task<(double lat, double lon)> GetCoordinatesFromCityAsync(string city)
    {
        var encodedCity = System.Web.HttpUtility.UrlEncode(city);
        var url = $"https://nominatim.openstreetmap.org/search?format=json&q={encodedCity}&limit=1";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("User-Agent", UserAgent);

        try
        {
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return (59.91, 10.75); // fallback: Oslo

            var content = await response.Content.ReadAsStringAsync();
            var results = JsonSerializer.Deserialize<List<NominatimResult>>(content);

            if (results != null && results.Count > 0)
            {
                var first = results[0];
                return (
                    double.Parse(first.lat, CultureInfo.InvariantCulture),
                    double.Parse(first.lon, CultureInfo.InvariantCulture)
                );
            }
        }
        catch
        {
            // Kunne logget feilen, men går videre
        }

        return (59.91, 10.75); // fallback
    }

    // Intern klasse for deserialisering
    private class NominatimResult
    {
        public string lat { get; set; }
        public string lon { get; set; }
    }
}
