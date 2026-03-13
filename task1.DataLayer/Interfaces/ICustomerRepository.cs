using task1.DataLayer.Entities;

namespace task1.DataLayer.Interfaces
{
    public interface ICustomerRepository
    {
        IQueryable<Customer> GetAll();
        IQueryable<Customer> GetById(int id);
        Task<Customer?> GetByEmailAsync(string email);
        Task<Customer?> UpdateCustomer(int id, Customer customer);
        Task<bool> DeleteCustomer(int id);
        Task<Customer> AddCustomerAsync(Customer customer);
        Task<List<Customer>> Search(string? query);
        Task<List<Customer>> PaginateCustomers(int page, string? sortBy);
        Task<int> GetCountAsync();
        Task<(Customer Customer, int CarCount)?> GetCustomerWithMostCarsAsync();
        Task SaveChangesAsync();
    }
}
