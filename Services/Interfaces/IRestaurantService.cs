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

    }
}
