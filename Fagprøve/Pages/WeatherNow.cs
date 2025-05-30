namespace Fagprøve.Pages
{
    public class WeatherNow
    {
        public DateTime Time { get; set; }
        public double Temperature { get; set; }
        public double CloudCoverage { get; set; }
        public double Humidity { get; set; }
        public string SymbolCode { get; set; }

        public double Precipitation { get; set; } // 👈 LEGG TIL DENNE
        public DateTime Date { get; internal set; }
    }
}
