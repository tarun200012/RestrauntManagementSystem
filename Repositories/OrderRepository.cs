using Microsoft.EntityFrameworkCore;
using NLog;
using RestaurantAPI.Data;
using RestaurantAPI.Models;
using RestaurantAPI.Repositories.Interfaces;

namespace RestaurantAPI.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersForRestaurantAsync(int restaurantId)
        {
            return await _context.Orders
                .Where(o => o.RestaurantId == restaurantId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersForCustomerAtRestaurantAsync(int restaurantId, int customerId)
        {
            return await _context.Orders
        .Where(o => o.RestaurantId == restaurantId && o.CustomerId == customerId && o.IsConfirmed)
        .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.MenuItem) // Include MenuItem inside OrderItem
        .ToListAsync();
        }



    }
}
