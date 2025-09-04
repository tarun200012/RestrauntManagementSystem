namespace RestaurantAPI.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<Customer?> GetByEmailAsync(string email);
        Task<Customer> RegisterAsync(Customer customer);
    }
}
