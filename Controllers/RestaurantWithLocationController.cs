using Microsoft.AspNetCore.Mvc;
using RestaurantAPI.Dtos;
using RestaurantAPI.Models;
using RestaurantAPI.Services;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RestaurantWithLocationController : ControllerBase
    {
        private readonly IRestaurantService _restaurantService;
        private readonly ILocationService _locationService;

        public RestaurantWithLocationController(
            IRestaurantService restaurantService,
            ILocationService locationService)
        {
            _restaurantService = restaurantService;
            _locationService = locationService;
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> CreateRestaurantWithLocation([FromBody] RestaurantWithLocationDto dto)
        {
            var location = new Location
            {
                City = dto.Location.City,
                State = dto.Location.State,
                Country = dto.Location.Country,
                Address = dto.Location.Address
            };

            var createdLocation = await _locationService.AddAsync(location); // This now returns Location

            var restaurant = new Restaurant
            {
                Name = dto.Name,
                Description = dto.Description,
                Mobile = dto.Mobile,
                Email = dto.Email,
                LocationId = createdLocation.Id
            };

            await _restaurantService.AddAsync(restaurant);
            return Ok(new { message = "Restaurant created", restaurant });
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRestaurantWithLocation(int id, [FromBody] RestaurantWithLocationDto dto)
        {
            var restaurant = await _restaurantService.GetByIdAsync(id);
            if (restaurant == null)
                return NotFound("Restaurant not found.");

            Location location;

            if (restaurant.LocationId.HasValue)
            {
                location = await _locationService.GetByIdAsync(restaurant.LocationId.Value);
                if (location == null)
                    return NotFound("Associated location not found.");
            }
            else
            {
                location = new Location();
                location = await _locationService.AddAsync(location);
                restaurant.LocationId = location.Id;
            }

            // Update Location
            location.City = dto.Location.City;
            location.State = dto.Location.State;
            location.Country = dto.Location.Country;
            location.Address = dto.Location.Address;
            await _locationService.UpdateAsync(location);

            // Update Restaurant
            restaurant.Name = dto.Name;
            restaurant.Description = dto.Description;
            restaurant.Mobile = dto.Mobile;
            restaurant.Email = dto.Email;
            await _restaurantService.UpdateAsync(restaurant);

            return Ok(new { message = "Restaurant and Location updated", restaurant});
        }



        // ✅ GET ALL

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
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


        // ✅ GET BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var r = await _restaurantService.GetByIdAsync(id);
            if (r == null) return NotFound("Restaurant not found.");

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

    }
}
