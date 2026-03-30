using task1.Application.Models;

namespace task1.Application.Interfaces
{
    public interface INotificationService
    {
        Task RecordAsync(string message, Guid tenantId);
        Task<List<NotificationModel>> GetAllAsync(Guid tenantId);
        Task<bool> DeleteAsync(Guid tenantId, int id);
        Task<int> ClearAsync(Guid tenantId);
    }
}
