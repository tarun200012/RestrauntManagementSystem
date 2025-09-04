using RestaurantAPI.Dtos;
using RestaurantAPI.DTOs.Common;

namespace RestaurantAPI.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDto>> GetAllAsync();
        Task<CustomerDto?> GetByIdAsync(int id);
        Task<CustomerDto> AddAsync(CustomerDto dto);

        Task<PaginatedResponse<CustomerDto>> GetPaginatedAsync(string search, int pageNumber, int pageSize);
    }
}
