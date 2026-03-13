using task1.Application.Interfaces;
using task1.Application.Models;
using task1.DataLayer.Entities;
using task1.DataLayer.Interfaces;

namespace task1.Application.Services
{
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string GeneralManager = "General Manager";
        public const string SocialMediaManager = "Social Media Manager";

        public static readonly IReadOnlyList<string> All = new[] { Admin, GeneralManager, SocialMediaManager };

        /// <summary>Normalizes role (e.g. "roleAdmin" -> "Admin") so we always store and return display names.</summary>
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

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<UserModel>> GetAllAsync(string? role = null)
        {
            var normalizedRole = string.IsNullOrWhiteSpace(role) ? null : UserRoles.Normalize(role);
            var users = await _userRepository.GetAllAsync(normalizedRole);
            return users.Select(ToModel).ToList();
        }

        public async Task<List<UserModel>> PaginateUsersAsync(int page)
        {
            var users = await _userRepository.PaginateUsersAsync(page);
            return users.Select(ToModel).ToList();
        }

        public async Task<int> GetCountAsync()
        {
            return await _userRepository.GetCountAsync();
        }

        public async Task<UserModel?> GetByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;
            return ToModel(user);
        }

        public async Task<UserModel> AddUserAsync(CreateUserModel model)
        {
            var role = UserRoles.Normalize(model.Role);
            if (!UserRoles.All.Contains(role, StringComparer.Ordinal))
                throw new ArgumentException($"Role must be one of: {string.Join(", ", UserRoles.All)}", nameof(model));

            var email = model.Email?.Trim() ?? string.Empty;
            if (await _userRepository.GetByEmailAsync(email) != null)
                throw new InvalidOperationException("A user with this email already exists.");

            var user = new User
            {
                Name = model.Name?.Trim() ?? string.Empty,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = role
            };

            var added = await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();
            return ToModel(added);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var deleted = await _userRepository.DeleteUserAsync(id);
            if (!deleted) return false;
            await _userRepository.SaveChangesAsync();
            return true;
        }

        public async Task<UserModel?> EditUserAsync(int id, UserModel model)
        {
            var entity = await _userRepository.GetByIdAsync(id);
            if (entity == null) return null;

            var newEmail = model.Email?.Trim() ?? string.Empty;
            var existingByEmail = await _userRepository.GetByEmailAsync(newEmail);
            if (existingByEmail != null && existingByEmail.Id != id)
                throw new InvalidOperationException("A user with this email already exists.");

            entity.Name = model.Name?.Trim() ?? string.Empty;
            entity.Email = newEmail;
            var updated = await _userRepository.UpdateUserAsync(entity);
            if (updated == null) return null;
            await _userRepository.SaveChangesAsync();
            return ToModel(updated);
        }

        public async Task<UserModel?> ChangeRoleAsync(int id, string role)
        {
            var normalizedRole = UserRoles.Normalize(role);
            if (!UserRoles.All.Contains(normalizedRole, StringComparer.Ordinal))
                throw new ArgumentException($"Role must be one of: {string.Join(", ", UserRoles.All)}", nameof(role));

            var entity = await _userRepository.GetByIdAsync(id);
            if (entity == null) return null;

            entity.Role = normalizedRole;
            var updated = await _userRepository.UpdateUserAsync(entity);
            if (updated == null) return null;
            await _userRepository.SaveChangesAsync();
            return ToModel(updated);
        }

        private static UserModel ToModel(User user) => new UserModel
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role
        };
    }
}
