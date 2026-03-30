using task1.Application.Models;

namespace task1.Application.Interfaces
{
    public interface IUserService
    {
        Task<List<UserModel>> GetAllAsync(Guid tenantId, string? role = null);
        Task<List<UserModel>> PaginateUsersAsync(Guid tenantId, int page, int pageSize, string? role = null);
        Task<int> GetCountAsync(Guid tenantId, string? role = null);
        Task<UserModel?> GetByIdAsync(Guid tenantId, int id);
        Task<UserModel> AddUserAsync(Guid tenantId, CreateUserModel model);
        Task<bool> DeleteUserAsync(Guid tenantId, int id);
        Task<UserModel?> EditUserAsync(Guid tenantId, int id, UserModel model);
        Task<UserModel?> ChangeRoleAsync(Guid tenantId, int id, string role);
    }
}
