using task1.Application.Interfaces;
using task1.Application.Resilience;
using task1.DataLayer.Entities;
using task1.DataLayer.Interfaces;

namespace task1.Application.Services
{
    public class RolesAdminService : IRolesAdminService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IDatabaseResiliencePipeline _resilience;

        public RolesAdminService(IRoleRepository roleRepository, IDatabaseResiliencePipeline resilience)
        {
            _roleRepository = roleRepository;
            _resilience = resilience;
        }

        public async Task<int> CreateRoleAsync(string name)
        {
            var trimmed = name?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(trimmed))
                throw new ArgumentException("Role name is required.", nameof(name));

            var existing = await _resilience.ExecuteAsync(() => _roleRepository.GetByNameAsync(trimmed));
            if (existing != null)
                throw new InvalidOperationException("Role name already exists.");

            var role = new Role { Name = trimmed };
            var created = await _resilience.ExecuteAsync(() => _roleRepository.CreateRoleAsync(role));
            await _resilience.ExecuteAsync(() => _roleRepository.SaveChangesAsync());
            return created.Id;
        }

        public async Task SetRoleClaimsAsync(int roleId, IEnumerable<int> claimIds)
        {
            if (roleId <= 0) throw new ArgumentOutOfRangeException(nameof(roleId));

            var role = await _resilience.ExecuteAsync(() => _roleRepository.GetByIdAsync(roleId));
            if (role == null) throw new InvalidOperationException("Role not found.");

            await _resilience.ExecuteAsync(() => _roleRepository.ReplaceRoleClaimsAsync(roleId, claimIds));
            await _resilience.ExecuteAsync(() => _roleRepository.SaveChangesAsync());
        }

        public async Task<string?> GetRoleNameByIdAsync(int roleId)
        {
            return await _resilience.ExecuteAsync(() => _roleRepository.GetRoleNameByIdAsync(roleId));
        }
    }
}

