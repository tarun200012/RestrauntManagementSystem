using AutoMapper;
using NLog;
using RestaurantAPI.Data;
using RestaurantAPI.Models;
using RestaurantAPI.Repositories.Interfaces;
using RestaurantAPI.Services.Interfaces;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _context;

    public OrderService(IOrderRepository orderRepository, IRestaurantRepository restaurantRepository,IMapper mapper, ApplicationDbContext context)
    {
        _orderRepository = orderRepository;
        _restaurantRepository = restaurantRepository;
        _context = context;
        _mapper = mapper;
    }


    public async Task<(bool IsSuccess, string Message)> ScheduleOrderAsync(int restaurantId, int customerId, ScheduleOrderRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);
            if (restaurant == null)
                return (false, $"Restaurant with ID {restaurantId} not found.");


            var scheduledUtcTime = request.ScheduledAt;
            var scheduleTime = scheduledUtcTime.ToLocalTime(); // Now in local time
            var scheduleTimeOfDay = scheduleTime.TimeOfDay;

            if (scheduleTime < DateTime.Now)
                return (false, "You cannot schedule an order in the past.");


            if (restaurant.OpenTime == null || restaurant.CloseTime == null)
                return (false, "Restaurant's opening and closing time not set.");

            bool isOpen;

            if (restaurant.OpenTime < restaurant.CloseTime)
            {
                // ✅ Same day window
                isOpen = scheduleTimeOfDay >= restaurant.OpenTime && scheduleTimeOfDay <= restaurant.CloseTime;
            }
            else
            {
                // ✅ Overnight window (spans midnight)
                isOpen = scheduleTimeOfDay >= restaurant.OpenTime || scheduleTimeOfDay <= restaurant.CloseTime;
            }

            if (!isOpen)
            {
                return (false, $"Order time {scheduleTime:t} is outside restaurant hours ({restaurant.OpenTime:t} - {restaurant.CloseTime:t}).");
            }

            var windowStart = scheduleTime.Date.AddHours(scheduleTime.Hour);
            var windowEnd = windowStart.AddHours(1);

            var existingOrders = await _orderRepository.GetOrdersForRestaurantAsync(restaurantId);
            var countInWindow = existingOrders.Count(o => o.ScheduledAt >= windowStart && o.ScheduledAt < windowEnd);

            if (countInWindow >= 10)
            {
                return (false, $"The selected 1-hour time slot ({windowStart:t} - {windowEnd:t}) is fully booked. Please choose a different time.");
            }
            Coupon coupon = null;
            if (request.CouponId.HasValue)
            {
                coupon = await _context.Coupons.FindAsync(request.CouponId.Value);
                if (coupon == null || !coupon.IsActive ||
                    coupon.StartDate > DateTime.UtcNow || coupon.EndDate < DateTime.UtcNow)
                {
                    return (false, "Invalid or expired coupon.");
                }
            }

            var order = new Order
            {
                RestaurantId = restaurantId,
                CustomerId = customerId,
                ScheduledAt = scheduleTime,
                CouponId = request.CouponId ?? null,
                IsConfirmed = true, // Initially not confirmed
                OrderItems = request.OrderItems.Select(i => new OrderItem
                {
                    MenuItemId = i.MenuItemId,
                    Quantity = i.Quantity
                }).ToList()
            };

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveAsync();

            await transaction.CommitAsync();
            return (true, "Order scheduled successfully.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.Error(ex, $"Error scheduling order for RestaurantId {restaurantId} and CustomerId {customerId}");
            throw; // Bubble to global exception handler
        }
    }

    public async Task AddAsync(Order order)
    {
        try
        {
            await _context.Orders.AddAsync(order);
        }
        catch (Exception ex)
        {
            // Bubble up to service layer
            throw new Exception("Failed to add order.", ex);
        }
    }

    public async Task SaveAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to save changes to the database.", ex);
        }
    }

    public async Task<IEnumerable<Order>> GetOrdersForCustomerAtRestaurantAsync(int restaurantId, int customerId)
    {
        return await _orderRepository.GetOrdersForCustomerAtRestaurantAsync(restaurantId, customerId);
    }


}
