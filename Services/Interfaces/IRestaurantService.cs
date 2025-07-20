using RestaurantAPI.Dtos;
using RestaurantAPI.Models;

namespace RestaurantAPI.Services.Interfaces
{
    public interface IRestaurantService
    {
        Task<IEnumerable<Restaurant>> GetAllAsync();
        Task<IEnumerable<Restaurant>> GetAllWithLocationsAsync();
        Task<Restaurant?> GetByIdAsync(int id);
        Task<Restaurant> AddAsync(Restaurant restaurant);
        Task<Restaurant> UpdateAsync(Restaurant restaurant);
        Task DeleteAsync(int id);
        Task<IEnumerable<Restaurant>> AddBulkAsync(List<Restaurant> restaurants);
        Task<RestaurantWithLocationDto> CreateWithLocationAsync(RestaurantWithLocationDto dto);
        Task SoftDeleteAsync(int id);
        Task<RestaurantWithLocationDto> UpdateWithLocationAsync(int id, RestaurantWithLocationDto dto); // ✅ Added missing method signature
        Task<List<RestaurantWithLocationDto>> BulkCreateWithLocationAsync(List<RestaurantWithLocationDto> dtoList);
        Task<List<MenuItem>> GetMenuByRestaurantIdAsync(int restaurantId);

    }
}
