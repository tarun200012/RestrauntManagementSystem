using RestaurantAPI.Dtos;

namespace RestaurantAPI.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDto>> GetAllAsync();
        Task<CustomerDto?> GetByIdAsync(int id);
        Task<CustomerDto> AddAsync(CustomerDto dto);
    }
}
