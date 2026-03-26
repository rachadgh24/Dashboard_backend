using task1.DataLayer.Entities;

namespace task1.DataLayer.Interfaces
{
    public interface ICarsRepository
    {
        IQueryable<Car> GetAll();
        IQueryable<Car> GetById(int id);
    Task<Car?> UpdateCar(int id, Car car);
        Task<bool> DeleteCar(int id);
    Task<Car> AddCarAsync(Car car);
        Task<List<Car>> PaginateCars(int page, int pageSize);
        Task<int> GetCountAsync();
        Task SaveChangesAsync();
    }
}
