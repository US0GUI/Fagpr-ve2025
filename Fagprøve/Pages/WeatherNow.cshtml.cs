using Fagprøve.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace Fagprøve.Pages
{
    public class WeatherNowModel : PageModel
    {
        private readonly WeatherService _weatherService;
        private readonly WeatherContext _context;

        // Bruker dependency injection her – grei måte å hente inn tjenestene på
        public WeatherNowModel(WeatherService weatherService, WeatherContext context)
        {
            _weatherService = weatherService;
            _context = context;
        }

        public WeatherNow Weather { get; set; }
        public string LocationName { get; set; }

        public async Task OnGetAsync(double? lat, double? lon)
        {
            // Bruker Oslo som fallback hvis vi ikke får koordinater
            double latitude = lat.HasValue ? lat.Value : 59.91;
            double longitude = lon.HasValue ? lon.Value : 10.75;

            // Henter værdata basert på koordinater
            Weather = await _weatherService.GetWeatherNowAsync(latitude, longitude);

            // Henter navnet på stedet greit for å vise
            LocationName = await _weatherService.GetCityNameAsync(latitude, longitude);

            // Kun lagre hvis bruker har sendt inn posisjon  ellers blir det rart i databasen
            if (lat != null && lon != null)
            {
                var weatherData = new WeatherData
                {
                    CityName = LocationName,
                    Date = Weather.Time,  // Tidspunktet fra selve værdataen
                    Temperature = Weather.Temperature,
                    Humidity = Weather.Humidity,
                    CloudCoverage = Weather.CloudCoverage,
                    Precipitation = Weather.Precipitation,
                    SymbolCode = Weather.SymbolCode,
                    RetrievedAt = DateTime.Now
                };

                // Legger til i databasen (kan vurdere duplikatsjekk senere)
                _context.WeatherEntries.Add(weatherData);
                await _context.SaveChangesAsync();
            }
        }
    }
}
