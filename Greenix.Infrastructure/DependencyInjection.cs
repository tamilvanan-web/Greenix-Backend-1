using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Greenix.Application.Interfaces;
using Greenix.Infrastructure.Identity;
using Greenix.Infrastructure.Geospatial;
using Greenix.Infrastructure.Services;

namespace Greenix.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind Configuration Options
            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
            services.Configure<MunicipalSettings>(configuration.GetSection(MunicipalSettings.SectionName));

            // Register Services
            services.AddTransient<ITokenService, TokenService>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IFileStorageService, LocalFileStorageService>();
            services.AddTransient<IGeospatialService, GeospatialService>();
            
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IAdminService, AdminService>();

            return services;
        }
    }
}
