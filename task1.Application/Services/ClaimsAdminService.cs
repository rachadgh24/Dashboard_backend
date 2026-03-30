using task1.Application.Interfaces;
using task1.Application.Models;
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

        public async Task<List<AdminClaimModel>> GetAllAsync()
        {
            var claims = await _roleRepository.GetAllClaimsAsync();
            return claims.Select(c => new AdminClaimModel
            {
                Id = c.Id,
                Name = c.Name,
                Category = c.Category
            }).ToList();
        }
    }
}
