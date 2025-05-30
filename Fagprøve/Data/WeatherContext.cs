using Microsoft.EntityFrameworkCore;
using System;

namespace Fagprøve.Data  // Bruk ditt faktiske prosjekt-namespace
{
    public class WeatherContext : DbContext
    {
        public WeatherContext(DbContextOptions<WeatherContext> options)
            : base(options)
        {
            // Aktiver WAL-modus eksplisitt hvis ønskelig
            Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
        }

        public DbSet<WeatherData> WeatherEntries { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            // ✅ Valgfritt: Aktiver enkel logging for debugging
            optionsBuilder.LogTo(Console.WriteLine);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Valgfritt: Marker at vi bruker WAL (dokumentasjon/metadata)
            modelBuilder.HasAnnotation("Sqlite:JournalMode", "WAL");
        }
    }

    public class WeatherData
    {
        public int Id { get; set; }
        public string CityName { get; set; }
        public DateTime Date { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double CloudCoverage { get; set; }
        public double Precipitation { get; set; }
        public string SymbolCode { get; set; }
        public DateTime RetrievedAt { get; set; }
    }
}
