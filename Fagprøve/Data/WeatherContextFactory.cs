using Fagprøve.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MyProject.Data  // Husk å bruke samme namespace som WeatherContext.cs
{
    public class WeatherContextFactory : IDesignTimeDbContextFactory<WeatherContext>
    {
        public WeatherContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<WeatherContext>();
            optionsBuilder.UseSqlite("Data Source=weather.db");

            return new WeatherContext(optionsBuilder.Options);
        }
    }
}
