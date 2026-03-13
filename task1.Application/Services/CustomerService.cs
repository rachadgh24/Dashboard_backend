using Microsoft.EntityFrameworkCore;
using task1.Application.Interfaces;
using task1.Application.Models;
using task1.DataLayer.Entities;
using task1.DataLayer.Interfaces;

namespace task1.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<List<CustomerModel>> GetAllAsync()
        {
            var customers = await _customerRepository.GetAll().ToListAsync();
            return customers.Select(c => new CustomerModel
            {
                Email = c.Email,
                Id = c.Id,
                Name = c.Name,
                LastName = c.LastName,
                City = c.City
            }).ToList();
        }

        public async Task<CustomerModel?> GetByIdAsync(int id)
        {
            var customer = await _customerRepository.GetById(id).FirstOrDefaultAsync();
            if (customer == null) return null;

            return new CustomerModel
            {
                Email = customer.Email,
                Id = customer.Id,
                Name = customer.Name,
                LastName = customer.LastName,
                City = customer.City
            };
        }

        public async Task<CustomerModel> AddCustomerAsync(CustomerModel customerModel)
        {
            var customer = new Customer
            {
                Email = customerModel.Email ?? string.Empty,
                Name = customerModel.Name ?? string.Empty,
                LastName = customerModel.LastName ?? string.Empty,
                City = customerModel.City ?? string.Empty
            };

            var addedEntity = await _customerRepository.AddCustomerAsync(customer);
            await _customerRepository.SaveChangesAsync();

            return new CustomerModel
            {
                Id = addedEntity.Id,
                Name = addedEntity.Name,
                LastName = addedEntity.LastName,
                City = addedEntity.City,
                Email = addedEntity.Email
            };
        }

        public async Task<CustomerModel?> UpdateCustomerAsync(int id, CustomerModel customerModel)
        {
            var customer = new Customer
            {
                Name = customerModel.Name ?? string.Empty,
                LastName = customerModel.LastName ?? string.Empty,
                City = customerModel.City ?? string.Empty,
                Email = customerModel.Email ?? string.Empty
            };

            var updatedEntity = await _customerRepository.UpdateCustomer(id, customer);
            if (updatedEntity == null) return null;

            await _customerRepository.SaveChangesAsync();
            return new CustomerModel
            {
                Id = updatedEntity.Id,
                Name = updatedEntity.Name,
                LastName = updatedEntity.LastName,
                City = updatedEntity.City,
                Email = updatedEntity.Email
            };
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var deleted = await _customerRepository.DeleteCustomer(id);
            if (!deleted) return false;

            await _customerRepository.SaveChangesAsync();
            return true;
        }

        public async Task<List<CustomerModel>> Search(string? query)
        {
            var customers = await _customerRepository.Search(query);
            return customers.Select(c => new CustomerModel
            {
                Id = c.Id,
                Name = c.Name,
                LastName = c.LastName,
                City = c.City,
                Email = c.Email,
                Cars = c.Cars.Select(car => new CarModel
                {
                    Id = car.Id,
                    Model = car.Model,
                    maxSpeed = car.maxSpeed,
                    CustomerId = car.CustomerId
                }).ToList()
            }).ToList();
        }

        public async Task<List<CustomerModel>> PaginateCustomersAsync(int page, string? sortBy)
        {
            var customers = await _customerRepository.PaginateCustomers(page, sortBy);
            return customers.Select(c => new CustomerModel
            {
                Id = c.Id,
                Name = c.Name,
                LastName = c.LastName,
                City = c.City,
                Email = c.Email,
                Cars = c.Cars.Select(car => new CarModel
                {
                    Id = car.Id,
                    Model = car.Model,
                    maxSpeed = car.maxSpeed,
                    CustomerId = car.CustomerId
                }).ToList()
            }).ToList();
        }

        public async Task<int> GetCountAsync() => await _customerRepository.GetCountAsync();

        public async Task<(string Name, int CarCount, List<CarModel> Cars)?> GetCustomerWithMostCarsAsync()
        {
            var result = await _customerRepository.GetCustomerWithMostCarsAsync();
            if (result == null) return null;
            var name = $"{result.Value.Customer.Name} {result.Value.Customer.LastName}".Trim();
            if (string.IsNullOrEmpty(name)) name = result.Value.Customer.Email;
            var cars = result.Value.Customer.Cars.Select(c => new CarModel
            {
                Id = c.Id,
                Model = c.Model,
                maxSpeed = c.maxSpeed,
                CustomerId = c.CustomerId
            }).ToList();
            return (name, result.Value.CarCount, cars);
        }
    }
}
