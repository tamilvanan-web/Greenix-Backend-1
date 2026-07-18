using System.Threading.Tasks;
using Greenix.Shared.Models;
using Greenix.Application.DTOs.Dashboard;

namespace Greenix.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<BaseResponse<DashboardSummaryResponse>> GetDashboardSummaryAsync(string userId, string role);
    }
}
