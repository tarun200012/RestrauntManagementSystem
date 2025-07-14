using RestaurantAPI.Models;

namespace RestaurantAPI.Services.Interfaces
{
    public interface ILocationService
    {
        Task<IEnumerable<Location>> GetAllAsync();
        Task<Location?> GetByIdAsync(int id);
        Task<Location> AddAsync(Location location);
        Task<Location> UpdateAsync(Location location);
        Task DeleteAsync(int id);
    }
}
