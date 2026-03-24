namespace task1.Application.Interfaces
{
    public interface IRolesAdminService
    {
        Task<int> CreateRoleAsync(string name);
        Task SetRoleClaimsAsync(int roleId, IEnumerable<int> claimIds);
        Task<string?> GetRoleNameByIdAsync(int roleId);
    }
}

