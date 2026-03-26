using task1.Application.Interfaces;
using task1.DataLayer.Interfaces;

namespace task1.Application.Services
{
    public class RoleClaimsService : IRoleClaimsService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleClaimsService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<List<string>> GetClaimNamesForRoleAsync(string roleName)
        {
            return await _roleRepository.GetClaimNamesByRoleNameAsync(roleName);
        }
    }
}
