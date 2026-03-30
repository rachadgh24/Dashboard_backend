namespace task1.Application.Interfaces
{
    public interface IRoleClaimsService
    {
        Task<List<string>> GetClaimNamesForRoleAsync(string roleName);
        Task<List<string>> GetClaimNamesForRoleAsync(string roleName, Guid tenantId);
    }
}

