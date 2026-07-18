using Greenix.Application.DTOs.Admin;
using Greenix.Application.Interfaces;
using Greenix.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Greenix.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(Roles = Roles.Administrator)]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("complaints")]
        public async Task<IActionResult> GetAllComplaints()
        {
            var complaints = await _adminService.GetAllComplaintsAsync();

            return Ok(complaints);
        }

        [HttpGet("complaints/{id}")]
        public async Task<IActionResult> GetComplaintById(int id)
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

        [HttpPut("complaints/{id}/status")]
        public async Task<IActionResult> UpdateComplaintStatus(
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


        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _adminService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _adminService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            return Ok(user);
        }


    }
}