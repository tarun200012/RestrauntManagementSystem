using Microsoft.EntityFrameworkCore;
using NLog;
using RestaurantAPI.Data;
using RestaurantAPI.Models;
using RestaurantAPI.Repositories.Interfaces;

namespace RestaurantAPI.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            _logger.Info("Fetching all customers");
            return await _context.Customers
                .Where(c => !c.IsDeleted)
                .ToListAsync();
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            _logger.Info($"Fetching customer with ID: {id}");
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task AddAsync(Customer customer)
        {
            _logger.Info($"Adding new customer: {customer.Name}");
            await _context.Customers.AddAsync(customer);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
