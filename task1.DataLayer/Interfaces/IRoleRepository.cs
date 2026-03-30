using task1.DataLayer.Entities;

namespace task1.DataLayer.Interfaces
{
    public interface IRoleRepository
    {
        Task<Role?> GetByNameAsync(string name);
        Task<Role?> GetByNameAsync(string name, Guid tenantId);
        Task<Role?> GetByIdAsync(int id);
        Task<Role?> GetByIdAsync(int id, Guid tenantId);
        Task<List<Role>> GetAllAsync();
        Task<List<Role>> GetAllAsync(Guid tenantId);
        Task<List<string>> GetClaimNamesByRoleNameAsync(string roleName);
        Task<List<string>> GetClaimNamesByRoleNameAsync(string roleName, Guid tenantId);
        Task<List<Claim>> GetAllClaimsAsync();
        Task<Role> CreateRoleAsync(Role role);
        Task ReplaceRoleClaimsAsync(int roleId, IEnumerable<int> claimIds);
        Task<string?> GetRoleNameByIdAsync(int id);
        Task<string?> GetRoleNameByIdAsync(int id, Guid tenantId);
        Task SaveChangesAsync();
    }
}
