using Moq;
using task1.Application.Interfaces;
using task1.Application.Models;
using task1.Application.Services;
using task1.DataLayer.Entities;
using task1.DataLayer.Interfaces;
using Xunit;

namespace task1.Application.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        // Why: success path for authentication must issue a token when tenant is active.
        var tid = TestTenantIds.A;
        var user = new User
        {
            Id = 1,
            TenantId = tid,
            PhoneNumber = "+1555",
            Name = "U",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("pw"),
            Role = new Role { Id = 1, Name = "Admin", TenantId = tid }
        };
        var tenant = new Tenant { TenantId = tid, Name = "Org", Status = true };

        var mockUser = new Mock<IUserRepository>();
        mockUser.Setup(r => r.GetByPhoneNumberAsync("+1555")).ReturnsAsync(user);
        var mockRole = new Mock<IRoleRepository>();
        var mockTenant = new Mock<ITenantRepository>();
        mockTenant.Setup(r => r.GetByIdAsync(tid)).ReturnsAsync(tenant);
        var mockClaims = new Mock<IRoleClaimsService>();
        var mockToken = new Mock<ITokenService>();
        mockToken.Setup(t => t.GenerateToken("1", "+1555", "U", "Admin", tid)).Returns("jwt-here");

        var sut = new AuthService(mockUser.Object, mockRole.Object, mockTenant.Object, mockClaims.Object, mockToken.Object);

        var result = await sut.LoginAsync("+1555", "pw");

        Assert.NotNull(result);
        Assert.Equal("jwt-here", result!.Token);
    }

    [Fact]
    public async Task LoginAsync_BadPassword_ReturnsNull()
    {
        // Why: invalid credentials must not leak whether the phone exists.
        var user = new User
        {
            Id = 1,
            TenantId = TestTenantIds.A,
            PhoneNumber = "+1",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("good"),
            Role = new Role { Name = "Admin", TenantId = TestTenantIds.A }
        };
        var mockUser = new Mock<IUserRepository>();
        mockUser.Setup(r => r.GetByPhoneNumberAsync("+1")).ReturnsAsync(user);
        var mockRole = new Mock<IRoleRepository>();
        var mockTenant = new Mock<ITenantRepository>();
        var mockClaims = new Mock<IRoleClaimsService>();
        var mockToken = new Mock<ITokenService>();
        var sut = new AuthService(mockUser.Object, mockRole.Object, mockTenant.Object, mockClaims.Object, mockToken.Object);

        var result = await sut.LoginAsync("+1", "wrong");

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_TenantInactive_ReturnsNull()
    {
        // Why: suspended orgs must not log in even with valid password.
        var tid = TestTenantIds.A;
        var user = new User
        {
            Id = 1,
            TenantId = tid,
            PhoneNumber = "+1",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("pw"),
            Role = new Role { Name = "Admin", TenantId = tid }
        };
        var mockUser = new Mock<IUserRepository>();
        mockUser.Setup(r => r.GetByPhoneNumberAsync("+1")).ReturnsAsync(user);
        var mockTenant = new Mock<ITenantRepository>();
        mockTenant.Setup(r => r.GetByIdAsync(tid)).ReturnsAsync(new Tenant { TenantId = tid, Status = false });
        var sut = new AuthService(mockUser.Object, Mock.Of<IRoleRepository>(), mockTenant.Object,
            Mock.Of<IRoleClaimsService>(), Mock.Of<ITokenService>());

        var result = await sut.LoginAsync("+1", "pw");

        Assert.Null(result);
    }

    [Fact]
    public async Task RegisterAsync_EmptyOrg_Throws()
    {
        // Why: validation before touching persistence.
        var sut = new AuthService(Mock.Of<IUserRepository>(), Mock.Of<IRoleRepository>(), Mock.Of<ITenantRepository>(),
            Mock.Of<IRoleClaimsService>(), Mock.Of<ITokenService>());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.RegisterAsync("   ", "f", "l", "+1", "p"));
    }

    [Fact]
    public async Task RegisterAsync_DuplicateOrg_Throws()
    {
        // Why: unique organization names for multi-tenant signup.
        var tid = TestTenantIds.A;
        var mockTenant = new Mock<ITenantRepository>();
        mockTenant.Setup(r => r.GetByNameAsync("Acme")).ReturnsAsync(new Tenant { TenantId = tid, Name = "Acme" });
        var sut = new AuthService(Mock.Of<IUserRepository>(), Mock.Of<IRoleRepository>(), mockTenant.Object,
            Mock.Of<IRoleClaimsService>(), Mock.Of<ITokenService>());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.RegisterAsync("Acme", "f", "l", "+1999888777", "p"));
    }

    [Fact]
    public async Task RegisterAsync_DuplicatePhone_Throws()
    {
        // Why: phone is unique across tenants for login lookup.
        var mockTenant = new Mock<ITenantRepository>();
        mockTenant.Setup(r => r.GetByNameAsync("Acme")).ReturnsAsync((Tenant?)null);
        var mockUser = new Mock<IUserRepository>();
        mockUser.Setup(r => r.GetByPhoneNumberAsync("+1")).ReturnsAsync(new User { Id = 1 });
        var sut = new AuthService(mockUser.Object, Mock.Of<IRoleRepository>(), mockTenant.Object,
            Mock.Of<IRoleClaimsService>(), Mock.Of<ITokenService>());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.RegisterAsync("Acme", "f", "l", "+1", "p"));
    }

    [Fact]
    public async Task RegisterAsync_Success_CreatesTenantRoleUserAndToken()
    {
        // Why: full registration pipeline must complete all steps and return JWT.
        var newTenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var tenantEntity = new Tenant { Name = "NewCo", Status = true };
        var mockTenant = new Mock<ITenantRepository>();
        mockTenant.Setup(r => r.GetByNameAsync("NewCo")).ReturnsAsync((Tenant?)null);
        mockTenant.Setup(r => r.CreateAsync(It.IsAny<Tenant>())).ReturnsAsync((Tenant t) =>
        {
            t.TenantId = newTenantId;
            return t;
        });
        mockTenant.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var mockUser = new Mock<IUserRepository>();
        mockUser.Setup(r => r.GetByPhoneNumberAsync("+1888")).ReturnsAsync((User?)null);
        mockUser.Setup(r => r.AddUserAsync(It.IsAny<User>())).ReturnsAsync((User u) => { u.Id = 7; return u; });
        mockUser.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var createdRole = new Role { Id = 99, Name = "Admin", TenantId = newTenantId };
        var mockRole = new Mock<IRoleRepository>();
        mockRole.Setup(r => r.CreateRoleAsync(It.IsAny<Role>())).ReturnsAsync(createdRole);
        mockRole.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        var claims = new List<Claim>
        {
            new() { Id = 1, Name = "c1", Category = "cat" },
            new() { Id = 2, Name = "c2", Category = "cat" }
        };
        mockRole.Setup(r => r.GetAllClaimsAsync()).ReturnsAsync(claims);
        mockRole.Setup(r => r.ReplaceRoleClaimsAsync(99, It.IsAny<IEnumerable<int>>())).Returns(Task.CompletedTask);

        var mockToken = new Mock<ITokenService>();
        mockToken.Setup(t => t.GenerateToken("7", "+1888", It.IsAny<string>(), "Admin", newTenantId)).Returns("reg-token");

        var sut = new AuthService(mockUser.Object, mockRole.Object, mockTenant.Object,
            Mock.Of<IRoleClaimsService>(), mockToken.Object);

        var result = await sut.RegisterAsync("NewCo", "F", "L", "+1888", "secret");

        Assert.Equal("reg-token", result.Token);
        mockRole.Verify(r => r.ReplaceRoleClaimsAsync(99, It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(new[] { 1, 2 }))), Times.Once);
    }

    [Fact]
    public async Task GetPermissionsAsync_EmptyTenant_ReturnsEmptyClaims()
    {
        // Why: unauthenticated or bad context should not throw and yields no permissions.
        var sut = new AuthService(Mock.Of<IUserRepository>(), Mock.Of<IRoleRepository>(), Mock.Of<ITenantRepository>(),
            Mock.Of<IRoleClaimsService>(), Mock.Of<ITokenService>());

        var result = await sut.GetPermissionsAsync("Admin", Guid.Empty);

        Assert.Empty(result.Claims);
    }

    [Fact]
    public async Task GetPermissionsAsync_ValidRole_MapsIntersectingClaims()
    {
        // Why: JWT authorization must only include claims assigned to the role.
        var mockClaimsSvc = new Mock<IRoleClaimsService>();
        mockClaimsSvc.Setup(s => s.GetClaimNamesForRoleAsync("Admin", TestTenantIds.A))
            .ReturnsAsync(new List<string> { "ClaimA" });
        var allClaims = new List<Claim>
        {
            new() { Id = 1, Name = "ClaimA", Category = "X" },
            new() { Id = 2, Name = "Other", Category = "Y" }
        };
        var mockRole = new Mock<IRoleRepository>();
        mockRole.Setup(r => r.GetAllClaimsAsync()).ReturnsAsync(allClaims);

        var sut = new AuthService(Mock.Of<IUserRepository>(), mockRole.Object, Mock.Of<ITenantRepository>(),
            mockClaimsSvc.Object, Mock.Of<ITokenService>());

        var result = await sut.GetPermissionsAsync("Admin", TestTenantIds.A);

        Assert.Single(result.Claims);
        Assert.Equal("ClaimA", result.Claims[0].Name);
    }
}
