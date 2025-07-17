using Microsoft.EntityFrameworkCore;
using NLog;
using RestaurantAPI.Data;
using RestaurantAPI.Models;
using RestaurantAPI.Repositories.Interfaces;

namespace RestaurantAPI.Repositories
{
    public class RestaurantRepository : IRestaurantRepository
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ApplicationDbContext _context;

        public RestaurantRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Restaurant>> GetAllAsync()
        {
            try
            {
                _logger.Info("Fetching all restaurants");
                return await _context.Restaurants.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GetAllAsync()");
                throw;
            }
        }

        public async Task<IEnumerable<Restaurant>> GetAllWithLocationAsync()
        {
            try
            {
                _logger.Info("Fetching all restaurants with their locations");
                return await _context.Restaurants.Include(r => r.Location).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GetAllWithLocationAsync()");
                throw;
            }
        }

        public async Task<Restaurant?> GetByIdAsync(int id)
        {
            try
            {
                _logger.Info($"Fetching restaurant with ID: {id}");
                return await _context.Restaurants
                    .Include(r => r.Location)
                    .FirstOrDefaultAsync(r => r.Id == id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error in GetByIdAsync(ID: {id})");
                throw;
            }
        }

        public async Task AddAsync(Restaurant entity)
        {
            try
            {
                _logger.Info($"Adding restaurant: {entity.Name}");
                await _context.Restaurants.AddAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error while adding restaurant: {entity.Name}");
                throw;
            }
        }

        public void Update(Restaurant entity)
        {
            try
            {
                _logger.Info($"Updating restaurant ID: {entity.Id}");
                _context.Restaurants.Update(entity);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error while updating restaurant ID: {entity.Id}");
                throw;
            }
        }

        public void Delete(Restaurant entity)
        {
            try
            {
                _logger.Info($"Deleting restaurant ID: {entity.Id}");
                _context.Restaurants.Remove(entity);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error while deleting restaurant ID: {entity.Id}");
                throw;
            }
        }

        public async Task SaveAsync()
        {
            try
            {
                _logger.Info("Saving changes to database");
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while saving changes to database");
                throw;
            }
        }

        public async Task AddBulkAsync(List<Restaurant> restaurants)
        {
            try
            {
                _logger.Info($"Adding {restaurants.Count} restaurants in bulk");
                await _context.Restaurants.AddRangeAsync(restaurants);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while bulk-adding restaurants");
                throw;
            }
        }
    }
}
