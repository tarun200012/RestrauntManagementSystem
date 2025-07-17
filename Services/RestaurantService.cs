using NLog;
using RestaurantAPI.Models;
using RestaurantAPI.Repositories.Interfaces;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Services
{
    public class RestaurantService : IRestaurantService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IRestaurantRepository _restaurantRepository;

        public RestaurantService(IRestaurantRepository restaurantRepository)
        {
            _restaurantRepository = restaurantRepository;
        }

        public async Task<IEnumerable<Restaurant>> GetAllAsync()
        {
            try
            {
                _logger.Debug("Fetching all restaurants (without locations)");
                return await _restaurantRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching all restaurants");
                throw;
            }
        }

        public async Task<IEnumerable<Restaurant>> GetAllWithLocationsAsync()
        {
            try
            {
                _logger.Debug("Fetching all restaurants with their locations");
                return await _restaurantRepository.GetAllWithLocationAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching restaurants with locations");
                throw;
            }
        }

        public async Task<Restaurant?> GetByIdAsync(int id)
        {
            try
            {
                _logger.Debug($"Fetching restaurant by ID: {id}");
                return await _restaurantRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error fetching restaurant with ID {id}");
                throw;
            }
        }

        public async Task<Restaurant> AddAsync(Restaurant restaurant)
        {
            try
            {
                _logger.Info($"Adding restaurant: {restaurant.Name}");
                await _restaurantRepository.AddAsync(restaurant);
                await _restaurantRepository.SaveAsync();
                _logger.Info($"Restaurant '{restaurant.Name}' added successfully with ID {restaurant.Id}");
                return restaurant;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error adding restaurant");
                throw;
            }
        }

        public async Task<Restaurant> UpdateAsync(Restaurant restaurant)
        {
            try
            {
                _logger.Info($"Updating restaurant with ID: {restaurant.Id}");
                _restaurantRepository.Update(restaurant);
                await _restaurantRepository.SaveAsync();
                _logger.Info($"Restaurant with ID {restaurant.Id} updated successfully");
                return restaurant;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error updating restaurant with ID {restaurant.Id}");
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                _logger.Info($"Attempting to delete restaurant with ID: {id}");
                var restaurant = await _restaurantRepository.GetByIdAsync(id);
                if (restaurant != null)
                {
                    _restaurantRepository.Delete(restaurant);
                    await _restaurantRepository.SaveAsync();
                    _logger.Info($"Restaurant with ID {id} deleted");
                }
                else
                {
                    _logger.Warn($"Restaurant with ID {id} not found, nothing to delete");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error deleting restaurant with ID {id}");
                throw;
            }
        }
        public async Task<IEnumerable<Restaurant>> AddBulkAsync(List<Restaurant> restaurants)
        {
            try
            {
                foreach (var r in restaurants)
                await _restaurantRepository.AddAsync(r);

            await _restaurantRepository.SaveAsync();
            return restaurants;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error adding restaurants list");
                throw;
            }
        }

    }
}
