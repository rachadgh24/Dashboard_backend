using Moq;
using task1.Application.Services;
using task1.DataLayer.Entities;
using task1.DataLayer.Interfaces;
using Xunit;

namespace task1.Application.Tests;

public class ClaimsAdminServiceTests
{
    [Fact]
    public async Task GetAllAsync_Empty_ReturnsEmpty()
    {
        // Why: zero claims is valid before seeding.
        var mock = new Mock<IRoleRepository>();
        mock.Setup(r => r.GetAllClaimsAsync()).ReturnsAsync(new List<Claim>());
        var sut = new ClaimsAdminService(mock.Object);

        var result = await sut.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_Many_MapsToAdminDtos()
    {
        // Why: admin UI lists id, name, category for permission matrix.
        var claims = new List<Claim>
        {
            new() { Id = 1, Name = "ViewX", Category = "Customers" },
            new() { Id = 2, Name = "EditY", Category = "Users" }
        };
        var mock = new Mock<IRoleRepository>();
        mock.Setup(r => r.GetAllClaimsAsync()).ReturnsAsync(claims);
        var sut = new ClaimsAdminService(mock.Object);

        var result = await sut.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("ViewX", result[0].Name);
        Assert.Equal("Customers", result[0].Category);
    }
}
