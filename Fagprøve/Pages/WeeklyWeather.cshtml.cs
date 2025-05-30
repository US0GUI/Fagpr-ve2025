using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;

namespace Fagprøve.Pages
{
    public class WeeklyWeatherModel : PageModel
    {
        private readonly WeatherService _weatherService;

        public WeeklyWeatherModel(WeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        [BindProperty]
        public string CityName { get; set; }

        public List<WeatherNow> WeeklyForecast { get; set; }
        public double TotalPrecipitation { get; set; }
        public double AverageHumidity { get; set; }
        public double AverageCloudCoverage { get; set; }

        public async Task OnGetAsync()
        {
            await FetchWeatherAsync("Oslo"); // Standard valg
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!string.IsNullOrEmpty(CityName))
            {
                await FetchWeatherAsync(CityName);
            }

            return Page();
        }

        private async Task FetchWeatherAsync(string city)
        {
            var (lat, lon) = await _weatherService.GetCoordinatesFromCityAsync(city);
            WeeklyForecast = await _weatherService.GetWeeklyForecastAsync(lat, lon);

            if (WeeklyForecast != null && WeeklyForecast.Count > 0)
            {
                TotalPrecipitation = WeeklyForecast.Sum(d => d.Precipitation);
                AverageHumidity = WeeklyForecast.Average(d => d.Humidity);
                AverageCloudCoverage = WeeklyForecast.Average(d => d.CloudCoverage);

                // Legg til beregning av uke­nummer
                var currentCulture = CultureInfo.CurrentCulture;
                int weekNumber = currentCulture.Calendar.GetWeekOfYear(
                    DateTime.Now,
                    CalendarWeekRule.FirstFourDayWeek,
                    DayOfWeek.Monday
                );
                ViewData["WeekNumber"] = weekNumber;
            }
        }
    }
}
