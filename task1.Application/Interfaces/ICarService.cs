using task1.Application.Models;

namespace task1.Application.Interfaces
{
    public interface ICarService
    {
        Task<List<CarModel>> GetAllAsync();
        Task<CarModel?> GetByIdAsync(int id);
        Task<CarModel> AddCarAsync(CarModel carModel);
        Task<CarModel?> UpdateCarAsync(int id, CarModel carModel);
        Task<bool> DeleteCarAsync(int id);
        Task<List<CarModel>> PaginateCarsAsync(int page, int pageSize);
        Task<int> GetCountAsync();
    }
}
