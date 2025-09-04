using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using NLog;
using RestaurantAPI.Data;
using RestaurantAPI.Dtos;
using RestaurantAPI.DTOs.Common;
using RestaurantAPI.Models;
using RestaurantAPI.Repositories.Interfaces;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Services
{
    public class CustomerService : ICustomerService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ICustomerRepository _repository;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public CustomerService(ICustomerRepository repository, IMapper mapper,ApplicationDbContext context)
        {
            _repository = repository;
            _mapper = mapper;
            _context = context;
        }

        public async Task<IEnumerable<CustomerDto>> GetAllAsync()
        {
            try
            {
                _logger.Info("Fetching all customers.");
                var customers = await _repository.GetAllAsync();
                return _mapper.Map<IEnumerable<CustomerDto>>(customers);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while fetching all customers.");
                throw;
            }
        }

        public async Task<PaginatedResponse<CustomerDto>> GetPaginatedAsync(string search, int pageNumber, int pageSize)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 50;
                var query = _context.Customers.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(c => c.Name.Contains(search));
                }

                var totalCount = await query.CountAsync();

                var customers = await query
                    .OrderBy(c=> c.Id)
                    .ThenBy(c => c.Name)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ProjectTo<CustomerDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                var result = new PaginatedResponse<CustomerDto>
                {
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Data = customers
                };

                return result;



            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while fetching paginated customers.");
                throw;
            }
        }

        public async Task<CustomerDto?> GetByIdAsync(int id)
        {
            try
            {
                _logger.Info($"Fetching customer by ID: {id}");
                var customer = await _repository.GetByIdAsync(id);
                return customer == null ? null : _mapper.Map<CustomerDto>(customer);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"An error occurred while fetching customer by ID: {id}");
                throw;
            }
        }

        public async Task<CustomerDto> AddAsync(CustomerDto dto)
        {
            try
            {
                _logger.Info("Adding a new customer.");
                var customer = _mapper.Map<Customer>(dto);
                await _repository.AddAsync(customer);
                await _repository.SaveAsync();
                _logger.Info("Customer added successfully.");
                return _mapper.Map<CustomerDto>(customer);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while adding a customer.");
                throw;
            }
        }
    }
}
