using task1.Application.Interfaces;
using task1.DataLayer.Entities;
using task1.DataLayer.Interfaces;

namespace task1.Application.Services
{
    public class ClaimsAdminService : IClaimsAdminService
    {
        private readonly IRoleRepository _roleRepository;

        public ClaimsAdminService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<List<Claim>> GetAllAsync()
        {
            return await _roleRepository.GetAllClaimsAsync();
        }
    }
}
