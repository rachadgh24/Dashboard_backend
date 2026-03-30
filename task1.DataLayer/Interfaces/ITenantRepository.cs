using task1.DataLayer.Entities;

namespace task1.DataLayer.Interfaces
{
    public interface ITenantRepository
    {
        Task<Tenant> CreateAsync(Tenant tenant);
        Task<Tenant?> GetByIdAsync(Guid tenantId);
        Task<Tenant?> GetByNameAsync(string name);
        Task SaveChangesAsync();
    }
}
