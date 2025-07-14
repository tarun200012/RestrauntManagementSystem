using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Data;
using RestaurantAPI.Models;
using RestaurantAPI.Repositories.Interfaces;

namespace RestaurantAPI.Repositories
{
    public class RestaurantRepository : IRestaurantRepository
    {
        private readonly ApplicationDbContext _context;

        public RestaurantRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Restaurant>> GetAllAsync()
        {
            return await _context.Restaurants.ToListAsync();
        }

        public async Task<IEnumerable<Restaurant>> GetAllWithLocationAsync()
        {
            return await _context.Restaurants.Include(r => r.Location).ToListAsync();
        }

        public async Task<Restaurant?> GetByIdAsync(int id)
        {
            return await _context.Restaurants.FindAsync(id);
        }

        public async Task AddAsync(Restaurant entity)
        {
            await _context.Restaurants.AddAsync(entity);
        }

        public void Update(Restaurant entity)
        {
            _context.Restaurants.Update(entity);
        }

        public void Delete(Restaurant entity)
        {
            _context.Restaurants.Remove(entity);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
