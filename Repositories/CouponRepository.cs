using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Data;
using RestaurantAPI.Models;
using RestaurantAPI.Repositories.Interfaces;

namespace RestaurantAPI.Repositories
{
    public class CouponRepository : ICouponRepository
    {
        private readonly ApplicationDbContext _context;

        public CouponRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Coupon?> GetCouponWithDetailsAsync(int id)
        {
            return await _context.Coupons
                .Include(c => c.CouponRestaurants)
                    .ThenInclude(cr => cr.Restaurant)
                .Include(c => c.CouponCustomers)
                    .ThenInclude(cc => cc.Customer)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Coupon>> GetAllCouponsWithDetailsAsync()
        {
            return await _context.Coupons
                .Include(c => c.CouponRestaurants)
                    .ThenInclude(cr => cr.Restaurant)
                .Include(c => c.CouponCustomers)
                    .ThenInclude(cc => cc.Customer)
                .ToListAsync();
        }
        public async Task<IEnumerable<Coupon>> GetAllAsync()
        {
            // Eager load related restaurants & customers if needed
            return await _context.Coupons
                .ToListAsync();
        }

        public async Task<Coupon?> GetByIdAsync(int id)
        {
            return await _context.Coupons
                .Include(c => c.CouponRestaurants)
                    .ThenInclude(cr => cr.Restaurant)
                .Include(c => c.CouponCustomers)
                    .ThenInclude(cc => cc.Customer)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Coupon entity)
        {
            await _context.Coupons.AddAsync(entity);
        }

        public void Update(Coupon entity)
        {
            _context.Coupons.Update(entity);
        }

        public void Delete(Coupon entity)
        {
            _context.Coupons.Remove(entity);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<Coupon> entities)
        {
            await _context.AddRangeAsync(entities);
        }
    }
}
