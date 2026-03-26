namespace task1.Application.Interfaces;

public interface INotificationRealtimePublisher
{
    Task NotifyCreatedAsync(int id, string message, DateTime createdAtUtc);
    Task NotifyDeletedAsync(int id);
    Task NotifyClearedAsync();
}
