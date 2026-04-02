using Moq;
using task1.Application.Models;
using task1.Application.Services;
using task1.DataLayer.Entities;
using task1.DataLayer.Interfaces;
using Xunit;

namespace task1.Application.Tests;

public class CustomerServiceTests
{
    private static Customer Cust(int id, Guid tid, string name = "N", string last = "L", string city = "C", string email = "e@x.com") =>
        new()
        {
            Id = id,
            TenantId = tid,
            Name = name,
            LastName = last,
            City = city,
            Email = email,
            Cars = new List<Car>()
        };

    [Fact]
    public async Task GetAllAsync_ManyCustomers_MapsAll()
    {
        // Why: success path for full customer list screens.
        var list = new List<Customer> { Cust(1, TestTenantIds.A), Cust(2, TestTenantIds.A) };
        var mock = new Mock<ICustomerRepository>();
        mock.Setup(r => r.GetAll(TestTenantIds.A)).Returns(list.BuildMock());
        var sut = new CustomerService(mock.Object);

        var result = await sut.GetAllAsync(TestTenantIds.A);

        Assert.Equal(2, result.Count);
        Assert.Equal("N", result[0].Name);
    }

    [Fact]
    public async Task GetAllAsync_Empty_ReturnsEmpty()
    {
        // Why: zero customers is valid for a new org.
        var mock = new Mock<ICustomerRepository>();
        mock.Setup(r => r.GetAll(TestTenantIds.A)).Returns(new List<Customer>().BuildMock());
        var sut = new CustomerService(mock.Object);

        var result = await sut.GetAllAsync(TestTenantIds.A);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_OtherTenant_ReturnsNull()
    {
        // Why: tenant isolation — no row for that tenant/id pair.
        var list = new List<Customer> { Cust(1, TestTenantIds.A) };
        var mock = new Mock<ICustomerRepository>();
        mock.Setup(r => r.GetById(TestTenantIds.B, 1)).Returns(
            list.Where(c => c.TenantId == TestTenantIds.B && c.Id == 1).BuildMock());
        var sut = new CustomerService(mock.Object);

        var result = await sut.GetByIdAsync(TestTenantIds.B, 1);

        Assert.Null(result);
    }

    [Fact]
    public async Task AddCustomerAsync_SavesAndReturns()
    {
        // Why: create must persist and echo persisted fields.
        var mock = new Mock<ICustomerRepository>();
        mock.Setup(r => r.AddCustomerAsync(It.IsAny<Customer>()))
            .ReturnsAsync((Customer c) => { c.Id = 10; return c; });
        mock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        var sut = new CustomerService(mock.Object);
        var model = new CustomerModel
        {
            Name = "A",
            LastName = "B",
            City = "C",
            Email = "a@b.c"
        };

        var result = await sut.AddCustomerAsync(TestTenantIds.A, model);

        Assert.Equal(10, result.Id);
        mock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateCustomerAsync_RepositoryReturnsNull_DoesNotSave()
    {
        // Why: race — concurrent delete means update finds nothing at commit time.
        var mock = new Mock<ICustomerRepository>();
        mock.Setup(r => r.UpdateCustomer(TestTenantIds.A, 1, It.IsAny<Customer>())).ReturnsAsync((Customer?)null);
        var sut = new CustomerService(mock.Object);

        var result = await sut.UpdateCustomerAsync(TestTenantIds.A, 1,
            new CustomerModel { Name = "X", LastName = "Y", City = "Z", Email = "x@y.z" });

        Assert.Null(result);
        mock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteCustomerAsync_Success_CallsDeleteAndSave()
    {
        // Why: soft-delete orchestration in Application layer.
        var mock = new Mock<ICustomerRepository>();
        mock.Setup(r => r.DeleteCustomer(TestTenantIds.A, 3)).ReturnsAsync(true);
        mock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        var sut = new CustomerService(mock.Object);

        var ok = await sut.DeleteCustomerAsync(TestTenantIds.A, 3);

        Assert.True(ok);
        mock.Verify(r => r.DeleteCustomer(TestTenantIds.A, 3), Times.Once);
        mock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Search_NullQuery_StillDelegates()
    {
        // Why: search with empty/null query is allowed; repository defines semantics.
        var customers = new List<Customer> { Cust(1, TestTenantIds.A) };
        var mock = new Mock<ICustomerRepository>();
        mock.Setup(r => r.Search(TestTenantIds.A, null)).ReturnsAsync(customers);
        var sut = new CustomerService(mock.Object);

        var result = await sut.Search(TestTenantIds.A, null);

        Assert.Single(result);
    }

    [Fact]
    public async Task Search_ManyResults_MapsNestedCars()
    {
        // Why: many results with nested cars must map to DTO graph for UI.
        var car = new Car { Id = 1, Model = "M", maxSpeed = 100, CustomerId = 1, TenantId = TestTenantIds.A };
        var c = Cust(1, TestTenantIds.A);
        c.Cars = new List<Car> { car };
        var mock = new Mock<ICustomerRepository>();
        mock.Setup(r => r.Search(TestTenantIds.A, "a")).ReturnsAsync(new List<Customer> { c });
        var sut = new CustomerService(mock.Object);

        var result = await sut.Search(TestTenantIds.A, "a");

        Assert.Single(result);
        Assert.Single(result[0].Cars);
        Assert.Equal("M", result[0].Cars[0].Model);
    }

    [Fact]
    public async Task PaginateCustomersAsync_ReturnsPage()
    {
        // Why: pagination boundary for customer tables.
        var rows = new List<Customer> { Cust(1, TestTenantIds.A), Cust(2, TestTenantIds.A) };
        var mock = new Mock<ICustomerRepository>();
        mock.Setup(r => r.PaginateCustomers(TestTenantIds.A, 1, 10, "name"))
            .ReturnsAsync(rows);
        var sut = new CustomerService(mock.Object);

        var result = await sut.PaginateCustomersAsync(TestTenantIds.A, 1, 10, "name");

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetCustomerWithMostCarsAsync_NoData_ReturnsNull()
    {
        // Why: optional aggregate must handle missing data.
        var mock = new Mock<ICustomerRepository>();
        mock.Setup(r => r.GetCustomerWithMostCarsAsync(TestTenantIds.A)).ReturnsAsync(((Customer, int)?)null);
        var sut = new CustomerService(mock.Object);

        var result = await sut.GetCustomerWithMostCarsAsync(TestTenantIds.A);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCustomerWithMostCarsAsync_WithCars_ReturnsTuple()
    {
        // Why: dashboard widgets need name, count, and car list.
        var cust = Cust(1, TestTenantIds.A, "Ann", "Bee", "City", "a@b.c");
        cust.Cars.Add(new Car { Id = 1, Model = "C1", maxSpeed = 1, TenantId = TestTenantIds.A, CustomerId = 1 });
        var mock = new Mock<ICustomerRepository>();
        mock.Setup(r => r.GetCustomerWithMostCarsAsync(TestTenantIds.A))
            .ReturnsAsync((cust, 1));
        var sut = new CustomerService(mock.Object);

        var result = await sut.GetCustomerWithMostCarsAsync(TestTenantIds.A);

        Assert.NotNull(result);
        Assert.Equal("Ann Bee", result.Value.Name);
        Assert.Equal(1, result.Value.CarCount);
        Assert.Single(result.Value.Cars);
    }

    [Fact]
    public async Task GetCountAsync_Delegates()
    {
        // Why: paging totals.
        var mock = new Mock<ICustomerRepository>();
        mock.Setup(r => r.GetCountAsync(TestTenantIds.A)).ReturnsAsync(5);
        var sut = new CustomerService(mock.Object);

        Assert.Equal(5, await sut.GetCountAsync(TestTenantIds.A));
    }
}
