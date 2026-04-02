using Microsoft.EntityFrameworkCore;
using Moq;
using task1.Application.Models;
using task1.Application.Services;
using task1.DataLayer.Entities;
using task1.DataLayer.Interfaces;
using Xunit;

namespace task1.Application.Tests;

public class UserServiceTests
{
    private static Role RoleEntity(int id, string name, Guid tid) =>
        new() { Id = id, Name = name, TenantId = tid };

    private static User UserEntity(int id, Guid tid, int roleId, string phone = "+100", string name = "U") =>
        new()
        {
            Id = id,
            TenantId = tid,
            RoleId = roleId,
            PhoneNumber = phone,
            Name = name,
            PasswordHash = "hash",
            Role = RoleEntity(roleId, UserRoles.GeneralManager, tid)
        };

    [Fact]
    public async Task GetAllAsync_WithRoleFilter_NormalizesAndDelegates()
    {
        // Why: role query params from clients may be normalized before hitting the repository.
        var users = new List<User> { UserEntity(1, TestTenantIds.A, 1) };
        var mockUser = new Mock<IUserRepository>();
        mockUser.Setup(r => r.GetAllAsync(TestTenantIds.A, UserRoles.GeneralManager)).ReturnsAsync(users);
        var mockRole = new Mock<IRoleRepository>();
        var sut = new UserService(mockUser.Object, mockRole.Object);

        var result = await sut.GetAllAsync(TestTenantIds.A, "roleGeneralManager");

        Assert.Single(result);
        Assert.Equal(UserRoles.GeneralManager, result[0].Role);
    }

    [Fact]
    public async Task PaginateUsersAsync_EmptyPage_ReturnsEmpty()
    {
        // Why: zero/one/many — empty page at high page index.
        var mockUser = new Mock<IUserRepository>();
        mockUser.Setup(r => r.PaginateUsersAsync(TestTenantIds.A, 50, 10, null)).ReturnsAsync(new List<User>());
        var mockRole = new Mock<IRoleRepository>();
        var sut = new UserService(mockUser.Object, mockRole.Object);

        var result = await sut.PaginateUsersAsync(TestTenantIds.A, 50, 10);

        Assert.Empty(result);
    }

