using Moq;
using task1.Application.Services;
using task1.DataLayer.Entities;
using task1.DataLayer.Interfaces;
using Xunit;

namespace task1.Application.Tests;

public class RolesAdminServiceTests
{
    [Fact]
    public async Task GetAllAsync_MapsRoles()
    {
        // Why: success path for role pickers.
        var roles = new List<Role>
        {
            new() { Id = 1, Name = "A", TenantId = TestTenantIds.A },
            new() { Id = 2, Name = "B", TenantId = TestTenantIds.A }
        };
        var mock = new Mock<IRoleRepository>();
        mock.Setup(r => r.GetAllAsync(TestTenantIds.A)).ReturnsAsync(roles);
        var sut = new RolesAdminService(mock.Object);

        var result = await sut.GetAllAsync(TestTenantIds.A);

        Assert.Equal(2, result.Count);
        Assert.Equal("A", result[0].Name);
    }

    [Fact]
    public async Task GetAllAsync_EmptyTenant_ReturnsEmpty()
    {
        // Why: zero/one/many — new tenant may have no custom roles yet.
        var mock = new Mock<IRoleRepository>();
        mock.Setup(r => r.GetAllAsync(TestTenantIds.A)).ReturnsAsync(new List<Role>());
        var sut = new RolesAdminService(mock.Object);

        var result = await sut.GetAllAsync(TestTenantIds.A);

        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateRoleAsync_EmptyName_ThrowsArgumentException()
    {
        // Why: validation before hitting the database.
        var sut = new RolesAdminService(Mock.Of<IRoleRepository>());

        await Assert.ThrowsAsync<ArgumentException>(() => sut.CreateRoleAsync(TestTenantIds.A, "  "));
    }

    [Fact]
    public async Task CreateRoleAsync_DuplicateName_Throws()
    {
        // Why: role names must be unique per tenant.
        var mock = new Mock<IRoleRepository>();
        mock.Setup(r => r.GetByNameAsync("Dup", TestTenantIds.A))
            .ReturnsAsync(new Role { Id = 1, Name = "Dup", TenantId = TestTenantIds.A });
        var sut = new RolesAdminService(mock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.CreateRoleAsync(TestTenantIds.A, "Dup"));
    }

    [Fact]
    public async Task CreateRoleAsync_Success_ReturnsId()
    {
        // Why: callers need the new role id for follow-up claim assignment.
        var mock = new Mock<IRoleRepository>();
        mock.Setup(r => r.GetByNameAsync("New", TestTenantIds.A)).ReturnsAsync((Role?)null);
        mock.Setup(r => r.CreateRoleAsync(It.IsAny<Role>()))
            .ReturnsAsync((Role role) => { role.Id = 50; return role; });
        mock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        var sut = new RolesAdminService(mock.Object);

        var id = await sut.CreateRoleAsync(TestTenantIds.A, "New");

        Assert.Equal(50, id);
    }

    [Fact]
    public async Task SetRoleClaimsAsync_RoleMissing_Throws()
    {
        // Why: security — cannot attach claims to another tenant's or non-existent role.
        var mock = new Mock<IRoleRepository>();
        mock.Setup(r => r.GetByIdAsync(9, TestTenantIds.A)).ReturnsAsync((Role?)null);
        var sut = new RolesAdminService(mock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.SetRoleClaimsAsync(TestTenantIds.A, 9, new[] { 1, 2 }));
    }

    [Fact]
    public async Task SetRoleClaimsAsync_Success_ReplacesAndSaves()
    {
        // Why: happy path updates claim set and persists.
        var mock = new Mock<IRoleRepository>();
        mock.Setup(r => r.GetByIdAsync(1, TestTenantIds.A))
            .ReturnsAsync(new Role { Id = 1, Name = "R", TenantId = TestTenantIds.A });
        mock.Setup(r => r.ReplaceRoleClaimsAsync(1, It.IsAny<IEnumerable<int>>())).Returns(Task.CompletedTask);
        mock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        var sut = new RolesAdminService(mock.Object);

        await sut.SetRoleClaimsAsync(TestTenantIds.A, 1, new[] { 10, 20 });

        mock.Verify(r => r.ReplaceRoleClaimsAsync(1, It.Is<IEnumerable<int>>(x => x.SequenceEqual(new[] { 10, 20 }))), Times.Once);
    }

    [Fact]
    public async Task SetRoleClaimsAsync_InvalidRoleId_ThrowsArgumentOutOfRange()
    {
        // Why: guard against zero/negative identifiers.
        var sut = new RolesAdminService(Mock.Of<IRoleRepository>());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            sut.SetRoleClaimsAsync(TestTenantIds.A, 0, Array.Empty<int>()));
    }

    [Fact]
    public async Task GetRoleNameByIdAsync_Delegates()
    {
        // Why: lightweight lookup for labels.
        var mock = new Mock<IRoleRepository>();
        mock.Setup(r => r.GetRoleNameByIdAsync(3, TestTenantIds.A)).ReturnsAsync("X");
        var sut = new RolesAdminService(mock.Object);

        var name = await sut.GetRoleNameByIdAsync(TestTenantIds.A, 3);

        Assert.Equal("X", name);
    }
}
