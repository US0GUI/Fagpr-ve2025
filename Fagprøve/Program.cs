using Fagprøve.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddAuthorization();
builder.Services.AddHttpClient<WeatherService>();
builder.Services.AddTransient<WeatherService>();

builder.Services.AddDbContext<WeatherContext>(options =>
    options.UseSqlite("Data Source=weather.db",
        sqliteOptions => sqliteOptions.MigrationsAssembly(typeof(WeatherContext).Assembly.FullName))
);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WeatherContext>();

    if (!db.WeatherEntries.Any())
    {
        db.WeatherEntries.Add(new WeatherData
        {
            CityName = "Oslo",
            Date = DateTime.Now,
            Temperature = 15.5,
            Humidity = 80,
            CloudCoverage = 20,
            Precipitation = 0,
            SymbolCode = "sunny",
            RetrievedAt = DateTime.Now
        });
        db.SaveChanges();
    }

    var entries = db.WeatherEntries.ToList();
    foreach (var entry in entries)
    {
        Console.WriteLine($"{entry.CityName} - {entry.Temperature} °C");
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