    [Fact]
    public async Task PaginateUsersAsync_OneUser_ReturnsOne()
    {
        // Why: single-record pagination still maps correctly.
        var users = new List<User> { UserEntity(1, TestTenantIds.A, 1) };
        var mockUser = new Mock<IUserRepository>();
        mockUser.Setup(r => r.PaginateUsersAsync(TestTenantIds.A, 1, 10, null)).ReturnsAsync(users);
        var mockRole = new Mock<IRoleRepository>();
        var sut = new UserService(mockUser.Object, mockRole.Object);

        var result = await sut.PaginateUsersAsync(TestTenantIds.A, 1, 10);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetByIdAsync_WrongTenant_ReturnsNull()
    {
        // Why: tenant isolation when repository has no matching user.
        var mockUser = new Mock<IUserRepository>();
        mockUser.Setup(r => r.GetByIdAsync(TestTenantIds.B, 1)).ReturnsAsync((User?)null);
        var mockRole = new Mock<IRoleRepository>();
        var sut = new UserService(mockUser.Object, mockRole.Object);

        var result = await sut.GetByIdAsync(TestTenantIds.B, 1);

        Assert.Null(result);
    }

    [Fact]
    public async Task AddUserAsync_RoleMissing_Throws()
    {
        // Why: security/data integrity — cannot assign unknown role.
        var mockUser = new Mock<IUserRepository>();
        var mockRole = new Mock<IRoleRepository>();
        mockRole.Setup(r => r.GetByNameAsync(UserRoles.SocialMediaManager, TestTenantIds.A))
            .ReturnsAsync((Role?)null);
        var sut = new UserService(mockUser.Object, mockRole.Object);
        var model = new CreateUserModel
        {
            Name = "A",
            PhoneNumber = "+1999",
            Password = "secret",
            Role = UserRoles.SocialMediaManager
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.AddUserAsync(TestTenantIds.A, model));
    }

    [Fact]
    public async Task AddUserAsync_DuplicatePhone_Throws()
    {
        // Why: prevents duplicate identity for login.
        var role = RoleEntity(1, UserRoles.GeneralManager, TestTenantIds.A);
        var mockUser = new Mock<IUserRepository>();
        mockUser.Setup(r => r.GetByPhoneNumberAsync(TestTenantIds.A, "+1")).ReturnsAsync(UserEntity(9, TestTenantIds.A, 1, "+1"));
        var mockRole = new Mock<IRoleRepository>();
        mockRole.Setup(r => r.GetByNameAsync(UserRoles.GeneralManager, TestTenantIds.A)).ReturnsAsync(role);
        var sut = new UserService(mockUser.Object, mockRole.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.AddUserAsync(TestTenantIds.A, new CreateUserModel
            {
                Name = "New",
                PhoneNumber = "+1",
                Password = "p",
                Role = UserRoles.GeneralManager
            }));
    }

    [Fact]
    public async Task AddUserAsync_Success_Saves()
    {
        // Why: happy path creates user with hashed password path exercised.
        var role = RoleEntity(1, UserRoles.GeneralManager, TestTenantIds.A);
        var mockUser = new Mock<IUserRepository>();
        mockUser.Setup(r => r.GetByPhoneNumberAsync(TestTenantIds.A, "+200")).ReturnsAsync((User?)null);
        mockUser.Setup(r => r.AddUserAsync(It.IsAny<User>())).ReturnsAsync((User u) => { u.Id = 5; return u; });
        mockUser.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        var mockRole = new Mock<IRoleRepository>();
        mockRole.Setup(r => r.GetByNameAsync(UserRoles.GeneralManager, TestTenantIds.A)).ReturnsAsync(role);
        var sut = new UserService(mockUser.Object, mockRole.Object);

        var result = await sut.AddUserAsync(TestTenantIds.A, new CreateUserModel
        {
            Name = "N",
            PhoneNumber = "+200",
            Password = "pw",
            Role = UserRoles.GeneralManager
        });

        Assert.Equal(5, result.Id);
        mockUser.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task EditUserAsync_PhoneTakenByOtherUser_Throws()
    {
        // Why: concurrent uniqueness rule for phone numbers on update.
        var self = UserEntity(1, TestTenantIds.A, 1, "+1");
        var other = UserEntity(2, TestTenantIds.A, 1, "+2");
        var mockUser = new Mock<IUserRepository>();
        mockUser.Setup(r => r.GetByIdAsync(TestTenantIds.A, 1)).ReturnsAsync(self);
        mockUser.Setup(r => r.GetByPhoneNumberAsync(TestTenantIds.A, "+2")).ReturnsAsync(other);
        var mockRole = new Mock<IRoleRepository>();
        var sut = new UserService(mockUser.Object, mockRole.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.EditUserAsync(TestTenantIds.A, 1, new UserModel { Name = "X", PhoneNumber = "+2", Role = "r" }));
    }

    [Fact]
    public async Task EditUserAsync_UpdateReturnsNull_DoesNotSave()
    {
        // Why: race — user removed between read and update.
        var self = UserEntity(1, TestTenantIds.A, 1, "+1");
        var mockUser = new Mock<IUserRepository>();
        mockUser.Setup(r => r.GetByIdAsync(TestTenantIds.A, 1)).ReturnsAsync(self);
        mockUser.Setup(r => r.GetByPhoneNumberAsync(TestTenantIds.A, "+1")).ReturnsAsync(self);
        mockUser.Setup(r => r.UpdateUserAsync(TestTenantIds.A, It.IsAny<User>())).ReturnsAsync((User?)null);
        var mockRole = new Mock<IRoleRepository>();
        var sut = new UserService(mockUser.Object, mockRole.Object);

        var result = await sut.EditUserAsync(TestTenantIds.A, 1, new UserModel { Name = "X", PhoneNumber = "+1" });

        Assert.Null(result);
        mockUser.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task EditUserAsync_SaveThrowsConcurrency_Propagates()
    {
        // Why: lost update detection at persistence layer.
        var self = UserEntity(1, TestTenantIds.A, 1, "+1");
        var mockUser = new Mock<IUserRepository>();
        mockUser.Setup(r => r.GetByIdAsync(TestTenantIds.A, 1)).ReturnsAsync(self);
        mockUser.Setup(r => r.GetByPhoneNumberAsync(TestTenantIds.A, "+1")).ReturnsAsync(self);
        mockUser.Setup(r => r.UpdateUserAsync(TestTenantIds.A, It.IsAny<User>())).ReturnsAsync(self);
        mockUser.Setup(r => r.SaveChangesAsync()).ThrowsAsync(new DbUpdateConcurrencyException());
        var mockRole = new Mock<IRoleRepository>();
        var sut = new UserService(mockUser.Object, mockRole.Object);

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
            sut.EditUserAsync(TestTenantIds.A, 1, new UserModel { Name = "X", PhoneNumber = "+1" }));
    }

    [Fact]
    public async Task ChangeRoleAsync_InvalidRole_Throws()
    {
        // Why: cannot switch to non-existent role.
        var mockUser = new Mock<IUserRepository>();
        var mockRole = new Mock<IRoleRepository>();
        mockRole.Setup(r => r.GetByNameAsync("Nope", TestTenantIds.A)).ReturnsAsync((Role?)null);
        var sut = new UserService(mockUser.Object, mockRole.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.ChangeRoleAsync(TestTenantIds.A, 1, "Nope"));
    }

    [Fact]
    public async Task DeleteUserAsync_Success_CallsRepository()
    {
        // Why: soft-delete orchestration for users.
        var mockUser = new Mock<IUserRepository>();
        mockUser.Setup(r => r.DeleteUserAsync(TestTenantIds.A, 1)).ReturnsAsync(true);
        mockUser.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        var mockRole = new Mock<IRoleRepository>();
        var sut = new UserService(mockUser.Object, mockRole.Object);

        var ok = await sut.DeleteUserAsync(TestTenantIds.A, 1);

        Assert.True(ok);
        mockUser.Verify(r => r.DeleteUserAsync(TestTenantIds.A, 1), Times.Once);
        mockUser.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetCountAsync_Delegates()
    {
        // Why: paging header totals.
        var mockUser = new Mock<IUserRepository>();
        mockUser.Setup(r => r.GetCountAsync(TestTenantIds.A, null)).ReturnsAsync(3);
        var mockRole = new Mock<IRoleRepository>();
        var sut = new UserService(mockUser.Object, mockRole.Object);

        Assert.Equal(3, await sut.GetCountAsync(TestTenantIds.A));
    }
}
