using task1.Application.Interfaces;
using task1.Application.Models;
using task1.DataLayer.Entities;
using task1.DataLayer.Interfaces;
using System.Text;

namespace task1.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly IRoleClaimsService _roleClaimsService;
        private readonly ITokenService _tokenService;

        public AuthService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            ITenantRepository tenantRepository,
            IRoleClaimsService roleClaimsService,
            ITokenService tokenService)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _tenantRepository = tenantRepository;
            _roleClaimsService = roleClaimsService;
            _tokenService = tokenService;
        }

        public async Task<AuthPermissionsResultModel> GetPermissionsAsync(string? roleName, Guid tenantId)
        {
            if (string.IsNullOrWhiteSpace(roleName) || tenantId == Guid.Empty)
                return new AuthPermissionsResultModel();

            var claimNames = await _roleClaimsService.GetClaimNamesForRoleAsync(roleName, tenantId);
            var allClaims = await _roleRepository.GetAllClaimsAsync();
            var nameSet = new HashSet<string>(claimNames, StringComparer.OrdinalIgnoreCase);
            var claims = allClaims
                .Where(c => nameSet.Contains(c.Name))
                .Select(c => new AuthClaimModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Category = c.Category
                })
                .ToList();

            return new AuthPermissionsResultModel { Claims = claims };
        }

        public async Task<AuthLoginResultModel?> LoginAsync(string phoneNumber, string password)
        {
            var normalizedPhoneNumber = NormalizePhoneNumber(phoneNumber);
            var user = await _userRepository.GetByPhoneNumberAsync(normalizedPhoneNumber);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            var tenant = await _tenantRepository.GetByIdAsync(user.TenantId);
            if (tenant == null || !tenant.Status)
                return null;

            var roleName = user.Role?.Name;
            if (string.IsNullOrWhiteSpace(roleName))
                return null;
            var token = _tokenService.GenerateToken(user.Id.ToString(), user.PhoneNumber, user.Name, roleName, user.TenantId);
            return new AuthLoginResultModel { Token = token };
        }



        public async Task<AuthRegisterResultModel> RegisterAsync(
            string organizationName,
            string? firstName,
            string? lastName,
            string phoneNumber,
            string password)
        {
            var normalizedOrganizationName = organizationName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalizedOrganizationName))
                throw new InvalidOperationException("Organization name is required.");

            var existingTenant = await _tenantRepository.GetByNameAsync(normalizedOrganizationName);
            if (existingTenant != null)
                throw new InvalidOperationException("Organization name is already registered.");

            var normalizedPhoneNumber = NormalizePhoneNumber(phoneNumber);
            var existingUser = await _userRepository.GetByPhoneNumberAsync(normalizedPhoneNumber);
            if (existingUser != null)
                throw new InvalidOperationException("Phone number is already registered.");

            var tenant = new Tenant
            {
                Name = normalizedOrganizationName,
                Status = true,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            };
            await _tenantRepository.CreateAsync(tenant);
            await _tenantRepository.SaveChangesAsync();

            var adminRole = new Role
            {
                Name = "Admin",
                TenantId = tenant.TenantId
            };
            var createdRole = await _roleRepository.CreateRoleAsync(adminRole);
            await _roleRepository.SaveChangesAsync();

            var claimIds = (await _roleRepository.GetAllClaimsAsync())
                .Select(c => c.Id)
                .ToList();
            await _roleRepository.ReplaceRoleClaimsAsync(createdRole.Id, claimIds);
            await _roleRepository.SaveChangesAsync();

            var fullName = $"{firstName?.Trim()} {lastName?.Trim()}".Trim();
            if (string.IsNullOrWhiteSpace(fullName))
                fullName = normalizedPhoneNumber;

            var newUser = new User
            {
                PhoneNumber = normalizedPhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Name = fullName,
                RoleId = createdRole.Id,
                TenantId = tenant.TenantId
            };

            var createdUser = await _userRepository.AddUserAsync(newUser);
            await _userRepository.SaveChangesAsync();

            var token = _tokenService.GenerateToken(
                createdUser.Id.ToString(),
                createdUser.PhoneNumber,
                createdUser.Name,
                createdRole.Name,
                createdUser.TenantId);

            return new AuthRegisterResultModel { Token = token };
        }

        private static string NormalizePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return string.Empty;

            var trimmed = phoneNumber.Trim();
            var builder = new StringBuilder(trimmed.Length);
            foreach (var c in trimmed)
            {
                if (char.IsDigit(c) || c == '+')
                    builder.Append(c);
            }

            var normalized = builder.ToString();
            if (normalized.StartsWith("++", StringComparison.Ordinal))
                normalized = "+" + normalized.TrimStart('+');

            return normalized;
        }
    }
}
