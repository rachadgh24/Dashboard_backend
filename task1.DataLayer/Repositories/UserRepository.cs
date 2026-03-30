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

        public async Task<List<User>> GetAllAsync(Guid tenantId, string? role = null)
        {
            IQueryable<User> query = _context.Users.AsNoTracking().Include(u => u.Role).Where(u => u.TenantId == tenantId);
            if (!string.IsNullOrWhiteSpace(role))
                query = query.Where(u => u.Role.Name == role);
            return await query.OrderBy(u => u.Id).ToListAsync();
        }

        public async Task<List<User>> PaginateUsersAsync(Guid tenantId, int page, int pageSize, string? role = null)
        {
            IQueryable<User> query = _context.Users.AsNoTracking().Include(u => u.Role).Where(u => u.TenantId == tenantId);
            if (!string.IsNullOrWhiteSpace(role))
                query = query.Where(u => u.Role.Name == role);
            return await query
                .OrderBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetCountAsync(Guid tenantId, string? role = null)
        {
            IQueryable<User> query = _context.Users.Where(u => u.TenantId == tenantId);
            if (!string.IsNullOrWhiteSpace(role))
                query = query.Where(u => u.Role.Name == role);
            return await query.CountAsync();
        }

        public async Task<User?> GetByPhoneNumberAsync(string phoneNumber)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        }

        public async Task<User?> GetByPhoneNumberAsync(Guid tenantId, string phoneNumber)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.PhoneNumber == phoneNumber);
        }

        public async Task<User?> GetByIdAsync(Guid tenantId, int id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Id == id);
        }

        public Task<User> AddUserAsync(User user)
        {
            var entity = _context.Users.Add(user);
            return Task.FromResult(entity.Entity);
        }

        public async Task<User?> UpdateUserAsync(Guid tenantId, User user)
        {
            var entity = await _context.Users.FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Id == user.Id);
            if (entity == null) return null;

            entity.Name = user.Name;
            entity.PhoneNumber = user.PhoneNumber;
            entity.RoleId = user.RoleId;
            if (!string.IsNullOrEmpty(user.PasswordHash))
                entity.PasswordHash = user.PasswordHash;
            return entity;
        }

        public async Task<bool> DeleteUserAsync(Guid tenantId, int id)
        {
            var entity = await _context.Users.FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Id == id);
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
