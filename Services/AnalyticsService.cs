using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Data;
using RestaurantAPI.DTOs.Admin;
using RestaurantAPI.DTOs.Common;
using RestaurantAPI.Services.Interfaces;
using System;

namespace RestaurantAPI.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OrdersOverviewDto> GetOrdersOverviewAsync(int? restaurantId)
        {
            var query = _context.Orders.AsNoTracking();

            if (restaurantId.HasValue)
                query = query.Where(o => o.RestaurantId == restaurantId);

            var now = DateTime.UtcNow;
            var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            return new OrdersOverviewDto
            {
                TotalOrders = await query.CountAsync(),
                OrdersToday = await query.CountAsync(o => o.ScheduledAt.Date == now.Date),
                OrdersThisWeek = await query.CountAsync(o => o.ScheduledAt >= startOfWeek),
                OrdersThisMonth = await query.CountAsync(o => o.ScheduledAt >= startOfMonth)
            };
        }

        public async Task<PaginatedResponse<RevenueDto>> GetRevenueAsync(
      int? restaurantId,
      int pageNumber,
      int pageSize,
      int? year = null,
      int? month = null,
      bool includeProfitability = false)
        {
            var finalYear = year ?? DateTime.UtcNow.Year;
            var finalMonth = month ?? DateTime.UtcNow.Month;

            var revenueData = await _context.RevenueDtos
                .FromSqlRaw(
                    "EXEC dbo.usp_GenerateRestaurantMonthlyReport @Year = {0}, @Month = {1}, @RestaurantId = {2}, @IncludeProfitability = {3}",
                    finalYear,
                    finalMonth,
                    restaurantId ?? (object)DBNull.Value,
                    includeProfitability ? 1 : 0
                )
                .AsNoTracking()
                .ToListAsync();

            var totalCount = revenueData.Count;
            var pagedData = revenueData
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PaginatedResponse<RevenueDto>
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = pagedData
            };
        }


        public async Task<PaginatedResponse<MenuItemSalesDto>> GetTopItemsAsync(int? restaurantId, int pageNumber, int pageSize)
        {
            var query = _context.OrderItems
                .Include(oi => oi.MenuItem)
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.IsConfirmed)
                .AsNoTracking();

            if (restaurantId.HasValue)
                query = query.Where(oi => oi.Order.RestaurantId == restaurantId);

            var grouped = await query
                .GroupBy(oi => new { oi.MenuItemId})
                .Select(g => new MenuItemSalesDto
                {
                    MenuItemId = g.Key.MenuItemId,
                    MenuItemName = g.Max(x=> x.MenuItem.Name),
                    QuantitySold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.Quantity * x.MenuItem.Price)
                })
                .OrderByDescending(x => x.QuantitySold)
                .ToListAsync();

            return new PaginatedResponse<MenuItemSalesDto>
            {
                TotalCount = grouped.Count,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = grouped.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList()
            };
        }

        public async Task<PaginatedResponse<CustomerActivityDto>> GetTopCustomersAsync(int? restaurantId, int pageNumber, int pageSize)
        {
            var query = _context.Orders
                .Include(o => o.Customer)
                .AsNoTracking();

            if (restaurantId.HasValue)
                query = query.Where(o => o.RestaurantId == restaurantId);

            var grouped = await query
                .GroupBy(o => new { o.CustomerId})
                .Select(g => new CustomerActivityDto
                {
                    CustomerId = g.Key.CustomerId,
                    CustomerName = g.Max(o=> o.Customer.Name),
                    OrderCount = g.Count(),
                    TotalSpent = g.SelectMany(o => o.OrderItems)
                                  .Sum(oi => oi.Quantity * oi.MenuItem.Price)
                })
                .OrderByDescending(x => x.OrderCount)
                .ToListAsync();

            return new PaginatedResponse<CustomerActivityDto>
            {
                TotalCount = grouped.Count,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = grouped.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList()
            };
        }

        public async Task<PaginatedResponse<PeakHourDto>> GetPeakHoursAsync(int? restaurantId, int pageNumber, int pageSize)
        {
            var query = _context.Orders.AsNoTracking();

            if (restaurantId.HasValue)
                query = query.Where(o => o.RestaurantId == restaurantId);

            var grouped = await query
                .GroupBy(o => o.ScheduledAt.Hour)
                .Select(g => new PeakHourDto
                {
                    Hour = g.Key,
                    OrderCount = g.Count()
                })
                .OrderByDescending(x => x.OrderCount)
                .ToListAsync();

            return new PaginatedResponse<PeakHourDto>
            {
                TotalCount = grouped.Count,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = grouped.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList()
            };
        }
    }
}
