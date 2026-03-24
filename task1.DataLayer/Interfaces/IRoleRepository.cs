using task1.DataLayer.Entities;

namespace task1.DataLayer.Interfaces
{
    public interface IRoleRepository
    {
        Task<Role?> GetByNameAsync(string name);
        Task<Role?> GetByIdAsync(int id);
        Task<List<Role>> GetAllAsync();
        Task<List<string>> GetClaimNamesByRoleNameAsync(string roleName);
        Task<List<Claim>> GetAllClaimsAsync();
        Task<Role> CreateRoleAsync(Role role);
        Task ReplaceRoleClaimsAsync(int roleId, IEnumerable<int> claimIds);
        Task<string?> GetRoleNameByIdAsync(int id);
        Task SaveChangesAsync();
    }
}
