using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using task1.DataLayer.DbContexts;
using task1.DataLayer.Entities;
using task1.DataLayer.Interfaces;

namespace task1.DataLayer.Repositories{
    public class CarsRepository : ICarsRepository
    {
        private readonly DotNetTrainingCoreContext _context;
        public CarsRepository(DotNetTrainingCoreContext context)
        {
            _context = context;
        }

        public IQueryable<Car> GetAll()
        {
            return _context.Cars;
        }

        public IQueryable<Car> GetById(int id)
        {
            return _context.Cars.Where(c => c.Id == id);
        }
        public Task<Car> AddCarAsync(Car car)
        {
            var entity = _context.Cars.Add(car);
            return Task.FromResult(entity.Entity);
        }
        public async Task<Car?> UpdateCar(int id, Car car)
        {
            var entity = await _context.Cars.FindAsync(id);
            if (entity == null) return null;
            entity.Model = car.Model;
            entity.maxSpeed = car.maxSpeed;
            entity.CustomerId = car.CustomerId;
            _context.Update(entity);
            return entity;
        }
        public async Task<bool> DeleteCar(int id)
        {
            var entity = await _context.Cars.FindAsync(id);
            if (entity == null) return false;
            _context.Cars.Remove(entity);
            return true;
        }
        public async Task<List<Car>> PaginateCars(int page, int pageSize)
        {
            return await _context.Cars
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetCountAsync()
        {
            return await _context.Cars.CountAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}