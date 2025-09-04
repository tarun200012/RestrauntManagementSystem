using RestaurantAPI.DTOs.Admin;
using RestaurantAPI.DTOs.Common;

namespace RestaurantAPI.Services.Interfaces
{
    public interface IAnalyticsService
    {
        Task<OrdersOverviewDto> GetOrdersOverviewAsync(int? restaurantId);
        Task<PaginatedResponse<RevenueDto>> GetRevenueAsync(int? restaurantId, int pageNumber, int pageSize, int? year,int? month,bool includeProfitability);
        Task<PaginatedResponse<MenuItemSalesDto>> GetTopItemsAsync(int? restaurantId, int pageNumber, int pageSize);
        Task<PaginatedResponse<CustomerActivityDto>> GetTopCustomersAsync(int? restaurantId, int pageNumber, int pageSize);
        Task<PaginatedResponse<PeakHourDto>> GetPeakHoursAsync(int? restaurantId, int pageNumber, int pageSize);
    }

}
