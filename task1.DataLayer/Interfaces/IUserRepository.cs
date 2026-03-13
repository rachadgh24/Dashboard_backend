using task1.DataLayer.Entities;

namespace task1.DataLayer.Interfaces
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllAsync(string? role = null);
        Task<List<User>> PaginateUsersAsync(int page);
        Task<int> GetCountAsync();
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(int id);
        Task<User> AddUserAsync(User user);
        Task<User?> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
        Task SaveChangesAsync();
    }
}
