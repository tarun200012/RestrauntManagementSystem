using Microsoft.AspNetCore.Mvc;
using NLog; // ✅ Add NLog
using RestaurantAPI.Dtos;
using RestaurantAPI.Models;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RestaurantWithLocationController : ControllerBase
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger(); // ✅ Logger instance
        private readonly IRestaurantService _restaurantService;
        private readonly ILocationService _locationService;

        public RestaurantWithLocationController(
            IRestaurantService restaurantService,
            ILocationService locationService)
        {
            _restaurantService = restaurantService;
            _locationService = locationService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRestaurantWithLocation([FromBody] RestaurantWithLocationDto dto)
        {
        
            _logger.Info("Creating new restaurant with location");

            var location = new Location
            {
                City = dto.Location.City,
                State = dto.Location.State,
                Country = dto.Location.Country,
                Address = dto.Location.Address
            };

            var createdLocation = await _locationService.AddAsync(location);

            var restaurant = new Restaurant
            {
                Name = dto.Name,
                Description = dto.Description,
                Mobile = dto.Mobile,
                Email = dto.Email,
                LocationId = createdLocation.Id
            };

            await _restaurantService.AddAsync(restaurant);

            _logger.Info($"Created restaurant '{restaurant.Name}' with LocationId {restaurant.LocationId}");

            return Ok(new { message = "Restaurant created", restaurant });

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRestaurantWithLocation(int id, [FromBody] RestaurantWithLocationDto dto)
        {
               
                _logger.Info($"Updating restaurant and location for restaurant ID: {id}");

                var restaurant = await _restaurantService.GetByIdAsync(id);
                if (restaurant == null)
                    throw new NotFoundException($"Restaurant with ID {id} not found.");

                Location location;

                if (restaurant.LocationId.HasValue)
                {
                    location = await _locationService.GetByIdAsync(restaurant.LocationId.Value);
                    if (location == null)
                        throw new NotFoundException($"Associated location not found for restaurant ID {id}.");
                }
                else
                {
                    location = await _locationService.AddAsync(new Location());
                    restaurant.LocationId = location.Id;
                }

                location.City = dto.Location.City;
                location.State = dto.Location.State;
                location.Country = dto.Location.Country;
                location.Address = dto.Location.Address;
                await _locationService.UpdateAsync(location);

                restaurant.Name = dto.Name;
                restaurant.Description = dto.Description;
                restaurant.Mobile = dto.Mobile;
                restaurant.Email = dto.Email;
                await _restaurantService.UpdateAsync(restaurant);

                _logger.Info($"Updated restaurant ID {id} and location ID {restaurant.LocationId}");

                return Ok(new { message = "Restaurant and Location updated", restaurant });
          
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
                _logger.Info("Fetching all restaurants with locations");

                var restaurants = await _restaurantService.GetAllWithLocationsAsync();
                var result = new List<RestaurantWithLocationDto>();

                foreach (var r in restaurants)
                {
                    result.Add(new RestaurantWithLocationDto
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Description = r.Description,
                        Mobile = r.Mobile,
                        Email = r.Email,
                        Location = r.Location == null ? null : new LocationDto
                        {
                            Id = r.Location.Id,
                            City = r.Location.City,
                            State = r.Location.State,
                            Country = r.Location.Country,
                            Address = r.Location.Address
                        }
                    });
                }

                return Ok(result);
            }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
           
                _logger.Info($"Fetching restaurant by ID {id}");

                var r = await _restaurantService.GetByIdAsync(id);
                if (r == null)
                    throw new NotFoundException($"Restaurant with ID {id} not found.");

                var dto = new RestaurantWithLocationDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    Mobile = r.Mobile,
                    Email = r.Email,
                    Location = r.Location != null ? new LocationDto
                    {
                        City = r.Location.City,
                        State = r.Location.State,
                        Country = r.Location.Country,
                        Address = r.Location.Address
                    } : new LocationDto
                    {
                        City = "Not in Service"
                    }
                };

                return Ok(dto);
            
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRestaurant(int id)
        {
            _logger.Info($"Deleting restaurant with ID {id}");

            var restaurant = await _restaurantService.GetByIdAsync(id);
            if (restaurant == null)
                throw new NotFoundException($"Restaurant with ID {id} not found.");

            var locationId = restaurant.LocationId;

            await _restaurantService.DeleteAsync(id);

            if (locationId != null)
                await _locationService.DeleteAsync(locationId.Value);

            _logger.Info($"Deleted restaurant with ID {id} and associated location {locationId}");

            return NoContent();
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> CreateRestaurantsInBulk([FromBody] DTOs.BulkRestaurantWithLocationDto dto)
        {
            _logger.Info("Bulk creating restaurants with locations");

            var createdRestaurants = new List<Restaurant>();

            foreach (var item in dto.Restaurants)
            {
                var location = new Location
                {
                    City = item.Location.City,
                    State = item.Location.State,
                    Country = item.Location.Country,
                    Address = item.Location.Address
                };

                var createdLocation = await _locationService.AddAsync(location);

                var restaurant = new Restaurant
                {
                    Name = item.Name,
                    Description = item.Description,
                    Mobile = item.Mobile,
                    Email = item.Email,
                    LocationId = createdLocation.Id
                };

                await _restaurantService.AddAsync(restaurant);

                createdRestaurants.Add(restaurant);
                _logger.Info($"Created restaurant '{restaurant.Name}' with LocationId {restaurant.LocationId}");
            }

            return Ok(new { message = "Bulk insert successful", createdRestaurants });
        }

    }
}
