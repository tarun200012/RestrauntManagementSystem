namespace RestaurantAPI.Services.Interfaces
{
    public interface IOrderService
    {
        Task AddAsync(Order order);
        Task SaveAsync();
        Task<(bool IsSuccess, string Message)> ScheduleOrderAsync(int restaurantId, int customerId, ScheduleOrderRequest request);

        Task<IEnumerable<Order>> GetOrdersForCustomerAtRestaurantAsync(int restaurantId, int customerId);


    }
}
