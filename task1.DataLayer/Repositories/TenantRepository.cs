using task1.DataLayer.DbContexts;
using task1.DataLayer.Entities;
using task1.DataLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace task1.DataLayer.Repositories
{
    public class TenantRepository : ITenantRepository
    {
        private readonly DotNetTrainingCoreContext _context;

        public TenantRepository(DotNetTrainingCoreContext context)
        {
            _context = context;
        }

        public Task<Tenant> CreateAsync(Tenant tenant)
        {
            var entity = _context.Tenants.Add(tenant);
            return Task.FromResult(entity.Entity);
        }

        public async Task<Tenant?> GetByIdAsync(Guid tenantId)
        {
            return await _context.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TenantId == tenantId);
        }

        public async Task<Tenant?> GetByNameAsync(string name)
        {
            var normalized = name.Trim().ToLowerInvariant();
            return await _context.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Name.ToLower() == normalized);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
