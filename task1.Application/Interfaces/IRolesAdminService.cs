using task1.Application.Models;

namespace task1.Application.Interfaces
{
    public interface IRolesAdminService
    {
        Task<List<AdminRoleModel>> GetAllAsync(Guid tenantId);
        Task<int> CreateRoleAsync(Guid tenantId, string name);
        Task SetRoleClaimsAsync(Guid tenantId, int roleId, IEnumerable<int> claimIds);
        Task<string?> GetRoleNameByIdAsync(Guid tenantId, int roleId);
    }
}

