using Microsoft.EntityFrameworkCore;
using task1.Application.Interfaces;
using task1.Application.Models;
using task1.Application.Resilience;
using task1.DataLayer.Entities;
using task1.DataLayer.Interfaces;
namespace task1.Application.Services
{
    public class CarService : ICarService
    {
        private readonly ICarsRepository _carsRepository;
        private readonly IDatabaseResiliencePipeline _resilience;

        public CarService(ICarsRepository carsRepository, IDatabaseResiliencePipeline resilience)
        {
            _carsRepository = carsRepository;
            _resilience = resilience;
        }

        public async Task<List<CarModel>> GetAllAsync()
{
    var cars = await _resilience.ExecuteAsync(() => _carsRepository.GetAll().ToListAsync());
    return cars.Select(c => new CarModel
    {
        Id = c.Id,
        Model = c.Model,
        maxSpeed = c.maxSpeed,
        CustomerId = c.CustomerId
    }).ToList();
        }

        public async Task<CarModel?> GetByIdAsync(int id)
        {
    var car = await _resilience.ExecuteAsync(() => _carsRepository.GetById(id).FirstOrDefaultAsync());
    if (car == null) return null;
    return new CarModel
    {
        Id = car.Id,
        Model = car.Model,
        maxSpeed = car.maxSpeed,
        CustomerId = car.CustomerId
    };
        }

        public async Task<CarModel> AddCarAsync(CarModel carModel)
        {
    var car = new Car
    {
        Model = carModel.Model,
        maxSpeed = carModel.maxSpeed,
        CustomerId = carModel.CustomerId
    };
    var addedCar = await _resilience.ExecuteAsync(() => _carsRepository.AddCarAsync(car));
    await _resilience.ExecuteAsync(() => _carsRepository.SaveChangesAsync());
    return new CarModel{
        Id = addedCar.Id,
        Model = addedCar.Model,
        maxSpeed = addedCar.maxSpeed,
        CustomerId = addedCar.CustomerId
    };
        }

        public async Task<CarModel?> UpdateCarAsync(int id, CarModel carModel)
        {
    var car = await _resilience.ExecuteAsync(() => _carsRepository.GetById(id).FirstOrDefaultAsync());
    if (car == null) return null;
    car.Model = carModel.Model;
    car.maxSpeed = carModel.maxSpeed;
    car.CustomerId = carModel.CustomerId;
    var updatedCar = await _resilience.ExecuteAsync(() => _carsRepository.UpdateCar(id, car));
    if (updatedCar == null) return null;
    await _resilience.ExecuteAsync(() => _carsRepository.SaveChangesAsync());
    return new CarModel{
       Id = updatedCar.Id,
       Model = updatedCar.Model,
       maxSpeed = updatedCar.maxSpeed,
       CustomerId = updatedCar.CustomerId
    };
        }

        public async Task<bool> DeleteCarAsync(int id)
        {
    var deleted = await _resilience.ExecuteAsync(() => _carsRepository.DeleteCar(id));
    if (!deleted) return false;
    await _resilience.ExecuteAsync(() => _carsRepository.SaveChangesAsync());
    return true;
        }

        public async Task<List<CarModel>> PaginateCarsAsync(int page)
        {
            var cars = await _resilience.ExecuteAsync(() => _carsRepository.PaginateCars(page));
            return cars.Select(c => new CarModel
            {
                Id = c.Id,
                Model = c.Model,
                maxSpeed = c.maxSpeed,
                CustomerId = c.CustomerId
            }).ToList();
        }

        public async Task<int> GetCountAsync() => await _resilience.ExecuteAsync(() => _carsRepository.GetCountAsync());
    }
}