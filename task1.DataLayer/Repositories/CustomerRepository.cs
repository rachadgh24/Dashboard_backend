using Microsoft.EntityFrameworkCore;
using task1.DataLayer.DbContexts;
using task1.DataLayer.Entities;
using task1.DataLayer.Interfaces;

namespace task1.DataLayer.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly DotNetTrainingCoreContext _context;

        public CustomerRepository(DotNetTrainingCoreContext context)
        {
            _context = context;
        }

        public IQueryable<Customer> GetAll()
        {
            return _context.Customers.AsNoTracking();
        }

        public IQueryable<Customer> GetById(int id)
        {
            return _context.Customers.Where(c => c.Id == id);
        }

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
        }

        public Task<Customer> AddCustomerAsync(Customer customer)
        {
            var entity = _context.Customers.Add(customer);
            return Task.FromResult(entity.Entity);
        }

        public async Task<Customer?> UpdateCustomer(int id, Customer customer)
        {
            var entity = await _context.Customers.FindAsync(id);
            if (entity == null) return null;

            entity.Name = customer.Name;
            entity.LastName = customer.LastName;
            entity.City = customer.City;
            entity.Email = customer.Email;
            return entity;
        }

        public async Task<bool> DeleteCustomer(int id)
        {
            var entity = await _context.Customers.FindAsync(id);
            if (entity == null) return false;

            var relatedCars = _context.Cars.Where(c => c.CustomerId == id);
            _context.Cars.RemoveRange(relatedCars);
            _context.Customers.Remove(entity);
            return true;
        }

        public async Task<List<Customer>> Search(string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<Customer>();
            }

            var lowerQuery = query.Trim().ToLower();
            var customers = await _context.Customers
                .Where(c => (c.Name != null && c.Name.ToLower().Contains(lowerQuery))
                         || (c.LastName != null && c.LastName.ToLower().Contains(lowerQuery))
                         || (c.City != null && c.City.ToLower().Contains(lowerQuery)))
                .AsNoTracking()
                .ToListAsync();

            if (customers.Count == 0) return customers;

            var customerIds = customers.Select(c => c.Id).ToList();
            var cars = await _context.Cars
                .Where(c => c.CustomerId != null && customerIds.Contains(c.CustomerId.Value))
                .OrderBy(c => c.Id)
                .AsNoTracking()
                .ToListAsync();

            foreach (var customer in customers)
            {
                customer.Cars = cars.Where(car => car.CustomerId == customer.Id).ToList();
            }

            return customers;
        }
        public async Task<List<Customer>> PaginateCustomers(int page, string? sortBy)
        {
            var normalized = sortBy?.Trim();
            const int pageSize = 4;
            List<Customer> customers;

            if (string.IsNullOrEmpty(normalized))
            {
                customers = await _context.Customers
                    .OrderBy(c => c.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .AsNoTracking()
                    .ToListAsync();
            }
            else
            {
                switch (normalized.ToLowerInvariant())
                {
                    case "leastcars":
                        {
                            var withCountAsc = _context.Customers
                                .Select(c => new { Customer = c, CarCount = _context.Cars.Count(car => car.CustomerId == c.Id) })
                                .OrderBy(x => x.CarCount)
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize);
                            customers = await withCountAsc.Select(x => x.Customer).AsNoTracking().ToListAsync();
                            break;
                        }
                    case "mostcars":
                        {
                            var withCountDesc = _context.Customers
                                .Select(c => new { Customer = c, CarCount = _context.Cars.Count(car => car.CustomerId == c.Id) })
                                .OrderByDescending(x => x.CarCount)
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize);
                            customers = await withCountDesc.Select(x => x.Customer).AsNoTracking().ToListAsync();
                            break;
                        }
                    case "carname":
                        {
                            var withMinModel = _context.Customers
                                .Select(c => new { Customer = c, MinModel = _context.Cars.Where(car => car.CustomerId == c.Id).Min(car => car.Model) ?? "\uFFFF" })
                                .OrderBy(x => x.MinModel)
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize);
                            customers = await withMinModel.Select(x => x.Customer).AsNoTracking().ToListAsync();
                            break;
                        }
                    case "ownername":
                        customers = await _context.Customers
                            .OrderBy(c => c.Name)
                            .ThenBy(c => c.LastName)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .AsNoTracking()
                            .ToListAsync();
                        break;
                    default:
                        customers = await _context.Customers
                            .OrderBy(c => c.Id)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .AsNoTracking()
                            .ToListAsync();
                        break;
                }
            }

            var customerIds = customers.Select(c => c.Id).ToList();
            if (customerIds.Count == 0) return customers;

            var cars = await _context.Cars
                .Where(c => c.CustomerId != null && customerIds.Contains(c.CustomerId.Value))
                .OrderBy(c => c.Id)
                .AsNoTracking()
                .ToListAsync();

            foreach (var customer in customers)
            {
                customer.Cars = cars.Where(car => car.CustomerId == customer.Id).ToList();
            }

            return customers;
        }

        public async Task<int> GetCountAsync()
        {
            return await _context.Customers.CountAsync();
        }

        public async Task<(Customer Customer, int CarCount)?> GetCustomerWithMostCarsAsync()
        {
            var top = await _context.Customers
                .Select(c => new { Customer = c, CarCount = _context.Cars.Count(car => car.CustomerId == c.Id) })
                .OrderByDescending(x => x.CarCount)
                .FirstOrDefaultAsync();
            if (top == null) return null;
            var cars = await _context.Cars
                .Where(c => c.CustomerId == top.Customer.Id)
                .AsNoTracking()
                .ToListAsync();
            top.Customer.Cars = cars;
            return (top.Customer, top.CarCount);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
