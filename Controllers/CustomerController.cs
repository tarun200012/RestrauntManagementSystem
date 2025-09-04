using Microsoft.AspNetCore.Mvc;
using NLog;
using RestaurantAPI.Dtos;
using RestaurantAPI.DTOs.Common;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ICustomerService _service;

        public CustomerController(ICustomerService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.Info("Fetching all customers");
            var customers = await _service.GetAllAsync();
            return Ok(customers);
        }

        [HttpGet("paginated")]
        public async Task<ActionResult<PaginatedResponse<CustomerDto>>> SearchCustomers([FromQuery] string search = "",[FromQuery] int pageNumber = 1,[FromQuery] int pageSize = 50)
        {
            _logger.Info($"Searching customers with query: {search}, page: {pageNumber}, size: {pageSize}");
            var customers = await _service.GetPaginatedAsync(search, pageNumber, pageSize);
            return Ok(customers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.Info($"Fetching customer with ID {id}");
            var customer = await _service.GetByIdAsync(id);
            if (customer == null)
                return NotFound($"Customer with ID {id} not found.");

            return Ok(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CustomerDto dto)
        {
            _logger.Info("Creating new customer");
            var created = await _service.AddAsync(dto);
            return Ok(new { message = "Customer created successfully", created });
        }
    }
}
