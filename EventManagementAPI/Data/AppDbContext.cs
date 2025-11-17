using Microsoft.EntityFrameworkCore;
using EventManagementAPI.Models;

namespace EventManagementAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
           : base(options)
        {
        }

        public DbSet<Event> Events { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Achievement> Achievements { get; set; }

        public DbSet<Gallery> Galleries { get; set; }
        public DbSet<GalleryImage> GalleryImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ✅ Convert Enum → String in database
            modelBuilder
                .Entity<Achievement>()
                .Property(a => a.Category)
                .HasConversion<string>();

            base.OnModelCreating(modelBuilder);

        }
    }
}

