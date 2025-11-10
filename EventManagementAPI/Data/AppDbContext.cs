using Microsoft.EntityFrameworkCore;
using EventManagementAPI.Models;

namespace EventManagementAPI.Data
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

         public DbSet<Event> Events { get; set; }
    }
}
