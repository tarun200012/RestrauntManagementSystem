using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Data;
using RestaurantAPI.Repositories.Interfaces;
using RestaurantAPI.Utilities;

namespace RestaurantAPI.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationDbContext _context;

        public AuthRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            
            return await _context.Customers.Include(c=>c.Role).FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<Customer> RegisterAsync(Customer customer)
        {
            customer.Password = PasswordHasher.HashPassword(customer.Password);
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }
    }

}
