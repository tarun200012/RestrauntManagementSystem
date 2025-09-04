using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog; // ✅ Add NLog
using RestaurantAPI.Dtos;
using RestaurantAPI.Models;
using RestaurantAPI.Models.Common;
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
        private readonly IOrderService _orderService; 


        public RestaurantWithLocationController(
            IRestaurantService restaurantService,
            ILocationService locationService,
            IMapper mapper, IOrderService orderService
            )
        {
            _restaurantService = restaurantService;
            _locationService = locationService;
            _mapper = mapper;
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRestaurantWithLocation([FromBody] RestaurantWithLocationDto dto)
        {

            _logger.Info("Creating new restaurant with location");
            var created = await _restaurantService.CreateWithLocationAsync(dto);
            return Ok(new { message = "Restaurant created", created });


        }

        [Authorize(Roles = "Admin,Customer")]
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

        [HttpPost("paginated")]
        public async Task<IActionResult> GetPaginated([FromBody] PaginatedRequest request)
        {
            var result = await _restaurantService.GetPaginatedAsync(request);
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

        [HttpGet("menu")]
        public async Task<IActionResult> GetMenuByRestaurantId([FromQuery] int id)
        {
            var menuItems = await _restaurantService.GetMenuByRestaurantIdAsync(id);

            if (menuItems == null || !menuItems.Any())
                throw new NotFoundException($"No menu items found for restaurant ID {id}");

            return Ok(menuItems);
        }

        [HttpPost("schedule_order/{restaurantId}/customer/{customerId}")]
        public async Task<IActionResult> ScheduleOrder(int restaurantId, int customerId, [FromBody] ScheduleOrderRequest request)
        {
            try
            {
                var (isSuccess, message) = await _orderService.ScheduleOrderAsync(restaurantId, customerId, request);
                if (!isSuccess)
                    return BadRequest(new { success = false, message });

                return Ok(new { success = true, message });
            }
            catch (Exception ex)
            {
                _logger.Error("Error scheduling order");
                throw;
            }
        }

        [HttpGet("orders/restaurant/{restaurantId}/customer/{customerId}")]
        public async Task<IActionResult> GetOrdersForCustomerAtRestaurant(int restaurantId, int customerId)
        {
            try
            {
                var orders = await _orderService.GetOrdersForCustomerAtRestaurantAsync(restaurantId, customerId);
                    return Ok(new { success = true, data = orders });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error fetching orders for RestaurantId {restaurantId} and CustomerId {customerId}");
                throw;
            }
        }




    }
}
