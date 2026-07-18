using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Greenix.Shared.Constants;
using Greenix.Shared.Models;
using Greenix.Application.Interfaces;
using Greenix.Application.DTOs.Dashboard;

namespace Greenix.API.Controllers
{
    [Authorize(Roles = Roles.GovernmentOfficer + "," + Roles.Administrator)]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/dashboard")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        private string GetUserRole() => User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        [HttpGet("summary")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<DashboardSummaryResponse>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BaseResponse<DashboardSummaryResponse>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetSummary()
        {
            var userId = GetUserId();
            var role = GetUserRole();

            var response = await _dashboardService.GetDashboardSummaryAsync(userId, role);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
