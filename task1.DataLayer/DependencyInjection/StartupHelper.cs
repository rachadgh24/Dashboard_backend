using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using task1.DataLayer.DbContexts;
using task1.DataLayer.Interfaces;
using task1.DataLayer.Repositories;

namespace task1.DataLayer.DependencyInjection
{
    public static class StartupHelper
    {
        public static void AddDotNetTrainingCoreContext(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<DotNetTrainingCoreContext>(opt =>
            {
                opt.UseNpgsql(connectionString, options =>
                {
                    options.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                    options.CommandTimeout(30);
                    options.MaxBatchSize(100);
                    options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                })
                .LogTo(Console.WriteLine, LogLevel.Information);
            });
        }

        public static void AddDataLayerRepositories(this IServiceCollection services)
        {
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ICarsRepository, CarsRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<ITenantRepository, TenantRepository>();
        }
    }
}