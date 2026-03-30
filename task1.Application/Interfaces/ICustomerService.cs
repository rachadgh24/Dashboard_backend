using task1.Application.Models;

namespace task1.Application.Interfaces
{
    public interface ICustomerService
    {
        Task<List<CustomerModel>> GetAllAsync(Guid tenantId);
        Task<CustomerModel?> GetByIdAsync(Guid tenantId, int id);
        Task<CustomerModel> AddCustomerAsync(Guid tenantId, CustomerModel customerModel);
        Task<CustomerModel?> UpdateCustomerAsync(Guid tenantId, int id, CustomerModel customerModel);
        Task<bool> DeleteCustomerAsync(Guid tenantId, int id);
        Task<List<CustomerModel>> Search(Guid tenantId, string? query);
        Task<List<CustomerModel>> PaginateCustomersAsync(Guid tenantId, int page, int pageSize, string? sortBy);
        Task<int> GetCountAsync(Guid tenantId);
        Task<(string Name, int CarCount, List<CarModel> Cars)?> GetCustomerWithMostCarsAsync(Guid tenantId);
    }
}
