namespace RestaurantAPI.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetOrdersForRestaurantAsync(int restaurantId);
        Task AddAsync(Order order); // Add this method to fix the error  
        Task SaveAsync(); // Ensure SaveAsync is also defined  
    }
}
