using Microsoft.EntityFrameworkCore;
using NLog;
using RestaurantAPI.Data;
using RestaurantAPI.Models;
using RestaurantAPI.Repositories.Interfaces;

namespace RestaurantAPI.Repositories
{
    public class LocationRepository : ILocationRepository
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ApplicationDbContext _context;

        public LocationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Location>> GetAllAsync()
        {
            try
            {
                _logger.Info("Fetching all locations");
                return await _context.Locations.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GetAllAsync()");
                throw;
            }
        }

        public async Task<Location?> GetByIdAsync(int id)
        {
            try
            {
                _logger.Info($"Fetching location with ID: {id}");
                return await _context.Locations.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error in GetByIdAsync(ID: {id})");
                throw;
            }
        }

        public async Task AddAsync(Location entity)
        {
            try
            {
                _logger.Info($"Adding location: ID={entity.Id}");
                await _context.Locations.AddAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error while adding location: ID={entity.Id}");
                throw;
            }
        }

        public void Update(Location entity)
        {
            try
            {
                _logger.Info($"Updating location: ID={entity.Id}");
                _context.Locations.Update(entity);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error while updating location: ID={entity.Id}");
                throw;
            }
        }

        public void Delete(Location entity)
        {
            try
            {
                _logger.Info($"Deleting location: ID={entity.Id}");
                _context.Locations.Remove(entity);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error while deleting location: ID={entity.Id}");
                throw;
            }
        }

        public async Task SaveAsync()
        {
            try
            {
                _logger.Info("Saving changes to DB (Location)");
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while saving changes to DB (Location)");
                throw;
            }
        }
    }
}
