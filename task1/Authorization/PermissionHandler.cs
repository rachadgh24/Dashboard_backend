using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using task1.Application.Interfaces;

namespace task1.Authorization
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IMemoryCache _cache;
        private readonly IRoleClaimsService _roleClaimsService;

        public PermissionHandler(IMemoryCache cache, IRoleClaimsService roleClaimsService)
        {
            _cache = cache;
            _roleClaimsService = roleClaimsService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var roleName = context.User.FindFirst("role")?.Value;
            if (string.IsNullOrWhiteSpace(roleName)) return;

            var cacheKey = $"roleClaims:{roleName}";
            var claimsSet = await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                var claimNames = await _roleClaimsService.GetClaimNamesForRoleAsync(roleName);
                return new HashSet<string>(claimNames, StringComparer.Ordinal);
            });

            if (claimsSet != null && claimsSet.Contains(requirement.Permission))
                context.Succeed(requirement);
        }
    }
}

