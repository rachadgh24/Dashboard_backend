using Microsoft.AspNetCore.SignalR;
using task1.Application.Interfaces;
using task1.Hubs;

namespace task1;

public sealed class NotificationsRealtimePublisher : INotificationRealtimePublisher
{
    private const string AdminGroup = "admins";
    private readonly IHubContext<NotificationsHub> _hubContext;

    public NotificationsRealtimePublisher(IHubContext<NotificationsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyCreatedAsync(int id, string message, DateTime createdAtUtc) =>
        _hubContext.Clients.Group(AdminGroup).SendAsync("NotificationCreated", new { id, message, createdAt = createdAtUtc });

    public Task NotifyDeletedAsync(int id) =>
        _hubContext.Clients.Group(AdminGroup).SendAsync("NotificationDeleted", new { id });

    public Task NotifyClearedAsync() =>
        _hubContext.Clients.Group(AdminGroup).SendAsync("NotificationsCleared");
}
