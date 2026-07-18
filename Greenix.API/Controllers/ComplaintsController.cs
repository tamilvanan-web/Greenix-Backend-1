using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Greenix.Shared.Constants;
using Greenix.Shared.Models;
using Greenix.Application.Interfaces;
using Greenix.Application.DTOs.Complaint;

namespace Greenix.API.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/complaints")]
    [ApiController]
    public class ComplaintsController : ControllerBase
    {
        private readonly IComplaintService _complaintService;

        public ComplaintsController(IComplaintService complaintService)
        {
            _complaintService = complaintService;
        }

        private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        private string GetUserRole() => User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        [HttpPost]
        [Authorize(Roles = Roles.Citizen)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(BaseResponse<ComplaintResponse>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BaseResponse<ComplaintResponse>))]
        public async Task<IActionResult> Create([FromBody] CreateComplaintRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new BaseResponse<ComplaintResponse>("Invalid payload submitted."));
            }

            var userId = GetUserId();
            var response = await _complaintService.CreateComplaintAsync(userId, request);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<ComplaintResponse>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(BaseResponse<ComplaintResponse>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(BaseResponse<ComplaintResponse>))]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = GetUserId();
            var role = GetUserRole();

            var response = await _complaintService.GetComplaintByIdAsync(id, userId, role);
            if (!response.Success)
            {
                if (response.Message.Contains("denied"))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, response);
                }
                return NotFound(response);
            }

            return Ok(response);
        }

        [HttpGet("citizen")]
        [Authorize(Roles = Roles.Citizen)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<List<ComplaintSummaryDto>>))]
        public async Task<IActionResult> GetCitizenHistory()
        {
            var userId = GetUserId();
            var response = await _complaintService.GetCitizenComplaintsAsync(userId);
            return Ok(response);
        }

        [HttpGet("officer")]
        [Authorize(Roles = Roles.GovernmentOfficer)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<List<ComplaintSummaryDto>>))]
        public async Task<IActionResult> GetOfficerQueue()
        {
            var userId = GetUserId();
            var response = await _complaintService.GetOfficerDashboardQueueAsync(userId);
            return Ok(response);
        }

        [HttpGet("worker")]
        [Authorize(Roles = Roles.FieldWorker)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<List<ComplaintSummaryDto>>))]
        public async Task<IActionResult> GetWorkerTasks()
        {
            var userId = GetUserId();
            var response = await _complaintService.GetWorkerTasksAsync(userId);
            return Ok(response);
        }

        [HttpGet("worker/localized")]
        [Authorize(Roles = Roles.FieldWorker)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<List<WorkerTaskSummaryDto>>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BaseResponse<List<WorkerTaskSummaryDto>>))]
        public async Task<IActionResult> GetWorkerTasksLocalized([FromQuery] decimal latitude, [FromQuery] decimal longitude)
        {
            if (latitude == 0 || longitude == 0)
            {
                return BadRequest(new BaseResponse<List<WorkerTaskSummaryDto>>("Invalid coordinates provided."));
            }

            var userId = GetUserId();
            var response = await _complaintService.GetWorkerTasksLocalizedAsync(userId, latitude, longitude);
            return Ok(response);
        }

        [HttpPost("{id}/assign")]
        [Authorize(Roles = Roles.GovernmentOfficer)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BaseResponse<string>))]
        public async Task<IActionResult> Assign(int id, [FromBody] AssignWorkerRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new BaseResponse<string>("Invalid payload submitted."));
            }

            var officerId = GetUserId();
            var response = await _complaintService.AssignWorkerAsync(id, officerId, request);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = Roles.FieldWorker + "," + Roles.GovernmentOfficer)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BaseResponse<string>))]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateComplaintStatusRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new BaseResponse<string>("Invalid payload submitted."));
            }

            var userId = GetUserId();
            var role = GetUserRole();

            var response = await _complaintService.UpdateStatusAsync(id, userId, role, request);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("{id}/feedback")]
        [Authorize(Roles = Roles.Citizen)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BaseResponse<string>))]
        public async Task<IActionResult> SubmitFeedback(int id, [FromBody] FeedbackRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new BaseResponse<string>("Invalid payload submitted."));
            }

            var userId = GetUserId();
            var response = await _complaintService.SubmitFeedbackAsync(id, userId, request);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
