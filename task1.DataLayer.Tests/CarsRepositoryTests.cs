using Microsoft.EntityFrameworkCore;
using task1.DataLayer.DbContexts;
using task1.DataLayer.Entities;
using task1.DataLayer.Repositories;
using Xunit;

namespace task1.DataLayer.Tests;

public class CarsRepositoryTests
{
    [Fact]
    public void GetAll_ReturnsAllCars()
    {
        var options = new DbContextOptionsBuilder<DotNetTrainingCoreContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (var context = new DotNetTrainingCoreContext(options))
        {
            context.Cars.Add(new Car { Id = 1, Model = "Toyota", maxSpeed = 180 });
            context.Cars.Add(new Car { Id = 2, Model = "Honda", maxSpeed = 200 });
            context.SaveChanges();
        }

        using (var context = new DotNetTrainingCoreContext(options))
        {
            var repo = new CarsRepository(context);
            var result = repo.GetAll().ToList();
            Assert.Equal(2, result.Count);
        }
    }

    [Fact]
    public void GetById_ReturnsMatchingCar()
    {
        var options = new DbContextOptionsBuilder<DotNetTrainingCoreContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (var context = new DotNetTrainingCoreContext(options))
        {
            context.Cars.Add(new Car { Id = 1, Model = "Toyota", maxSpeed = 180 });
            context.SaveChanges();
        }

        using (var context = new DotNetTrainingCoreContext(options))
        {
            var repo = new CarsRepository(context);
            var result = repo.GetById(1).FirstOrDefault();
            Assert.NotNull(result);
            Assert.Equal("Toyota", result.Model);
        }
    }

    [Fact]
    public async Task AddCarAsync_AddsCar()
    {
        var options = new DbContextOptionsBuilder<DotNetTrainingCoreContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (var context = new DotNetTrainingCoreContext(options))
        {
            var repo = new CarsRepository(context);
            var car = new Car { Model = "BMW", maxSpeed = 250 };
            var added = await repo.AddCarAsync(car);
            await repo.SaveChangesAsync();

            Assert.NotNull(added);
            Assert.Equal("BMW", added.Model);
        }
    }

    [Fact]
    public async Task UpdateCar_ExistingId_UpdatesAndReturnsCar()
    {
        var options = new DbContextOptionsBuilder<DotNetTrainingCoreContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (var context = new DotNetTrainingCoreContext(options))
        {
            context.Cars.Add(new Car { Id = 1, Model = "Toyota", maxSpeed = 180 });
            await context.SaveChangesAsync();
        }

        using (var context = new DotNetTrainingCoreContext(options))
        {
            var repo = new CarsRepository(context);
            var updated = await repo.UpdateCar(1, new Car { Model = "Lexus", maxSpeed = 220, CustomerId = null });
            await repo.SaveChangesAsync();

            Assert.NotNull(updated);
            Assert.Equal("Lexus", updated.Model);
            Assert.Equal(220, updated.maxSpeed);
        }
    }

    [Fact]
    public async Task UpdateCar_NonExistingId_ReturnsNull()
    {
        var options = new DbContextOptionsBuilder<DotNetTrainingCoreContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (var context = new DotNetTrainingCoreContext(options))
        {
            var repo = new CarsRepository(context);
            var result = await repo.UpdateCar(999, new Car { Model = "Lexus", maxSpeed = 220 });
            Assert.Null(result);
        }
    }

    [Fact]
    public async Task DeleteCar_ExistingId_ReturnsTrue()
    {
        var options = new DbContextOptionsBuilder<DotNetTrainingCoreContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (var context = new DotNetTrainingCoreContext(options))
        {
            context.Cars.Add(new Car { Id = 1, Model = "Toyota", maxSpeed = 180 });
            await context.SaveChangesAsync();
        }

        using (var context = new DotNetTrainingCoreContext(options))
        {
            var repo = new CarsRepository(context);
            var result = await repo.DeleteCar(1);
            await repo.SaveChangesAsync();
            Assert.True(result);
        }
    }

    [Fact]
    public async Task DeleteCar_NonExistingId_ReturnsFalse()
    {
        var options = new DbContextOptionsBuilder<DotNetTrainingCoreContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (var context = new DotNetTrainingCoreContext(options))
        {
            var repo = new CarsRepository(context);
            var result = await repo.DeleteCar(999);
            Assert.False(result);
        }
    }

    [Fact]
    public async Task PaginateCars_ReturnsPageOfFour()
    {
        var options = new DbContextOptionsBuilder<DotNetTrainingCoreContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (var context = new DotNetTrainingCoreContext(options))
        {
            for (int i = 1; i <= 6; i++)
                context.Cars.Add(new Car { Id = i, Model = $"Model{i}", maxSpeed = 150 + i });
            await context.SaveChangesAsync();
        }

        using (var context = new DotNetTrainingCoreContext(options))
        {
            var repo = new CarsRepository(context);
            var page1 = await repo.PaginateCars(1, 4);
            var page2 = await repo.PaginateCars(2, 4);

            Assert.Equal(4, page1.Count);
            Assert.Equal(2, page2.Count);
            Assert.Equal(1, page1[0].Id);
            Assert.Equal(5, page2[0].Id);
        }
    }

    [Fact]
    public async Task GetCountAsync_ReturnsCorrectCount()
    {
        var options = new DbContextOptionsBuilder<DotNetTrainingCoreContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (var context = new DotNetTrainingCoreContext(options))
        {
            context.Cars.Add(new Car { Id = 1, Model = "A", maxSpeed = 180 });
            context.Cars.Add(new Car { Id = 2, Model = "B", maxSpeed = 200 });
            await context.SaveChangesAsync();
        }

        using (var context = new DotNetTrainingCoreContext(options))
        {
            var repo = new CarsRepository(context);
            var count = await repo.GetCountAsync();
            Assert.Equal(2, count);
        }
    }
}
