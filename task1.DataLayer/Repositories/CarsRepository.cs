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

        public IQueryable<Car> GetAll(Guid tenantId)
        {
            return _context.Cars.Where(c => c.TenantId == tenantId);
        }

        public IQueryable<Car> GetById(Guid tenantId, int id)
        {
            return _context.Cars.Where(c => c.TenantId == tenantId && c.Id == id);
        }
        public Task<Car> AddCarAsync(Car car)
        {
            var entity = _context.Cars.Add(car);
            return Task.FromResult(entity.Entity);
        }
        public async Task<Car?> UpdateCar(Guid tenantId, int id, Car car)
        {
            var entity = await _context.Cars.FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == id);
            if (entity == null) return null;
            entity.Model = car.Model;
            entity.maxSpeed = car.maxSpeed;
            entity.CustomerId = car.CustomerId;
            _context.Update(entity);
            return entity;
        }
        public async Task<bool> DeleteCar(Guid tenantId, int id)
        {
            var entity = await _context.Cars.FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == id);
            if (entity == null) return false;
            _context.Cars.Remove(entity);
            return true;
        }
        public async Task<List<Car>> PaginateCars(Guid tenantId, int page, int pageSize)
        {
            return await _context.Cars
                .Where(c => c.TenantId == tenantId)
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetCountAsync(Guid tenantId)
        {
            return await _context.Cars.CountAsync(c => c.TenantId == tenantId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}