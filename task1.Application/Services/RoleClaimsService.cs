using task1.Application.Interfaces;
using task1.Application.Resilience;
using task1.DataLayer.Interfaces;

namespace task1.Application.Services
{
    public class RoleClaimsService : IRoleClaimsService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IDatabaseResiliencePipeline _resilience;

        public RoleClaimsService(IRoleRepository roleRepository, IDatabaseResiliencePipeline resilience)
        {
            _roleRepository = roleRepository;
            _resilience = resilience;
        }

        public async Task<List<string>> GetClaimNamesForRoleAsync(string roleName)
        {
            return await _resilience.ExecuteAsync(() => _roleRepository.GetClaimNamesByRoleNameAsync(roleName));
        }
    }
}

