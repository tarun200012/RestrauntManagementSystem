using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using RestaurantAPI.DTOs;
using RestaurantAPI.DTOs.RestaurantAPI.DTOs.Coupon;
using RestaurantAPI.Models;
using RestaurantAPI.Models.Common;
using RestaurantAPI.Services;
using RestaurantAPI.Services.Interfaces;
using System.Threading.Tasks;

namespace RestaurantAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CouponController : ControllerBase
    {
        private readonly ICouponService _couponService;
        private readonly IMapper _mapper;

        public CouponController(ICouponService couponService,IMapper mapper)
        {
            _couponService = couponService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var coupons = await _couponService.GetAllCouponsAsync();
            var dtoList = _mapper.Map<IEnumerable<CouponResponseDto>>(coupons);
            return Ok(dtoList);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var coupon = await _couponService.GetCouponByIdAsync(id);
            if (coupon == null) return NotFound();

            var dto = _mapper.Map<CouponResponseDto>(coupon);
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCouponRequestDto dto)
        {
            var coupon = await _couponService.CreateCouponAsync(dto);
            if (coupon == null) return NotFound();

            var result = _mapper.Map<CouponResponseDto>(coupon);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateCouponRequestDto dto)
        {
            var updated = await _couponService.UpdateCouponAsync(id, dto);
            if (updated == null) return NotFound();
            var result = _mapper.Map<CouponResponseDto>(updated);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _couponService.DeleteCouponAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpGet("test-job")]
        public IActionResult TriggerCouponEngine([FromQuery] int? restaurantId)
        {
            var jobId1 = BackgroundJob.Enqueue(()=> _couponService.CreateAutomaticCoupons(restaurantId));
            
            return Ok(jobId1);
        }

        [HttpPost("paginated")]
        public async Task<IActionResult> GetPaginated([FromBody] PaginatedRequest request)
        {
            var result = await _couponService.GetPaginatedAsync(request);
            return Ok(result);
        }

        [HttpGet("available")]

        public async Task<IActionResult> GetCouponsForRestaurantAndCustomer([FromQuery] int? restaurantId, [FromQuery] int? customerId)
        {
            var coupons = await _couponService.GetCouponsForRestaurantAndCustomerAsync(restaurantId, customerId);
            return Ok(coupons);
        }

    }
}
