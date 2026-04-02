using Moq;
using task1.Application.Interfaces;
using task1.Application.Services;
using task1.DataLayer.Entities;
using task1.DataLayer.Interfaces;
using Xunit;

namespace task1.Application.Tests;

public class NotificationServiceTests
{
    [Fact]
    public async Task RecordAsync_EmptyMessage_DoesNotPersistOrPublish()
    {
        // Why: noisy empty notifications should be ignored at the service boundary.
        var mockRepo = new Mock<INotificationRepository>();
        var mockRt = new Mock<INotificationRealtimePublisher>();
        var sut = new NotificationService(mockRepo.Object, mockRt.Object);

        await sut.RecordAsync("   ", TestTenantIds.A);

        mockRepo.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Never);
        mockRt.Verify(r => r.NotifyCreatedAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task RecordAsync_ValidMessage_AddsSavesAndNotifies()
    {
        // Why: success path must persist then push realtime update.
        var mockRepo = new Mock<INotificationRepository>();
        Notification? added = null;
        mockRepo.Setup(r => r.AddAsync(It.IsAny<Notification>()))
            .ReturnsAsync((Notification n) =>
            {
                added = n;
                n.Id = 42;
                return n;
            });
        mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        var mockRt = new Mock<INotificationRealtimePublisher>();
        mockRt.Setup(r => r.NotifyCreatedAsync(42, It.IsAny<string>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        var sut = new NotificationService(mockRepo.Object, mockRt.Object);

        await sut.RecordAsync(" Hello ", TestTenantIds.A);

        Assert.NotNull(added);
        Assert.Equal(TestTenantIds.A, added!.TenantId);
        mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        mockRt.Verify(r => r.NotifyCreatedAsync(42, "Hello", It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_EmptyList_ReturnsEmptyModels()
    {
        // Why: zero notifications after clear must not break the UI.
        var mockRepo = new Mock<INotificationRepository>();
        mockRepo.Setup(r => r.GetAllAsync(TestTenantIds.A)).ReturnsAsync(new List<Notification>());
        var sut = new NotificationService(mockRepo.Object, Mock.Of<INotificationRealtimePublisher>());

        var result = await sut.GetAllAsync(TestTenantIds.A);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_Many_MapsFields()
    {
        // Why: many rows must map Id, message, timestamp for lists.
        var list = new List<Notification>
        {
            new() { Id = 1, Message = "a", CreatedAt = DateTime.UtcNow, TenantId = TestTenantIds.A },
            new() { Id = 2, Message = "b", CreatedAt = DateTime.UtcNow, TenantId = TestTenantIds.A }
        };
        var mockRepo = new Mock<INotificationRepository>();
        mockRepo.Setup(r => r.GetAllAsync(TestTenantIds.A)).ReturnsAsync(list);
        var sut = new NotificationService(mockRepo.Object, Mock.Of<INotificationRealtimePublisher>());

        var result = await sut.GetAllAsync(TestTenantIds.A);

        Assert.Equal(2, result.Count);
        Assert.Equal("a", result[0].Message);
    }

    [Fact]
    public async Task DeleteAsync_Success_SavesAndNotifies()
    {
        // Why: soft-delete path should confirm persistence before SignalR fan-out.
        var mockRepo = new Mock<INotificationRepository>();
        mockRepo.Setup(r => r.DeleteAsync(TestTenantIds.A, 3)).ReturnsAsync(true);
        mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        var mockRt = new Mock<INotificationRealtimePublisher>();
        mockRt.Setup(r => r.NotifyDeletedAsync(3)).Returns(Task.CompletedTask);
        var sut = new NotificationService(mockRepo.Object, mockRt.Object);

        var ok = await sut.DeleteAsync(TestTenantIds.A, 3);

        Assert.True(ok);
        mockRt.Verify(r => r.NotifyDeletedAsync(3), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_DoesNotNotifyOrSave()
    {
        // Why: failed delete must not emit realtime events.
        var mockRepo = new Mock<INotificationRepository>();
        mockRepo.Setup(r => r.DeleteAsync(TestTenantIds.A, 3)).ReturnsAsync(false);
        var mockRt = new Mock<INotificationRealtimePublisher>();
        var sut = new NotificationService(mockRepo.Object, mockRt.Object);

        var ok = await sut.DeleteAsync(TestTenantIds.A, 3);

        Assert.False(ok);
        mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        mockRt.Verify(r => r.NotifyDeletedAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ClearAsync_WithRows_SavesAndNotifiesCleared()
    {
        // Why: bulk clear must commit then tell clients to refresh.
        var mockRepo = new Mock<INotificationRepository>();
        mockRepo.Setup(r => r.DeleteAllAsync(TestTenantIds.A)).ReturnsAsync(5);
        mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        var mockRt = new Mock<INotificationRealtimePublisher>();
        mockRt.Setup(r => r.NotifyClearedAsync()).Returns(Task.CompletedTask);
        var sut = new NotificationService(mockRepo.Object, mockRt.Object);

        var count = await sut.ClearAsync(TestTenantIds.A);

        Assert.Equal(5, count);
        mockRt.Verify(r => r.NotifyClearedAsync(), Times.Once);
    }

    [Fact]
    public async Task ClearAsync_Zero_DoesNotSaveOrNotify()
    {
        // Why: idempotent clear when already empty.
        var mockRepo = new Mock<INotificationRepository>();
        mockRepo.Setup(r => r.DeleteAllAsync(TestTenantIds.A)).ReturnsAsync(0);
        var mockRt = new Mock<INotificationRealtimePublisher>();
        var sut = new NotificationService(mockRepo.Object, mockRt.Object);

        var count = await sut.ClearAsync(TestTenantIds.A);

        Assert.Equal(0, count);
        mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        mockRt.Verify(r => r.NotifyClearedAsync(), Times.Never);
    }
}
