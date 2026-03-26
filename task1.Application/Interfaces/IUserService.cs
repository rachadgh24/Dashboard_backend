using task1.Application.Models;

namespace task1.Application.Interfaces
{
    public interface IUserService
    {
        Task<List<UserModel>> GetAllAsync(string? role = null);
        Task<List<UserModel>> PaginateUsersAsync(int page, int pageSize, string? role = null);
        Task<int> GetCountAsync(string? role = null);
        Task<UserModel?> GetByIdAsync(int id);
        Task<UserModel> AddUserAsync(CreateUserModel model);
        Task<bool> DeleteUserAsync(int id);
        Task<UserModel?> EditUserAsync(int id, UserModel model);
        Task<UserModel?> ChangeRoleAsync(int id, string role);
    }
}
