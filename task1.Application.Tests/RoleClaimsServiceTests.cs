using Moq;
using task1.Application.Services;
using task1.DataLayer.Interfaces;
using Xunit;

namespace task1.Application.Tests;

public class RoleClaimsServiceTests
{
    [Fact]
    public async Task GetClaimNamesForRoleAsync_ByNameOnly_Delegates()
    {
        // Why: legacy overload used where tenant is implicit.
        var mock = new Mock<IRoleRepository>();
        mock.Setup(r => r.GetClaimNamesByRoleNameAsync("Admin")).ReturnsAsync(new List<string> { "a", "b" });
        var sut = new RoleClaimsService(mock.Object);

        var result = await sut.GetClaimNamesForRoleAsync("Admin");

        Assert.Equal(new[] { "a", "b" }, result);
    }

    [Fact]
    public async Task GetClaimNamesForRoleAsync_WithTenant_Delegates()
    {
        // Why: tenant-scoped roles must resolve claims within that org only.
        var mock = new Mock<IRoleRepository>();
        mock.Setup(r => r.GetClaimNamesByRoleNameAsync("Admin", TestTenantIds.A))
            .ReturnsAsync(new List<string> { "c" });
        var sut = new RoleClaimsService(mock.Object);

        var result = await sut.GetClaimNamesForRoleAsync("Admin", TestTenantIds.A);

        Assert.Single(result);
        Assert.Equal("c", result[0]);
    }

    [Fact]
    public async Task GetClaimNamesForRoleAsync_EmptyResult_ReturnsEmpty()
    {
        // Why: role with no mapped claims should not throw.
        var mock = new Mock<IRoleRepository>();
        mock.Setup(r => r.GetClaimNamesByRoleNameAsync("X", TestTenantIds.A))
            .ReturnsAsync(new List<string>());
        var sut = new RoleClaimsService(mock.Object);

        var result = await sut.GetClaimNamesForRoleAsync("X", TestTenantIds.A);

        Assert.Empty(result);
    }
}
