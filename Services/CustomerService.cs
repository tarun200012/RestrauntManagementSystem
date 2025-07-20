using AutoMapper;
using NLog;
using RestaurantAPI.Dtos;
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

        public CustomerService(ICustomerRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
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
