using task1.Application.Interfaces;
using task1.Application.Models;
using task1.Application.Resilience;
using task1.DataLayer.Entities;
using task1.DataLayer.Interfaces;

namespace task1.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IDatabaseResiliencePipeline _resilience;

        public NotificationService(INotificationRepository notificationRepository, IDatabaseResiliencePipeline resilience)
        {
            _notificationRepository = notificationRepository;
            _resilience = resilience;
        }

        public async Task RecordAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var notification = new Notification
            {
                Message = message.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _resilience.ExecuteAsync(() => _notificationRepository.AddAsync(notification));
            await _resilience.ExecuteAsync(() => _notificationRepository.SaveChangesAsync());
        }

        public async Task<List<NotificationModel>> GetAllAsync()
        {
            var notifications = await _resilience.ExecuteAsync(() => _notificationRepository.GetAllAsync());
            return notifications
                .Select(n => new NotificationModel
                {
                    Id = n.Id,
                    Message = n.Message,
                    CreatedAt = n.CreatedAt
                })
                .ToList();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var deleted = await _resilience.ExecuteAsync(() => _notificationRepository.DeleteAsync(id));
            if (!deleted) return false;

            await _resilience.ExecuteAsync(() => _notificationRepository.SaveChangesAsync());
            return true;
        }

        public async Task<int> ClearAsync()
        {
            var count = await _resilience.ExecuteAsync(() => _notificationRepository.DeleteAllAsync());
            if (count > 0)
            {
                await _resilience.ExecuteAsync(() => _notificationRepository.SaveChangesAsync());
            }

            return count;
        }
    }
}
