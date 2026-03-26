using task1.Application.Interfaces;
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

        public async Task<int> CreateRoleAsync(string name)
        {
            var trimmed = name?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(trimmed))
                throw new ArgumentException("Role name is required.", nameof(name));

            var existing = await _roleRepository.GetByNameAsync(trimmed);
            if (existing != null)
                throw new InvalidOperationException("Role name already exists.");

            var role = new Role { Name = trimmed };
            var created = await _roleRepository.CreateRoleAsync(role);
            await _roleRepository.SaveChangesAsync();
            return created.Id;
        }

        public async Task SetRoleClaimsAsync(int roleId, IEnumerable<int> claimIds)
        {
            if (roleId <= 0) throw new ArgumentOutOfRangeException(nameof(roleId));

            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null) throw new InvalidOperationException("Role not found.");

            await _roleRepository.ReplaceRoleClaimsAsync(roleId, claimIds);
            await _roleRepository.SaveChangesAsync();
        }

        public async Task<string?> GetRoleNameByIdAsync(int roleId)
        {
            return await _roleRepository.GetRoleNameByIdAsync(roleId);
        }
    }
}
