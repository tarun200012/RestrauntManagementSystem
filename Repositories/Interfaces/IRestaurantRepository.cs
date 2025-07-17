using RestaurantAPI.Models;

namespace RestaurantAPI.Repositories.Interfaces
{
    public interface IRestaurantRepository : IRepository<Restaurant>
    {
        Task<IEnumerable<Restaurant>> GetAllWithLocationAsync();

        // IRestaurantRepository.cs
        Task AddBulkAsync(List<Restaurant> restaurants);

    }
}
