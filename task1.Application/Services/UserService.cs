using task1.Application;
using task1.Application.Interfaces;
using task1.Application.Models;
using task1.DataLayer.Entities;
using task1.DataLayer.Interfaces;

namespace task1.Application.Services
{
    public static class UserRoles
    {
        public const string GeneralManager = "General Manager";
        public const string SocialMediaManager = "Social Media Manager";

        public static readonly IReadOnlyList<string> All = new[] { GeneralManager, SocialMediaManager };

        /// <summary>Normalizes role names (e.g. "roleGeneralManager" -> "General Manager").</summary>
        public static string Normalize(string role)
        {
            if (string.IsNullOrWhiteSpace(role)) return role;
            var r = role.Trim();
            if (r.StartsWith("role", StringComparison.OrdinalIgnoreCase))
                r = r.Length > 4 ? r.Substring(4).TrimStart() : r;
            // Map common client forms to display names
            if (string.Equals(r, "GeneralManager", StringComparison.OrdinalIgnoreCase)) return GeneralManager;
            if (string.Equals(r, "SocialMediaManager", StringComparison.OrdinalIgnoreCase)) return SocialMediaManager;
            return r;
        }

        public static bool IsValid(string role) => All.Contains(Normalize(role), StringComparer.Ordinal);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;

        public UserService(IUserRepository userRepository, IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }

        public async Task<List<UserModel>> GetAllAsync(Guid tenantId, string? role = null)
        {
            var normalizedRole = string.IsNullOrWhiteSpace(role) ? null : UserRoles.Normalize(role);
            var users = await _userRepository.GetAllAsync(tenantId, normalizedRole);
            return users.Select(ToModel).ToList();
        }

        public async Task<List<UserModel>> PaginateUsersAsync(Guid tenantId, int page, int pageSize, string? role = null)
        {
            var normalizedRole = string.IsNullOrWhiteSpace(role) ? null : UserRoles.Normalize(role);
            var p = PaginationQuery.NormalizePage(page);
            var ps = PaginationQuery.NormalizePageSize(pageSize);
            var users = await _userRepository.PaginateUsersAsync(tenantId, p, ps, normalizedRole);
            return users.Select(ToModel).ToList();
        }

        public async Task<int> GetCountAsync(Guid tenantId, string? role = null)
        {
            var normalizedRole = string.IsNullOrWhiteSpace(role) ? null : UserRoles.Normalize(role);
            return await _userRepository.GetCountAsync(tenantId, normalizedRole);
        }

        public async Task<UserModel?> GetByIdAsync(Guid tenantId, int id)
        {
            var user = await _userRepository.GetByIdAsync(tenantId, id);
            if (user == null) return null;
            return ToModel(user);
        }

        public async Task<UserModel> AddUserAsync(Guid tenantId, CreateUserModel model)
        {
            var roleName = UserRoles.Normalize(model.Role);
            var role = await _roleRepository.GetByNameAsync(roleName, tenantId);
            if (role == null)
                throw new InvalidOperationException($"Role '{roleName}' not found in database.");

            var phoneNumber = model.PhoneNumber?.Trim() ?? string.Empty;
            if (await _userRepository.GetByPhoneNumberAsync(tenantId, phoneNumber) != null)
                throw new InvalidOperationException("A user with this phone number already exists.");

            var user = new User
            {
                Name = model.Name?.Trim() ?? string.Empty,
                PhoneNumber = phoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                RoleId = role.Id,
                TenantId = tenantId
            };

            var added = await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();
            return ToModel(added);
        }

        public async Task<bool> DeleteUserAsync(Guid tenantId, int id)
        {
            var deleted = await _userRepository.DeleteUserAsync(tenantId, id);
            if (!deleted) return false;
            await _userRepository.SaveChangesAsync();
            return true;
        }

        public async Task<UserModel?> EditUserAsync(Guid tenantId, int id, UserModel model)
        {
            var entity = await _userRepository.GetByIdAsync(tenantId, id);
            if (entity == null) return null;

            var newPhoneNumber = model.PhoneNumber?.Trim() ?? string.Empty;
            var existingByPhone = await _userRepository.GetByPhoneNumberAsync(tenantId, newPhoneNumber);
            if (existingByPhone != null && existingByPhone.Id != id)
                throw new InvalidOperationException("A user with this phone number already exists.");

            entity.Name = model.Name?.Trim() ?? string.Empty;
            entity.PhoneNumber = newPhoneNumber;
            var updated = await _userRepository.UpdateUserAsync(tenantId, entity);
            if (updated == null) return null;
            await _userRepository.SaveChangesAsync();
            return ToModel(updated);
        }

        public async Task<UserModel?> ChangeRoleAsync(Guid tenantId, int id, string role)
        {
            var roleName = UserRoles.Normalize(role);
            var roleEntity = await _roleRepository.GetByNameAsync(roleName, tenantId);
            if (roleEntity == null)
                throw new InvalidOperationException($"Role '{roleName}' not found in database.");

            var entity = await _userRepository.GetByIdAsync(tenantId, id);
            if (entity == null) return null;

            entity.RoleId = roleEntity.Id;
            var updated = await _userRepository.UpdateUserAsync(tenantId, entity);
            if (updated == null) return null;
            await _userRepository.SaveChangesAsync();
            return ToModel(updated);
        }

        private static UserModel ToModel(User user) => new UserModel
        {
            Id = user.Id,
            Name = user.Name,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role?.Name ?? string.Empty
        };
    }
}
