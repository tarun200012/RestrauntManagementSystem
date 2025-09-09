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
            var monthName = today.ToString("MMMM"); // e.g., "September"

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
                    o.Id ,
                    Amount = oi.MenuItem.Price * oi.Quantity
                }))
                .GroupBy(x => x.CustomerId)
                .Select(g => new CustomerSpendDto
                {
                    CustomerId = g.Key,
                    TotalSpent = g.Sum(x => x.Amount),
                    TotalOrders = g.Select(x => x.Id).Distinct().Count()
                })
                .OrderByDescending(x => x.TotalSpent)
                .ThenByDescending(x => x.TotalOrders)
                .ToListAsync()
                .ConfigureAwait(false);

            // Step 3: Collect eligible customers
            var flatCouponCustomers = topCustomers
                .Where(c => c.TotalSpent >= 20000)
                .Select(c => new CouponCustomer { CustomerId = c.CustomerId })
                .ToList();

            var percentCouponCustomers = topCustomers
                .Where(c => c.TotalSpent >= 30000)
                .Select(c => new CouponCustomer { CustomerId = c.CustomerId })
                .ToList();

            var bogoCouponCustomers = topCustomers
                .Where(c => c.TotalOrders >= 6)
                .Select(c => new CouponCustomer { CustomerId = c.CustomerId })
                .ToList();

            var couponsToInsert = new List<Coupon>();
            // Step 3: Prepare coupons in memory
            if (flatCouponCustomers.Any())
            {
                couponsToInsert.Add(new Coupon
                {
                    Name = $"{monthName}Flat100",
                    DiscountType = DiscountType.Flat,
                    DiscountValue = 100,
                    StartDate = today,
                    EndDate = today.AddMonths(1),
                    MinOrderAmount = 500,
                    IsActive = true,
                    CouponCustomers = flatCouponCustomers,
                    CouponRestaurants = restaurantId.HasValue
                        ? new List<CouponRestaurant>
                        {
                    new CouponRestaurant { RestaurantId = restaurantId.Value }
                        }
                        : new List<CouponRestaurant>()
                });
            }

            if (percentCouponCustomers.Any())
            {
                couponsToInsert.Add(new Coupon
                {
                    Name = $"{monthName}Percent10",
                    DiscountType = DiscountType.Percent,
                    DiscountValue = 10,
                    StartDate = today,
                    EndDate = today.AddMonths(1),
                    MinOrderAmount = 1000,
                    IsActive = true,
                    CouponCustomers = percentCouponCustomers,
                    CouponRestaurants = restaurantId.HasValue
                        ? new List<CouponRestaurant>
                        {
                    new CouponRestaurant { RestaurantId = restaurantId.Value }
                        }
                        : new List<CouponRestaurant>()
                });
            }

            if (bogoCouponCustomers.Any())
            {
                couponsToInsert.Add(new Coupon
                {
                    Name = $"{monthName}BOGO",
                    DiscountType = DiscountType.BOGO,
                    DiscountValue = 1,
                    StartDate = today,
                    EndDate = today.AddMonths(1),
                    MinOrderAmount = 0,
                    IsActive = true,
                    CouponCustomers = bogoCouponCustomers,
                    CouponRestaurants = restaurantId.HasValue
                        ? new List<CouponRestaurant>
                        {
                    new CouponRestaurant { RestaurantId = restaurantId.Value }
                        }
                        : new List<CouponRestaurant>()
                });
            }


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
            // Step 1: Start with active coupons
            var query = _context.Coupons
                .Where(c => c.IsActive)
                .AsQueryable();

            // Step 2: Filter by restaurant if provided
            if (restaurantId.HasValue)
            {
                query = query.Where(c =>
                    !_context.CouponRestaurants
                        .Where(cr => cr.CouponId == c.Id)
                        .Any() // Valid for all restaurants
                    ||
                    _context.CouponRestaurants
                        .Where(cr => cr.CouponId == c.Id && cr.RestaurantId == restaurantId.Value)
                        .Any() // Specific restaurant
                );
            }

            // Step 3: Filter by customer if provided
            if (customerId.HasValue)
            {
                query = query.Where(c =>
                    !_context.CouponsCustomer
                        .Where(cc => cc.CouponId == c.Id)
                        .Any() // Valid for all customers
                    ||
                    _context.CouponsCustomer
                        .Where(cc => cc.CouponId == c.Id && cc.CustomerId == customerId.Value)
                        .Any() // Specific customer
                );
            }

            // Step 4: Fetch data
            // Step 4: Project directly into DTO including IDs
            var mapped = await query
                .Select(c => new CouponResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    DiscountType = c.DiscountType.ToString(),
                    DiscountValue = c.DiscountValue,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    MinOrderAmount = c.MinOrderAmount,
                    IsActive = c.IsActive,
                    CustomerIds = _context.CouponsCustomer
                        .Where(cc => cc.CouponId == c.Id)
                        .Select(cc => cc.CustomerId)
                        .ToList(),
                    RestaurantIds = _context.CouponRestaurants
                        .Where(cr => cr.CouponId == c.Id)
                        .Select(cr => cr.RestaurantId)
                        .ToList()
                })
                .ToListAsync();

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
