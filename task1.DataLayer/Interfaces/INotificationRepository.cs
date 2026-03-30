using task1.DataLayer.Entities;

namespace task1.DataLayer.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification> AddAsync(Notification notification);
        Task<List<Notification>> GetAllAsync(Guid tenantId);
        Task<bool> DeleteAsync(Guid tenantId, int id);
        Task<int> DeleteAllAsync(Guid tenantId);
        Task SaveChangesAsync();
    }
}
