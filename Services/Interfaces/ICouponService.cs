using RestaurantAPI.Dtos;
using RestaurantAPI.DTOs;
using RestaurantAPI.DTOs.Common;
using RestaurantAPI.DTOs.RestaurantAPI.DTOs.Coupon;
using RestaurantAPI.Models;
using RestaurantAPI.Models.Common;

namespace RestaurantAPI.Services.Interfaces
{
    public interface ICouponService
    {
        Task<IEnumerable<Coupon>> GetAllCouponsAsync();
        Task<Coupon?> GetCouponByIdAsync(int id);
        Task<Coupon> CreateCouponAsync(CreateCouponRequestDto dto);
        Task<Coupon?> UpdateCouponAsync(int id, CreateCouponRequestDto dto);
        Task<bool> DeleteCouponAsync(int id);

        Task CreateAutomaticCoupons(int? restaurantId);

        Task<PaginatedResponse<CouponResponseDto>> GetPaginatedAsync(PaginatedRequest request);

        Task<List<CouponResponseDto>> GetCouponsForRestaurantAndCustomerAsync(int? restaurantId, int? customerId);
    }
}
