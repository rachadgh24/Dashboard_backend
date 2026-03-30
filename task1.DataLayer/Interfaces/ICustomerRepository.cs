using task1.DataLayer.Entities;

namespace task1.DataLayer.Interfaces
{
    public interface ICustomerRepository
    {
        IQueryable<Customer> GetAll(Guid tenantId);
        IQueryable<Customer> GetById(Guid tenantId, int id);
        Task<Customer?> GetByEmailAsync(Guid tenantId, string email);
        Task<Customer?> UpdateCustomer(Guid tenantId, int id, Customer customer);
        Task<bool> DeleteCustomer(Guid tenantId, int id);
        Task<Customer> AddCustomerAsync(Customer customer);
        Task<List<Customer>> Search(Guid tenantId, string? query);
        Task<List<Customer>> PaginateCustomers(Guid tenantId, int page, int pageSize, string? sortBy);
        Task<int> GetCountAsync(Guid tenantId);
        Task<(Customer Customer, int CarCount)?> GetCustomerWithMostCarsAsync(Guid tenantId);
        Task SaveChangesAsync();
    }
}
