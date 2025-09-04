using Microsoft.EntityFrameworkCore;
using RestaurantAPI.DTOs.Admin;
using RestaurantAPI.Models;

namespace RestaurantAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<Role> Role { get; set; }

        public DbSet<Coupon> Coupons { get; set; }

        public DbSet<CouponRestaurant> CouponRestaurants { get; set; }

        public DbSet<CouponCustomer> CouponsCustomer { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<RevenueDto> RevenueDtos { get; set; } = null!; // keyless mapping for SP output
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Coupon ↔ Restaurant (many-to-many)
            modelBuilder.Entity<CouponRestaurant>()
                .HasKey(cr => new { cr.CouponId, cr.RestaurantId });

            modelBuilder.Entity<CouponRestaurant>()
                .HasOne(cr => cr.Coupon)
                .WithMany(c => c.CouponRestaurants)
                .HasForeignKey(cr => cr.CouponId);

            modelBuilder.Entity<CouponRestaurant>()
                .HasOne(cr => cr.Restaurant)
                .WithMany(r => r.CouponRestaurants)
                .HasForeignKey(cr => cr.RestaurantId);

            // Coupon ↔ Customer (many-to-many)
            modelBuilder.Entity<CouponCustomer>()
                .HasKey(cc => new { cc.CouponId, cc.CustomerId });

            modelBuilder.Entity<CouponCustomer>()
                .HasOne(cc => cc.Coupon)
                .WithMany(c => c.CouponCustomers)
                .HasForeignKey(cc => cc.CouponId);

            modelBuilder.Entity<CouponCustomer>()
                .HasOne(cc => cc.Customer)
                .WithMany(cu => cu.CouponCustomers)
                .HasForeignKey(cc => cc.CustomerId);

            //one-many orders-coupouns
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Coupon)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CouponId)
                .OnDelete(DeleteBehavior.SetNull);

            // keyless mapping for stored proc result
            modelBuilder.Entity<RevenueDto>().HasNoKey().ToView(null); // not mapped to a table or view

            // Unique Email
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique();

            // Customer ↔ Role relationship
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.Role)
                .WithMany(r => r.Customers)
                .HasForeignKey(c => c.RoleId);

            modelBuilder.Entity<Restaurant>()
                .HasOne(r => r.Location)
                .WithOne(l => l.Restaurant)
                .HasForeignKey<Restaurant>(r => r.LocationId)
                .OnDelete(DeleteBehavior.Restrict);// Delete both if needed

            // One-to-many
            modelBuilder.Entity<MenuItem>()
                .HasOne(mi => mi.Restaurant)
                .WithMany(r => r.MenuItems)
                .HasForeignKey(mi => mi.RestaurantId)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Restaurant)
                .WithMany(r => r.Orders)
                .HasForeignKey(o => o.RestaurantId)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.MenuItem)
                .WithMany()
                .HasForeignKey(oi => oi.MenuItemId)
                 .OnDelete(DeleteBehavior.Restrict);
                 

            modelBuilder.Entity<Restaurant>().HasQueryFilter(r => !r.IsDeleted);
            modelBuilder.Entity<MenuItem>().HasQueryFilter(m => !m.IsDeleted);
            modelBuilder.Entity<Customer>().HasQueryFilter(c => !c.IsDeleted);


        }
    }
}
