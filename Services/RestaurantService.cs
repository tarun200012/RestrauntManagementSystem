using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NLog;
using RestaurantAPI.Data;
using RestaurantAPI.Dtos;
using RestaurantAPI.Models;
using RestaurantAPI.Repositories;
using RestaurantAPI.Repositories.Interfaces;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Services
{
    public class RestaurantService : IRestaurantService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly ILocationService _locationService;
        private readonly ILocationRepository _locationRepository;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public RestaurantService(IRestaurantRepository restaurantRepository, IMapper mapper, ILocationService locationService, ILocationRepository locationRepository, ApplicationDbContext context)
        {
            _restaurantRepository = restaurantRepository;
            _mapper = mapper;
            _locationService = locationService;
            _locationRepository = locationRepository;
            _context = context;
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


        public async Task<RestaurantWithLocationDto> CreateWithLocationAsync(RestaurantWithLocationDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var restaurant = _mapper.Map<Restaurant>(dto);
                var location = _mapper.Map<Location>(dto.Location);

                await _locationRepository.AddAsync(location);
                await _locationRepository.SaveAsync(); // Ensure the location is saved to get its ID  

                restaurant.LocationId = location.Id;

                await _restaurantRepository.AddAsync(restaurant);

                await _context.SaveChangesAsync(); // Save all changes at once  

                await transaction.CommitAsync();

                restaurant.Location = location;
                return _mapper.Map<RestaurantWithLocationDto>(restaurant);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task SoftDeleteAsync(int id)
        {
            var restaurant = await _restaurantRepository.GetByIdAsync(id);
            if (restaurant == null)
                throw new NotFoundException($"Restaurant with ID {id} not found.");

            restaurant.IsDeleted = true;
            await _restaurantRepository.SaveAsync();
        }

        public async Task<RestaurantWithLocationDto> UpdateWithLocationAsync(int id, RestaurantWithLocationDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var restaurant = await _restaurantRepository.GetByIdAsync(id);
                if (restaurant == null)
                    throw new NotFoundException($"Restaurant with ID {id} not found.");

                Location location;

                if (restaurant.LocationId.HasValue)
                {
                    location = await _locationRepository.GetByIdAsync(restaurant.LocationId.Value);
                    if (location == null)
                        throw new NotFoundException($"Associated location not found for restaurant ID {id}.");
                }
                else
                {
                    location = new Location();
                    await _locationRepository.AddAsync(location);
                    await _locationRepository.SaveAsync(); // Make sure to save to get the ID
                    restaurant.LocationId = location.Id;
                }

                dto.Location.Id = location.Id; 
                _mapper.Map(dto.Location, location); 

                _locationRepository.Update(location);  // <-- Await here
                await _locationRepository.SaveAsync();            // <-- Persist location update

                dto.Id = id;
                _mapper.Map(dto, restaurant);
                _restaurantRepository.Update(restaurant); // <-- Await here
                await _restaurantRepository.SaveAsync();             // <-- Persist restaurant update

                await transaction.CommitAsync();

                return _mapper.Map<RestaurantWithLocationDto>(restaurant); // or just return a success message if you prefer
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task<List<RestaurantWithLocationDto>> BulkCreateWithLocationAsync(List<RestaurantWithLocationDto> dtoList)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var createdRestaurants = new List<Restaurant>();

                foreach (var dto in dtoList)
                {
                    var location = _mapper.Map<Location>(dto.Location);
                    var createdLocation = await _locationService.AddAsync(location);

                    var restaurant = _mapper.Map<Restaurant>(dto);
                    restaurant.LocationId = createdLocation.Id;

                    await _restaurantRepository.AddAsync(restaurant);
                    createdRestaurants.Add(restaurant);
                }

                await _restaurantRepository.SaveAsync();
                await transaction.CommitAsync();

                return _mapper.Map<List<RestaurantWithLocationDto>>(createdRestaurants);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<MenuItem>> GetMenuByRestaurantIdAsync(int restaurantId)
        {
            try
            {
                _logger.Info($"Getting menu items for restaurant ID: {restaurantId}");
                return await _restaurantRepository.GetMenuItemsByRestaurantIdAsync(restaurantId);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while fetching menu items for restaurant ID: {restaurantId}", ex);
                throw;
            }
        }

    }
}
