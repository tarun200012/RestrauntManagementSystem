using NLog;
using RestaurantAPI.Models;
using RestaurantAPI.Repositories.Interfaces;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Services
{
    public class LocationService : ILocationService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ILocationRepository _locationRepository;

        public LocationService(ILocationRepository locationRepository)
        {
            _locationRepository = locationRepository;
        }

        public async Task<IEnumerable<Location>> GetAllAsync()
        {
            try
            {
                _logger.Debug("Fetching all locations");
                return await _locationRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching all locations");
                throw;
            }
          
        }

        public async Task<Location?> GetByIdAsync(int id)
        {
            
            try
            {
                _logger.Info($"Fetching location by ID: {id}");
                var location = await _locationRepository.GetByIdAsync(id);
                if (location == null)
                {
                    _logger.Warn($"Location with ID {id} not found");
                    throw new NotFoundException($"Location with ID {id} not found");
                }
                return location;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error fetching location with ID {id}");
                throw;
            }
        }

        public async Task<Location> AddAsync(Location location)
        {
            
            try
            {
                _logger.Info($"Adding location: {location.Address}, {location.City}");
                await _locationRepository.AddAsync(location);
                await _locationRepository.SaveAsync();
                _logger.Info($"Location added successfully with ID {location.Id}");
                return location;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error adding location");
                throw;
            }
        }

        public async Task<Location> UpdateAsync(Location location)
        {
            try
            {
                _logger.Info($"Updating location with ID: {location.Id}");
                _locationRepository.Update(location);
                await _locationRepository.SaveAsync();
                _logger.Info($"Location with ID {location.Id} updated successfully");
                return location;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error updating location with ID {location.Id}");
                throw;
            }
           
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                _logger.Info($"Attempting to delete location with ID: {id}");
                var location = await _locationRepository.GetByIdAsync(id);
                if (location != null)
                {
                    _locationRepository.Delete(location);
                    await _locationRepository.SaveAsync();
                    _logger.Info($"Location with ID {id} deleted");
                }
                else
                {
                    _logger.Warn($"Location with ID {id} not found, nothing to delete");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error deleting location with ID {id}");
                throw;
            }
        }
    }
}
