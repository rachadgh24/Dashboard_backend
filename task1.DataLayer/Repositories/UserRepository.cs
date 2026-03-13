using Microsoft.EntityFrameworkCore;
using task1.DataLayer.DbContexts;
using task1.DataLayer.Entities;
using task1.DataLayer.Interfaces;

namespace task1.DataLayer.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DotNetTrainingCoreContext _context;

        public UserRepository(DotNetTrainingCoreContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllAsync(string? role = null)
        {
            var query = _context.Users.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(role))
                query = query.Where(u => u.Role == role);
            return await query.OrderBy(u => u.Id).ToListAsync();
        }

        public async Task<List<User>> PaginateUsersAsync(int page)
        {
            return await _context.Users
                .AsNoTracking()
                .OrderBy(u => u.Id)
                .Skip((page - 1) * 4)
                .Take(4)
                .ToListAsync();
        }

        public async Task<int> GetCountAsync()
        {
            return await _context.Users.CountAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public Task<User> AddUserAsync(User user)
        {
            var entity = _context.Users.Add(user);
            return Task.FromResult(entity.Entity);
        }

        public async Task<User?> UpdateUserAsync(User user)
        {
            var entity = await _context.Users.FindAsync(user.Id);
            if (entity == null) return null;

            entity.Name = user.Name;
            entity.Email = user.Email;
            entity.Role = user.Role;
            if (!string.IsNullOrEmpty(user.PasswordHash))
                entity.PasswordHash = user.PasswordHash;
            return entity;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var entity = await _context.Users.FindAsync(id);
            if (entity == null) return false;

            _context.Users.Remove(entity);
            return true;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
