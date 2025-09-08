using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NLog;
using RestaurantAPI.Data;
using RestaurantAPI.Dtos;
using RestaurantAPI.DTOs;
using RestaurantAPI.DTOs.Common;
using RestaurantAPI.DTOs.RestaurantAPI.DTOs.Coupon;
using RestaurantAPI.Models;
using RestaurantAPI.Models.Common;
using RestaurantAPI.Repositories.Interfaces;
using RestaurantAPI.Services.Interfaces;
using System.Collections.Generic;
using System.Net.WebSockets;

namespace RestaurantAPI.Services
{
    public class CustomerSpendDto
    {
        public int CustomerId { get; set; }
        public decimal TotalSpent { get; set; }
        public int TotalOrders { get; set; }
    }
    public class CouponService : ICouponService
    {
        private readonly ICouponRepository _couponRepository;
        private readonly ApplicationDbContext _context;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IMapper _mapper;


        public CouponService(ICouponRepository couponRepository,ApplicationDbContext context,IMapper mapper)
        {
            _couponRepository = couponRepository;
            _context = context;
            _mapper = mapper;
        }

        private DiscountType ParseDiscountType(string discountType)
        {
            if (Enum.TryParse<DiscountType>(discountType, true, out var result))
                return result;

            throw new ArgumentException($"Invalid discount type: {discountType}");
        }

        public async Task<IEnumerable<Coupon>> GetAllCouponsAsync()
        {
            return await _couponRepository.GetAllCouponsWithDetailsAsync();
        }

        public async Task<Coupon?> GetCouponByIdAsync(int id)
        {
            return await _couponRepository.GetByIdAsync(id);
        }

        public async Task CreateAutomaticCoupons(int? restaurantId)
        {
            var today = DateTime.Today;
            var firstDateOfLastMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
            var lastDateOfLastMonth = firstDateOfLastMonth.AddMonths(1);

            // Step 1: Build aggregate query without Include
            var query = _context.Orders
                .Where(o => o.ScheduledAt >= firstDateOfLastMonth &&
                            o.ScheduledAt < lastDateOfLastMonth &&
                            o.IsConfirmed);

            if (restaurantId.HasValue)
            {
                query = query.Where(o => o.RestaurantId == restaurantId.Value);
            }

            // Step 2: Aggregate directly
            var topCustomers = await query
                .SelectMany(o => o.OrderItems.Select(oi => new
                {
                    o.CustomerId,
                    Amount = oi.MenuItem.Price * oi.Quantity
                }))
                .GroupBy(x => x.CustomerId)
                .Select(g => new CustomerSpendDto
                {
                    CustomerId = g.Key,
                    TotalSpent = g.Sum(x => x.Amount),
                    TotalOrders = g.Count()
                })
                .OrderByDescending(x => x.TotalSpent)
                .ThenByDescending(x => x.TotalOrders)
                .Take(10)
                .ToListAsync()
                .ConfigureAwait(false);

            // Step 3: Prepare coupons in memory
            var couponsToInsert = topCustomers
                .Where(c => c.TotalSpent >= 33000)
                .Select(c => new Coupon
                {
                    Name = "Test Reward",
                    DiscountType = DiscountType.Percent,
                    DiscountValue = 10,
                    StartDate = today,
                    EndDate = today.AddMonths(1),
                    MinOrderAmount = 100,
                    IsActive = true,
                    CouponCustomers = new List<CouponCustomer>
                    {
                new CouponCustomer { CustomerId = c.CustomerId }
                    },
                    CouponRestaurants = restaurantId.HasValue
                        ? new List<CouponRestaurant>
                        {
                    new CouponRestaurant { RestaurantId = restaurantId.Value }
                        }
                        : new List<CouponRestaurant>()
                })
                .ToList();

            // Step 4: Bulk insert in single SaveChanges
            if (couponsToInsert.Any())
            {
                await _couponRepository.AddRangeAsync(couponsToInsert);
                await _couponRepository.SaveAsync();
            }
        }




