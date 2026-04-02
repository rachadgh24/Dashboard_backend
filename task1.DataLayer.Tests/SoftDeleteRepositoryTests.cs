using Microsoft.EntityFrameworkCore;
using task1.DataLayer.DbContexts;
using task1.DataLayer.Entities;
using task1.DataLayer.Repositories;
using Xunit;

namespace task1.DataLayer.Tests;

public class SoftDeleteRepositoryTests
{
    private static readonly Guid TenantId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Fact]
    public async Task UserRepository_DeleteUserAsync_SetsIsDeleted_RowRemains()
    {
        // Why: user soft delete must preserve audit/history rows while hiding them from default queries.
        var options = new DbContextOptionsBuilder<DotNetTrainingCoreContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (var context = new DotNetTrainingCoreContext(options))
        {
            context.Tenants.Add(new Tenant { TenantId = TenantId, Name = "Org", Status = true });
            context.Roles.Add(new Role { Id = 50, Name = "Admin", TenantId = TenantId });
            context.Users.Add(new User
            {
                Id = 1,
                TenantId = TenantId,
                RoleId = 50,
                Name = "U",
                PhoneNumber = "+1999",
                PasswordHash = "h"
            });
            await context.SaveChangesAsync();
        }

        using (var context = new DotNetTrainingCoreContext(options))
        {
            var repo = new UserRepository(context);
            Assert.True(await repo.DeleteUserAsync(TenantId, 1));
            await repo.SaveChangesAsync();
        }

        using (var context = new DotNetTrainingCoreContext(options))
        {
            var raw = await context.Users.IgnoreQueryFilters().SingleAsync(u => u.Id == 1);
            Assert.True(raw.IsDeleted);
            Assert.NotNull(raw.DeletedAt);
            Assert.False(await context.Users.AnyAsync(u => u.Id == 1));
        }
    }

    [Fact]
    public async Task CustomerRepository_DeleteCustomerAsync_SoftDeletesCustomerAndCars()
    {
        // Why: cascading soft delete keeps referential data but marks dependent cars deleted too.
        var options = new DbContextOptionsBuilder<DotNetTrainingCoreContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (var context = new DotNetTrainingCoreContext(options))
        {
            context.Customers.Add(new Customer
            {
                Id = 1,
                TenantId = TenantId,
                Name = "N",
                LastName = "L",
                City = "C",
                Email = "e@e.e"
            });
            context.Cars.Add(new Car { Id = 10, Model = "M", maxSpeed = 100, TenantId = TenantId, CustomerId = 1 });
            await context.SaveChangesAsync();
        }

        using (var context = new DotNetTrainingCoreContext(options))
        {
            var repo = new CustomerRepository(context);
            Assert.True(await repo.DeleteCustomer(TenantId, 1));
            await repo.SaveChangesAsync();
        }

        using (var context = new DotNetTrainingCoreContext(options))
        {
            var cust = await context.Customers.IgnoreQueryFilters().SingleAsync(c => c.Id == 1);
            Assert.True(cust.IsDeleted);
            var car = await context.Cars.IgnoreQueryFilters().SingleAsync(c => c.Id == 10);
            Assert.True(car.IsDeleted);
            Assert.False(await context.Customers.AnyAsync(c => c.Id == 1));
            Assert.False(await context.Cars.AnyAsync(c => c.Id == 10));
        }
    }

    [Fact]
    public async Task NotificationRepository_DeleteAsync_SetsIsDeleted_RowRemains()
    {
        // Why: notification feed uses soft delete so history can be retained if needed.
        var options = new DbContextOptionsBuilder<DotNetTrainingCoreContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (var context = new DotNetTrainingCoreContext(options))
        {
            context.Tenants.Add(new Tenant { TenantId = TenantId, Name = "Org", Status = true });
            context.Notifications.Add(new Notification
            {
                Id = 1,
                Message = "Hi",
                CreatedAt = DateTime.UtcNow,
                TenantId = TenantId
            });
            await context.SaveChangesAsync();
        }

        using (var context = new DotNetTrainingCoreContext(options))
        {
            var repo = new NotificationRepository(context);
            Assert.True(await repo.DeleteAsync(TenantId, 1));
            await repo.SaveChangesAsync();
        }

        using (var context = new DotNetTrainingCoreContext(options))
        {
            var raw = await context.Notifications.IgnoreQueryFilters().SingleAsync(n => n.Id == 1);
            Assert.True(raw.IsDeleted);
            Assert.NotNull(raw.DeletedAt);
            Assert.False(await context.Notifications.AnyAsync(n => n.Id == 1));
        }
    }

    [Fact]
    public async Task NotificationRepository_DeleteAllAsync_SoftDeletesAllRows()
    {
        // Why: bulk clear must mark every row deleted without physical removal.
        var options = new DbContextOptionsBuilder<DotNetTrainingCoreContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (var context = new DotNetTrainingCoreContext(options))
        {
            context.Tenants.Add(new Tenant { TenantId = TenantId, Name = "Org", Status = true });
            context.Notifications.Add(new Notification { Id = 1, Message = "a", CreatedAt = DateTime.UtcNow, TenantId = TenantId });
            context.Notifications.Add(new Notification { Id = 2, Message = "b", CreatedAt = DateTime.UtcNow, TenantId = TenantId });
            await context.SaveChangesAsync();
        }

        using (var context = new DotNetTrainingCoreContext(options))
        {
            var repo = new NotificationRepository(context);
            Assert.Equal(2, await repo.DeleteAllAsync(TenantId));
            await repo.SaveChangesAsync();
        }

        using (var context = new DotNetTrainingCoreContext(options))
        {
            var all = await context.Notifications.IgnoreQueryFilters().Where(n => n.TenantId == TenantId).ToListAsync();
            Assert.All(all, n => Assert.True(n.IsDeleted));
            Assert.False(await context.Notifications.AnyAsync());
        }
    }
}
