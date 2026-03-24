using task1.Application.Interfaces;
using task1.Application.Resilience;
using task1.DataLayer.Entities;
using task1.DataLayer.Interfaces;

namespace task1.Application.Services
{
    public class ClaimsAdminService : IClaimsAdminService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IDatabaseResiliencePipeline _resilience;

        public ClaimsAdminService(IRoleRepository roleRepository, IDatabaseResiliencePipeline resilience)
        {
            _roleRepository = roleRepository;
            _resilience = resilience;
        }

        public async Task<List<Claim>> GetAllAsync()
        {
            return await _resilience.ExecuteAsync(() => _roleRepository.GetAllClaimsAsync());
        }
    }
}

