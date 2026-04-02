using Microsoft.EntityFrameworkCore;
using Moq;
using task1.Application.Models;
using task1.Application.Services;
using task1.DataLayer.Entities;
using task1.DataLayer.Interfaces;
using Xunit;

namespace task1.Application.Tests;

public class CarServiceTests
{
    private static Car CarEntity(int id, Guid tenantId, string model = "Toyota", int speed = 180, int? customerId = null) =>
        new()
        {
            Id = id,
            TenantId = tenantId,
            Model = model,
            maxSpeed = speed,
            CustomerId = customerId
        };

    [Fact]
    public async Task GetAllAsync_ValidTenant_ReturnsMappedModels()
    {
        // Why: success path ensures CRUD read maps entity fields to API DTOs correctly.
        var cars = new List<Car>
        {
            CarEntity(1, TestTenantIds.A, "A", 100),
            CarEntity(2, TestTenantIds.A, "B", 200)
        };
        var mock = new Mock<ICarsRepository>();
        mock.Setup(r => r.GetAll(TestTenantIds.A)).Returns(cars.BuildMock());
        var sut = new CarService(mock.Object);

        var result = await sut.GetAllAsync(TestTenantIds.A);

        Assert.Equal(2, result.Count);
        Assert.Equal("A", result[0].Model);
        Assert.Equal(100, result[0].maxSpeed);
    }

