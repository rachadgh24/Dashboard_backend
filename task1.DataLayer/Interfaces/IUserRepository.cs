using task1.DataLayer.Entities;

namespace task1.DataLayer.Interfaces
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllAsync(Guid tenantId, string? role = null);
        Task<List<User>> PaginateUsersAsync(Guid tenantId, int page, int pageSize, string? role = null);
        Task<int> GetCountAsync(Guid tenantId, string? role = null);
        Task<User?> GetByPhoneNumberAsync(string phoneNumber);
        Task<User?> GetByPhoneNumberAsync(Guid tenantId, string phoneNumber);
        Task<User?> GetByIdAsync(Guid tenantId, int id);
        Task<User> AddUserAsync(User user);
        Task<User?> UpdateUserAsync(Guid tenantId, User user);
        Task<bool> DeleteUserAsync(Guid tenantId, int id);
        Task SaveChangesAsync();
    }
}
