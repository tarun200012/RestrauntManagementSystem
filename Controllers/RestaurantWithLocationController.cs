using AutoMapper;
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
        private readonly IMapper _mapper;


        public RestaurantWithLocationController(
            IRestaurantService restaurantService,
            ILocationService locationService,
            IMapper mapper
            )
        {
            _restaurantService = restaurantService;
            _locationService = locationService;
            _mapper = mapper;

        }

        [HttpPost]
        public async Task<IActionResult> CreateRestaurantWithLocation([FromBody] RestaurantWithLocationDto dto)
        {

            _logger.Info("Creating new restaurant with location");
            var created = await _restaurantService.CreateWithLocationAsync(dto);
            return Ok(new { message = "Restaurant created", created });


        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRestaurantWithLocation(int id, [FromBody] RestaurantWithLocationDto dto)
        {
            _logger.Info($"Updating restaurant and location for restaurant ID: {id}");

          var restaurant =   await _restaurantService.UpdateWithLocationAsync(id, dto);

            _logger.Info($"Updated restaurant ID {id} and its location");

            return Ok(new { message = "Restaurant and Location updated successfully.", restaurant });
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.Info("Fetching all restaurants with locations");
            var restaurants = await _restaurantService.GetAllWithLocationsAsync();
            var result = _mapper.Map<List<RestaurantWithLocationDto>>(restaurants);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {

            _logger.Info($"Fetching restaurant by ID {id}");
            var restaurant = await _restaurantService.GetByIdAsync(id);
            if (restaurant == null)
                throw new NotFoundException($"Restaurant with ID {id} not found.");

            var dto = _mapper.Map<RestaurantWithLocationDto>(restaurant);
            return Ok(dto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRestaurant(int id)
        {
            _logger.Info($"Soft deleting restaurant with ID {id}");
            var restaurant = await _restaurantService.GetByIdAsync(id);
            if (restaurant == null)
                throw new NotFoundException($"Restaurant with ID {id} not found.");

            await _restaurantService.SoftDeleteAsync(id);
            return NoContent();
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> CreateRestaurantsInBulk([FromBody] DTOs.BulkRestaurantWithLocationDto dto)
        {
            _logger.Info("Bulk creating restaurants with locations");

            var resultDtos = await _restaurantService.BulkCreateWithLocationAsync(dto.Restaurants);
            return Ok(new { message = "Bulk insert successful", resultDtos });
        }

    }
}
