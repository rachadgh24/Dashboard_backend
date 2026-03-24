using Microsoft.EntityFrameworkCore;
using task1.DataLayer.DbContexts;
using task1.DataLayer.Entities;
using task1.DataLayer.Interfaces;

namespace task1.DataLayer.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly DotNetTrainingCoreContext _context;

        public RoleRepository(DotNetTrainingCoreContext context)
        {
            _context = context;
        }

        public async Task<Role?> GetByNameAsync(string name)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == name);
        }

        public async Task<Role?> GetByIdAsync(int id)
        {
            return await _context.Roles.FindAsync(id);
        }

        public async Task<List<Role>> GetAllAsync()
        {
            return await _context.Roles.AsNoTracking().OrderBy(r => r.Id).ToListAsync();
        }

        public async Task<List<string>> GetClaimNamesByRoleNameAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName)) return new List<string>();

            return await _context.Roles
                .AsNoTracking()
                .Where(r => r.Name == roleName)
                .SelectMany(r => r.RoleClaims.Select(rc => rc.Claim.Name))
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();
        }

        public async Task<List<Claim>> GetAllClaimsAsync()
        {
            return await _context.Claims
                .AsNoTracking()
                .OrderBy(c => c.Category)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public Task<Role> CreateRoleAsync(Role role)
        {
            var entity = _context.Roles.Add(role);
            return Task.FromResult(entity.Entity);
        }

        public async Task ReplaceRoleClaimsAsync(int roleId, IEnumerable<int> claimIds)
        {
            var ids = claimIds?.Distinct().ToArray() ?? Array.Empty<int>();

            var existing = await _context.RoleClaims.Where(rc => rc.RoleId == roleId).ToListAsync();
            if (existing.Count > 0) _context.RoleClaims.RemoveRange(existing);

            if (ids.Length == 0) return;

            var newLinks = ids.Select(id => new RoleClaim { RoleId = roleId, ClaimId = id });
            await _context.RoleClaims.AddRangeAsync(newLinks);
        }

        public async Task<string?> GetRoleNameByIdAsync(int id)
        {
            return await _context.Roles.AsNoTracking().Where(r => r.Id == id).Select(r => r.Name).FirstOrDefaultAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
