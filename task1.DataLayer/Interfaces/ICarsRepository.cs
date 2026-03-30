using task1.DataLayer.Entities;

namespace task1.DataLayer.Interfaces
{
    public interface ICarsRepository
    {
        IQueryable<Car> GetAll(Guid tenantId);
        IQueryable<Car> GetById(Guid tenantId, int id);
    Task<Car?> UpdateCar(Guid tenantId, int id, Car car);
        Task<bool> DeleteCar(Guid tenantId, int id);
    Task<Car> AddCarAsync(Car car);
        Task<List<Car>> PaginateCars(Guid tenantId, int page, int pageSize);
        Task<int> GetCountAsync(Guid tenantId);
        Task SaveChangesAsync();
    }
}
