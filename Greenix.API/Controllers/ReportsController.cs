using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Greenix.Shared.Constants;
using Greenix.Shared.Models;
using Greenix.Application.Interfaces;
using Greenix.Application.DTOs.Report;

namespace Greenix.API.Controllers
{
    [Authorize(Roles = Roles.GovernmentOfficer + "," + Roles.Administrator)]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/reports")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        private string GetUserRole() => User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        [HttpGet("export/csv")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BaseResponse<byte[]>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ExportCsv([FromQuery] ReportFilterRequest filters)
        {
            var userId = GetUserId();
            var role = GetUserRole();

            var response = await _reportService.ExportComplaintsToCsvAsync(filters, userId, role);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            var fileName = $"Complaints_Export_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return File(response.Data!, "text/csv", fileName);
        }
    }
}
