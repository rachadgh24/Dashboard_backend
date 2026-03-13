using task1.Application.Models;

namespace task1.Application.Interfaces
{
    public interface ICustomerService
    {
        Task<List<CustomerModel>> GetAllAsync();
        Task<CustomerModel?> GetByIdAsync(int id);
        Task<CustomerModel> AddCustomerAsync(CustomerModel customerModel);
        Task<CustomerModel?> UpdateCustomerAsync(int id, CustomerModel customerModel);
        Task<bool> DeleteCustomerAsync(int id);
        Task<List<CustomerModel>> Search(string? query);
        Task<List<CustomerModel>> PaginateCustomersAsync(int page, string? sortBy);
        Task<int> GetCountAsync();
        Task<(string Name, int CarCount, List<CarModel> Cars)?> GetCustomerWithMostCarsAsync();
    }
}