        public async Task<Coupon> CreateCouponAsync(CreateCouponRequestDto dto)
        {
            var coupon = new Coupon
            {
                Name = dto.Name,
                DiscountType = ParseDiscountType(dto.DiscountType),
                DiscountValue = dto.DiscountValue,
                StartDate = dto.StartDate.ToLocalTime(),
                EndDate = dto.EndDate.ToLocalTime(),
                MinOrderAmount = dto.MinOrderAmount,
                IsActive = dto.IsActive,
                // populate navigation collection
                CouponCustomers = dto.CustomerIds.Select(custId => new CouponCustomer
                {
                    CustomerId = custId
                }).ToList(),

                CouponRestaurants = dto.RestaurantIds.Select(restId => new CouponRestaurant {
                    RestaurantId = restId
                }).ToList(),
            };

            await _couponRepository.AddAsync(coupon);
            await _couponRepository.SaveAsync();

            return coupon;
        }

        public async Task<Coupon?> UpdateCouponAsync(int id, CreateCouponRequestDto dto)
        {
            var existing = await _couponRepository.GetByIdAsync(id);
            if (existing == null) return null;

            existing.Name = dto.Name;
            existing.DiscountType = ParseDiscountType(dto.DiscountType);
            existing.DiscountValue = dto.DiscountValue;
            existing.StartDate = dto.StartDate.ToLocalTime();
            existing.EndDate = dto.EndDate.ToLocalTime();
            existing.MinOrderAmount = dto.MinOrderAmount;
            existing.IsActive = dto.IsActive;

            _couponRepository.Update(existing);
            await _couponRepository.SaveAsync();

            return existing;
        }

        public async Task<bool> DeleteCouponAsync(int id)
        {
            var existing = await _couponRepository.GetByIdAsync(id);
            if (existing == null) return false;

            _couponRepository.Delete(existing);
            await _couponRepository.SaveAsync();

            return true;
        }

        public async Task<List<CouponResponseDto>> GetCouponsForRestaurantAndCustomerAsync(int? restaurantId, int? customerId)
        {
            // Step 1: Fetch coupons including relations
            var query = _context.Coupons
                .Include(c => c.CouponRestaurants)
                .Include(c => c.CouponCustomers)
                .Where(c => c.IsActive) // Only active coupons
                .AsQueryable();

            // Step 2: Filter by restaurant
            if (restaurantId.HasValue)
            {
                query = query.Where(c =>
                    !c.CouponRestaurants.Any() || // Valid for all restaurants
                    c.CouponRestaurants.Any(cr => cr.RestaurantId == restaurantId.Value)
                );
            }

            // Step 3: Filter by customer
            if (customerId.HasValue)
            {
                query = query.Where(c =>
                    !c.CouponCustomers.Any() || // Valid for all customers
                    c.CouponCustomers.Any(cc => cc.CustomerId == customerId.Value)
                );
            }

            // Step 4: Fetch data
            var coupons = await query.ToListAsync();

            // Step 5: Map to DTO
            var mapped = _mapper.Map<List<CouponResponseDto>>(coupons);

            return mapped;
        }


        public async Task<PaginatedResponse<CouponResponseDto>> GetPaginatedAsync(PaginatedRequest request)
        {
            _logger.Info("Fetching paginated restaurants with filters & sorting");

            var query = _context.Coupons
                .Include(c => c.CouponRestaurants)
                .Include(c => c.CouponCustomers)
                .Where(c => c.IsActive) // Ignore deleted
                .AsQueryable();

            // Apply Filters
            foreach (var filter in request.Filters)
            {
                var val = filter.Value?.ToLower();
                if (string.IsNullOrWhiteSpace(val)) continue;

                switch (filter.Field.ToLower())
                {
                    case "name":
                        query = query.Where(r => r.Name.Contains(val));
                        break;
                   
                }
            }

            // Apply Sorting
            bool isFirst = true;
            foreach (var sort in request.Sort)
            {
                var isAsc = sort.Direction.ToLower() == "asc";

                if (isFirst)
                {
                    query = sort.Field.ToLower() switch
                    {
                        "name" => isAsc ? query.OrderBy(r => r.Name) : query.OrderByDescending(r => r.Name),
                        _ => query
                    };
                    isFirst = false;
                }
                else
                {
                    query = sort.Field.ToLower() switch
                    {
                        "name" => isAsc ? ((IOrderedQueryable<Coupon>)query).ThenBy(r => r.Name) : ((IOrderedQueryable<Coupon>)query).ThenByDescending(r => r.Name),
                        _ => query
                    };
                }
            }

            var totalCount = await query.CountAsync();

            var pagedData = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var mappedData = _mapper.Map<List<CouponResponseDto>>(pagedData);

            return new PaginatedResponse<CouponResponseDto>
            {
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = mappedData
            };
        }

    }
}
