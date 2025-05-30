using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;

namespace Fagprøve.Pages
{
    public class WeatherAnalysisModel : PageModel
    {
        private readonly WeatherService _weatherService;

        public WeatherAnalysisModel(WeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        public List<WeatherNow> WeeklyForecast { get; set; }
        public double AverageTemp { get; set; }
        public double MaxTemp { get; set; }
        public double MinTemp { get; set; }
        public double AverageHumidity { get; set; }
        public double MaxHumidity { get; set; }
        public double MinHumidity { get; set; }
        public double AverageClouds { get; set; }
        public double TotalPrecipitation { get; set; }
        public double MaxPrecipitation { get; set; }
        public int ClearDays { get; set; }
        public int CloudyDays { get; set; }
        public int RainyDays { get; set; }

        public string City { get; set; } = "Oslo";
        public int WeekNumber { get; set; }

        public async Task OnGetAsync()
        {
            await LoadWeatherDataAsync(City);
        }

        public async Task OnPostAsync(string city)
        {
            if (!string.IsNullOrWhiteSpace(city))
            {
                City = city;
            }

            await LoadWeatherDataAsync(City);
        }

        private async Task LoadWeatherDataAsync(string city)
        {
            var (lat, lon) = await _weatherService.GetCoordinatesFromCityAsync(city);
            WeeklyForecast = await _weatherService.GetWeeklyForecastAsync(lat, lon);

            if (WeeklyForecast?.Any() == true)
            {
                AverageTemp = WeeklyForecast.Average(w => w.Temperature);
                MaxTemp = WeeklyForecast.Max(w => w.Temperature);
                MinTemp = WeeklyForecast.Min(w => w.Temperature);

                AverageHumidity = WeeklyForecast.Average(w => w.Humidity);
                MaxHumidity = WeeklyForecast.Max(w => w.Humidity);
                MinHumidity = WeeklyForecast.Min(w => w.Humidity);

                AverageClouds = WeeklyForecast.Average(w => w.CloudCoverage);
                TotalPrecipitation = WeeklyForecast.Sum(w => w.Precipitation);
                MaxPrecipitation = WeeklyForecast.Max(w => w.Precipitation);

                ClearDays = WeeklyForecast.Count(w => w.CloudCoverage < 25);
                CloudyDays = WeeklyForecast.Count(w => w.CloudCoverage > 75);
                RainyDays = WeeklyForecast.Count(w =>
                    w.SymbolCode.Contains("rain", StringComparison.OrdinalIgnoreCase) ||
                    w.SymbolCode.Contains("showers", StringComparison.OrdinalIgnoreCase)
                );

                WeekNumber = ISOWeek.GetWeekOfYear(WeeklyForecast.First().Time);
            }
        }
    }
}
