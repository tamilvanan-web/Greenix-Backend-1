using Microsoft.Extensions.DependencyInjection;
using Greenix.Application.Interfaces;
using Greenix.Application.Services;

namespace Greenix.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IComplaintService, ComplaintService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IReportService, ReportService>();

            return services;
        }
    }
}
