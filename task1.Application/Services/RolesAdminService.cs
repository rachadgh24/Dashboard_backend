using task1.Application.Interfaces;
using task1.Application.Models;
using task1.DataLayer.Entities;
using task1.DataLayer.Interfaces;

namespace task1.Application.Services
{
    public class RolesAdminService : IRolesAdminService
    {
        private readonly IRoleRepository _roleRepository;

        public RolesAdminService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<List<AdminRoleModel>> GetAllAsync(Guid tenantId)
        {
            var roles = await _roleRepository.GetAllAsync(tenantId);
            return roles.Select(r => new AdminRoleModel { Id = r.Id, Name = r.Name }).ToList();
        }

        public async Task<int> CreateRoleAsync(Guid tenantId, string name)
        {
            var trimmed = name?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(trimmed))
                throw new ArgumentException("Role name is required.", nameof(name));

            var existing = await _roleRepository.GetByNameAsync(trimmed, tenantId);
            if (existing != null)
                throw new InvalidOperationException("Role name already exists.");

            var role = new Role { Name = trimmed, TenantId = tenantId };
            var created = await _roleRepository.CreateRoleAsync(role);
            await _roleRepository.SaveChangesAsync();
            return created.Id;
        }

        public async Task SetRoleClaimsAsync(Guid tenantId, int roleId, IEnumerable<int> claimIds)
        {
            if (roleId <= 0) throw new ArgumentOutOfRangeException(nameof(roleId));

            var role = await _roleRepository.GetByIdAsync(roleId, tenantId);
            if (role == null) throw new InvalidOperationException("Role not found.");

            await _roleRepository.ReplaceRoleClaimsAsync(roleId, claimIds);
            await _roleRepository.SaveChangesAsync();
        }

        public async Task<string?> GetRoleNameByIdAsync(Guid tenantId, int roleId)
        {
            return await _roleRepository.GetRoleNameByIdAsync(roleId, tenantId);
        }
    }
}
