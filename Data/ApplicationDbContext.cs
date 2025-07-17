using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Models;

namespace RestaurantAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<Location> Locations { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Restaurant>()
                .HasOne(r => r.Location)
                .WithOne(l => l.Restaurant)
                .HasForeignKey<Restaurant>(r => r.LocationId)
                .OnDelete(DeleteBehavior.Cascade); // Delete both if needed

            modelBuilder.Entity<Restaurant>().HasQueryFilter(r => !r.IsDeleted);


        }
    }
}
