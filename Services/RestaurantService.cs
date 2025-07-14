using RestaurantAPI.Models;
using RestaurantAPI.Repositories.Interfaces;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Services
{
    public class RestaurantService : IRestaurantService
    {
        private readonly IRestaurantRepository _restaurantRepository;

        public RestaurantService(IRestaurantRepository restaurantRepository)
        {
            _restaurantRepository = restaurantRepository;
        }

        public async Task<IEnumerable<Restaurant>> GetAllAsync()
        {
            return await _restaurantRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Restaurant>> GetAllWithLocationsAsync()
        {
            return await _restaurantRepository.GetAllWithLocationAsync();
        }

        public async Task<Restaurant?> GetByIdAsync(int id)
        {
            return await _restaurantRepository.GetByIdAsync(id);
        }

        public async Task<Restaurant> AddAsync(Restaurant restaurant)
        {
            await _restaurantRepository.AddAsync(restaurant);
            await _restaurantRepository.SaveAsync();
            return restaurant;
        }

        public async Task<Restaurant> UpdateAsync(Restaurant restaurant)
        {
            _restaurantRepository.Update(restaurant);
            await _restaurantRepository.SaveAsync();
            return restaurant;
        }

        public async Task DeleteAsync(int id)
        {
            var restaurant = await _restaurantRepository.GetByIdAsync(id);
            if (restaurant != null)
            {
                _restaurantRepository.Delete(restaurant);
                await _restaurantRepository.SaveAsync();
            }
        }
    }
}
