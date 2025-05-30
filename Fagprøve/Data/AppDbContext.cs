using Microsoft.EntityFrameworkCore;
using Fagprøve.Models;

namespace Fagprøve.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
    }
}
