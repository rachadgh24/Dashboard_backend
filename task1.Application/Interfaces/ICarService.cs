using task1.Application.Models;

namespace task1.Application.Interfaces
{
    public interface ICarService
    {
        Task<List<CarModel>> GetAllAsync(Guid tenantId);
        Task<CarModel?> GetByIdAsync(Guid tenantId, int id);
        Task<CarModel> AddCarAsync(Guid tenantId, CarModel carModel);
        Task<CarModel?> UpdateCarAsync(Guid tenantId, int id, CarModel carModel);
        Task<bool> DeleteCarAsync(Guid tenantId, int id);
        Task<List<CarModel>> PaginateCarsAsync(Guid tenantId, int page, int pageSize);
        Task<int> GetCountAsync(Guid tenantId);
    }
}
