using task1.Application.Interfaces;
using task1.Application.Models;
using task1.DataLayer.Entities;
using task1.DataLayer.Interfaces;

namespace task1.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationRealtimePublisher _realtime;

        public NotificationService(INotificationRepository notificationRepository, INotificationRealtimePublisher realtime)
        {
            _notificationRepository = notificationRepository;
            _realtime = realtime;
        }

        public async Task RecordAsync(string message, Guid tenantId)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var notification = new Notification
            {
                Message = message.Trim(),
                CreatedAt = DateTime.UtcNow,
                TenantId = tenantId
            };

            await _notificationRepository.AddAsync(notification);
            await _notificationRepository.SaveChangesAsync();
            await _realtime.NotifyCreatedAsync(notification.Id, notification.Message, notification.CreatedAt);
        }

        public async Task<List<NotificationModel>> GetAllAsync(Guid tenantId)
        {
            var notifications = await _notificationRepository.GetAllAsync(tenantId);
            return notifications
                .Select(n => new NotificationModel
                {
                    Id = n.Id,
                    Message = n.Message,
                    CreatedAt = n.CreatedAt
                })
                .ToList();
        }

        public async Task<bool> DeleteAsync(Guid tenantId, int id)
        {
            var deleted = await _notificationRepository.DeleteAsync(tenantId, id);
            if (!deleted) return false;

            await _notificationRepository.SaveChangesAsync();
            await _realtime.NotifyDeletedAsync(id);
            return true;
        }

        public async Task<int> ClearAsync(Guid tenantId)
        {
            var count = await _notificationRepository.DeleteAllAsync(tenantId);
            if (count > 0)
            {
                await _notificationRepository.SaveChangesAsync();
                await _realtime.NotifyClearedAsync();
            }

            return count;
        }
    }
}
