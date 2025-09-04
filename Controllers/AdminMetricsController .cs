using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Controllers
{
    [ApiController]
    [Route("api/admin/metrics")]
    public class AdminMetricsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AdminMetricsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview([FromQuery] int? restaurantId)
            => Ok(await _analyticsService.GetOrdersOverviewAsync(restaurantId));

        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenue(
     [FromQuery] int? restaurantId,
     [FromQuery] int pageNumber = 1,
     [FromQuery] int pageSize = 10,
     [FromQuery] int? year = null,
     [FromQuery] int? month = null,
     [FromQuery] bool includeProfitability = false)
        {
            var result = await _analyticsService.GetRevenueAsync(
                restaurantId,
                pageNumber,
                pageSize,
                year,
                month,
                includeProfitability
            );

            return Ok(result);
        }


        [HttpGet("top-items")]
        public async Task<IActionResult> GetTopItems([FromQuery] int? restaurantId, int pageNumber = 1, int pageSize = 10)
            => Ok(await _analyticsService.GetTopItemsAsync(restaurantId, pageNumber, pageSize));

        [HttpGet("top-customers")]
        public async Task<IActionResult> GetTopCustomers([FromQuery] int? restaurantId, int pageNumber = 1, int pageSize = 10)
            => Ok(await _analyticsService.GetTopCustomersAsync(restaurantId, pageNumber, pageSize));

        [HttpGet("peak-hours")]
        public async Task<IActionResult> GetPeakHours([FromQuery] int? restaurantId, int pageNumber = 1, int pageSize = 10)
            => Ok(await _analyticsService.GetPeakHoursAsync(restaurantId, pageNumber, pageSize));
    }

}
