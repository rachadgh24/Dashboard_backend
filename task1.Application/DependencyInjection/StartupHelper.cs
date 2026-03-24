using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using task1.Application.Interfaces;
using task1.Application.Resilience;
using task1.Application.Services;
using task1.DataLayer.DependencyInjection;

namespace task1.Application.DependencyInjection
{
    public static class StartupHelper
    {
        public static void AddApplicationLayerServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDotNetTrainingCoreContext(configuration);
            services.AddDataLayerRepositories();
            services.AddSingleton<IDatabaseResiliencePipeline, DatabaseResiliencePipeline>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ICarService, CarService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IRoleClaimsService, RoleClaimsService>();
            services.AddScoped<IClaimsAdminService, ClaimsAdminService>();
            services.AddScoped<IRolesAdminService, RolesAdminService>();
        }
    }
}
