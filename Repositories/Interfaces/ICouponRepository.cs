using RestaurantAPI.Models;

namespace RestaurantAPI.Repositories.Interfaces
{
    public interface ICouponRepository : IRepository<Coupon>
    {
        Task<Coupon?> GetCouponWithDetailsAsync(int id);
        Task<IEnumerable<Coupon>> GetAllCouponsWithDetailsAsync();

        Task AddRangeAsync(IEnumerable<Coupon> entities);
    }
}