    [Fact]
    public async Task GetAllAsync_EmptyList_ReturnsEmpty()
    {
        // Why: zero records must not throw and should return an empty collection for new tenants.
        var mock = new Mock<ICarsRepository>();
        mock.Setup(r => r.GetAll(TestTenantIds.A)).Returns(new List<Car>().BuildMock());
        var sut = new CarService(mock.Object);

        var result = await sut.GetAllAsync(TestTenantIds.A);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_SingleRecord_ReturnsOne()
    {
        // Why: single-item tenants are common; mapping must preserve that one row.
        var cars = new List<Car> { CarEntity(7, TestTenantIds.A) };
        var mock = new Mock<ICarsRepository>();
        mock.Setup(r => r.GetAll(TestTenantIds.A)).Returns(cars.BuildMock());
        var sut = new CarService(mock.Object);

        var result = await sut.GetAllAsync(TestTenantIds.A);

        Assert.Single(result);
        Assert.Equal(7, result[0].Id);
    }

    [Fact]
    public async Task GetByIdAsync_WrongTenant_ReturnsNull()
    {
        // Why: tenant isolation prevents reading another tenant's row when the query returns no match.
        var cars = new List<Car> { CarEntity(1, TestTenantIds.A) };
        var mock = new Mock<ICarsRepository>();
        mock.Setup(r => r.GetById(TestTenantIds.B, 1)).Returns(
            cars.Where(c => c.TenantId == TestTenantIds.B && c.Id == 1).BuildMock());
        var sut = new CarService(mock.Object);

        var result = await sut.GetByIdAsync(TestTenantIds.B, 1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_Existing_ReturnsModel()
    {
        // Why: success path for detail views.
        var cars = new List<Car> { CarEntity(1, TestTenantIds.A) };
        var mock = new Mock<ICarsRepository>();
        mock.Setup(r => r.GetById(TestTenantIds.A, 1)).Returns(
            cars.Where(c => c.TenantId == TestTenantIds.A && c.Id == 1).BuildMock());
        var sut = new CarService(mock.Object);

        var result = await sut.GetByIdAsync(TestTenantIds.A, 1);

        Assert.NotNull(result);
        Assert.Equal("Toyota", result.Model);
    }

    [Fact]
    public async Task AddCarAsync_PersistsAndReturnsModel()
    {
        // Why: create flow must call save and return identifiers to the caller.
        var mock = new Mock<ICarsRepository>();
        Car? captured = null;
        mock.Setup(r => r.AddCarAsync(It.IsAny<Car>()))
            .ReturnsAsync((Car c) =>
            {
                captured = c;
                c.Id = 99;
                return c;
            });
        mock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        var sut = new CarService(mock.Object);
        var model = new CarModel { Model = "X", maxSpeed = 50, CustomerId = 3 };

        var result = await sut.AddCarAsync(TestTenantIds.A, model);

        Assert.Equal(99, result.Id);
        Assert.Equal(TestTenantIds.A, captured!.TenantId);
        mock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateCarAsync_UpdateReturnsNull_ReturnsNullWithoutSave()
    {
        // Why: simulates a lost update / race where another writer removed the row before save.
        var existing = CarEntity(1, TestTenantIds.A);
        var mock = new Mock<ICarsRepository>();
        mock.Setup(r => r.GetById(TestTenantIds.A, 1)).Returns(
            new List<Car> { existing }.BuildMock());
        mock.Setup(r => r.UpdateCar(TestTenantIds.A, 1, It.IsAny<Car>())).ReturnsAsync((Car?)null);
        var sut = new CarService(mock.Object);

        var result = await sut.UpdateCarAsync(TestTenantIds.A, 1, new CarModel { Model = "Z", maxSpeed = 1 });

        Assert.Null(result);
        mock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateCarAsync_SaveChangesThrowsConcurrency_PropagatesException()
    {
        // Why: concurrency conflicts from the store must surface so the API can map to 409/409-style handling.
        var existing = CarEntity(1, TestTenantIds.A);
        var mock = new Mock<ICarsRepository>();
        mock.Setup(r => r.GetById(TestTenantIds.A, 1)).Returns(
            new List<Car> { existing }.BuildMock());
        mock.Setup(r => r.UpdateCar(TestTenantIds.A, 1, It.IsAny<Car>())).ReturnsAsync(existing);
        mock.Setup(r => r.SaveChangesAsync()).ThrowsAsync(new DbUpdateConcurrencyException());
        var sut = new CarService(mock.Object);

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
            sut.UpdateCarAsync(TestTenantIds.A, 1, new CarModel { Model = "Z", maxSpeed = 1 }));
    }

    [Fact]
    public async Task DeleteCarAsync_Success_CallsRepositorySoftDeletePathAndSave()
    {
        // Why: orchestration must invoke delete and persist; actual IsDeleted is verified in DataLayer tests.
        var mock = new Mock<ICarsRepository>();
        mock.Setup(r => r.DeleteCar(TestTenantIds.A, 5)).ReturnsAsync(true);
        mock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        var sut = new CarService(mock.Object);

        var ok = await sut.DeleteCarAsync(TestTenantIds.A, 5);

        Assert.True(ok);
        mock.Verify(r => r.DeleteCar(TestTenantIds.A, 5), Times.Once);
        mock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteCarAsync_NotFound_DoesNotSave()
    {
        // Why: idempotent delete must not call SaveChanges when nothing was removed.
        var mock = new Mock<ICarsRepository>();
        mock.Setup(r => r.DeleteCar(TestTenantIds.A, 5)).ReturnsAsync(false);
        var sut = new CarService(mock.Object);

        var ok = await sut.DeleteCarAsync(TestTenantIds.A, 5);

        Assert.False(ok);
        mock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task PaginateCarsAsync_FirstPage_ReturnsSlice()
    {
        // Why: pagination boundary — first page returns up to pageSize items.
        var mock = new Mock<ICarsRepository>();
        mock.Setup(r => r.PaginateCars(TestTenantIds.A, 1, 4))
            .ReturnsAsync(Enumerable.Range(1, 4).Select(i => CarEntity(i, TestTenantIds.A, $"M{i}", 150 + i)).ToList());
        var sut = new CarService(mock.Object);

        var page = await sut.PaginateCarsAsync(TestTenantIds.A, 1, 4);

        Assert.Equal(4, page.Count);
        Assert.Equal(1, page[0].Id);
    }

    [Fact]
    public async Task PaginateCarsAsync_PageBeyondData_ReturnsEmpty()
    {
        // Why: many pages — requesting past the last page must return empty, not throw.
        var mock = new Mock<ICarsRepository>();
        mock.Setup(r => r.PaginateCars(TestTenantIds.A, 99, 10)).ReturnsAsync(new List<Car>());
        var sut = new CarService(mock.Object);

        var page = await sut.PaginateCarsAsync(TestTenantIds.A, 99, 10);

        Assert.Empty(page);
    }

    [Fact]
    public async Task GetCountAsync_DelegatesToRepository()
    {
        // Why: list UIs need total count for paging controls.
        var mock = new Mock<ICarsRepository>();
        mock.Setup(r => r.GetCountAsync(TestTenantIds.A)).ReturnsAsync(42);
        var sut = new CarService(mock.Object);

        var count = await sut.GetCountAsync(TestTenantIds.A);

        Assert.Equal(42, count);
    }
}
