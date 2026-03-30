using task1.Application.Models;

namespace task1.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthLoginResultModel?> LoginAsync(string phoneNumber, string password);
        Task<AuthRegisterResultModel> RegisterAsync(string organizationName, string? firstName, string? lastName, string phoneNumber, string password);
        Task<AuthPermissionsResultModel> GetPermissionsAsync(string? roleName, Guid tenantId);
    }
}
