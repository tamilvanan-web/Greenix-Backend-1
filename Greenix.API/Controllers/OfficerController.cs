using Greenix.Application.DTOs.Admin;
using Greenix.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Greenix.API.Controllers
{
    [ApiController]
    [Route("api/v1/officer")]
    [Authorize(Roles = "GovernmentOfficer")]
    public class OfficerController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public OfficerController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // GET: api/v1/officer/complaints
        [HttpGet("complaints")]
        public async Task<IActionResult> GetComplaints()
        {
            var complaints = await _adminService.GetAllComplaintsAsync();
            return Ok(complaints);
        }

        // GET: api/v1/officer/complaints/5
        [HttpGet("complaints/{id}")]
        public async Task<IActionResult> GetComplaint(int id)
        {
            var complaint = await _adminService.GetComplaintByIdAsync(id);

            if (complaint == null)
            {
                return NotFound(new
                {
                    message = "Complaint not found."
                });
            }

            return Ok(complaint);
        }

        // PUT: api/v1/officer/complaints/5/status
        [HttpPut("complaints/{id}/status")]
        public async Task<IActionResult> UpdateStatus(
            int id,
            [FromBody] UpdateComplaintStatusDto request)
        {
            var result = await _adminService.UpdateComplaintStatusAsync(id, request);

            if (!result)
            {
                return NotFound(new
                {
                    message = "Complaint not found."
                });
            }

            return Ok(new
            {
                message = "Complaint status updated successfully."
            });
        }
    }
}