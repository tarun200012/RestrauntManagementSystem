using RestaurantAPI.Models;
using RestaurantAPI.Repositories.Interfaces;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Services
{
    public class LocationService : ILocationService
    {
        private readonly ILocationRepository _locationRepository;

        public LocationService(ILocationRepository locationRepository)
        {
            _locationRepository = locationRepository;
        }

        public async Task<IEnumerable<Location>> GetAllAsync()
        {
            return await _locationRepository.GetAllAsync();
        }

        public async Task<Location?> GetByIdAsync(int id)
        {
            return await _locationRepository.GetByIdAsync(id);
        }

        public async Task<Location> AddAsync(Location location)
        {
            await _locationRepository.AddAsync(location);
            await _locationRepository.SaveAsync();
            return location;
        }

        public async Task<Location> UpdateAsync(Location location)
        {
            _locationRepository.Update(location);
            await _locationRepository.SaveAsync();
            return location;
        }

        public async Task DeleteAsync(int id)
        {
            var location = await _locationRepository.GetByIdAsync(id);
            if (location != null)
            {
                _locationRepository.Delete(location);
                await _locationRepository.SaveAsync();
            }
        }
    }
}
