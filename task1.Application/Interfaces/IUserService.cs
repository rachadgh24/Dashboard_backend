using task1.Application.Models;

namespace task1.Application.Interfaces
{
    public interface IUserService
    {
        Task<List<UserModel>> GetAllAsync(string? role = null);
        Task<List<UserModel>> PaginateUsersAsync(int page);
        Task<int> GetCountAsync();
        Task<UserModel?> GetByIdAsync(int id);
        Task<UserModel> AddUserAsync(CreateUserModel model);
        Task<bool> DeleteUserAsync(int id);
        Task<UserModel?> EditUserAsync(int id, UserModel model);
        Task<UserModel?> ChangeRoleAsync(int id, string role);
    }
}
