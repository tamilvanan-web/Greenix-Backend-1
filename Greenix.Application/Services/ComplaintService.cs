using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Greenix.Shared.Models;
using Greenix.Shared.Constants;
using Greenix.Domain.Entities;
using Greenix.Domain.Enums;
using Greenix.Application.Interfaces;
using Greenix.Application.DTOs.Complaint;

namespace Greenix.Application.Services
{
    public class ComplaintService : IComplaintService
    {
        private readonly IApplicationDbContext _context;
        private readonly IGeospatialService _geospatialService;
        private readonly IEmailService _emailService;

        public ComplaintService(IApplicationDbContext context, IGeospatialService geospatialService, IEmailService emailService)
        {
            _context = context;
            _geospatialService = geospatialService;
            _emailService = emailService;
        }

        public async Task<BaseResponse<ComplaintResponse>> CreateComplaintAsync(string citizenId, CreateComplaintRequest request)
        {
            // Verify coordinates are within Chennai boundary limits
            if (!_geospatialService.IsWithinMunicipalBoundaries(request.Latitude, request.Longitude))
            {
                return new BaseResponse<ComplaintResponse>("Submission blocked. The specified coordinates are outside the municipal boundary limits of Chennai.");
            }

            var category = await _context.Categories
                .Include(c => c.Department)
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId);

            if (category == null)
            {
                return new BaseResponse<ComplaintResponse>("The selected category does not exist.");
            }

            // Generate unique professional complaint number: GRX-yyyyMMdd-XXXX (XXXX = 4-digit unique hex)
            var uniqueId = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
            var complaintNumber = $"GRX-{DateTime.UtcNow:yyyyMMdd}-{uniqueId}";

            var complaint = new Complaint
            {
                ComplaintNumber = complaintNumber,
                CitizenId = citizenId,
                CategoryId = request.CategoryId,
                Description = request.Description,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Status = ComplaintStatus.Submitted,
                Priority = PriorityLevel.Low,
                CreatedAt = DateTime.UtcNow
            };

            // If an image URL is supplied, add it as the initial submission photo
            if (!string.IsNullOrEmpty(request.ImageUrl))
            {
                complaint.Images.Add(new ComplaintImage
                {
                    ImageUrl = request.ImageUrl,
                    ImageType = ImageType.Submission,
                    UploadedById = citizenId,
                    UploadedAt = DateTime.UtcNow
                });
            }

            complaint.StatusHistories.Add(new ComplaintStatusHistory
            {
                Status = ComplaintStatus.Submitted,
                Comments = "Complaint submitted by citizen.",
                ChangedById = citizenId,
                ChangedAt = DateTime.UtcNow
            });

            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync();

            // Dispatch submission confirmation email to reporting citizen
            var citizen = await _context.Users.FirstOrDefaultAsync(u => u.Id == citizenId);
            if (citizen != null && !string.IsNullOrEmpty(citizen.Email))
            {
                await _emailService.SendEmailAsync(
                    citizen.Email,
                    $"Greenix - Complaint Submitted: {complaint.ComplaintNumber}",
                    $"Dear {citizen.FullName},\n\nYour civic grievance has been successfully recorded on the Greenix platform.\n\nComplaint Number: {complaint.ComplaintNumber}\nDescription: {complaint.Description}\nCategory: {category.Name}\n\nTrack status updates through your dashboard."
                );
            }

            return await GetComplaintByIdAsync(complaint.Id, citizenId, Roles.Citizen);
        }

        public async Task<BaseResponse<ComplaintResponse>> GetComplaintByIdAsync(int id, string userId, string role)
        {
            var complaint = await _context.Complaints
                .Include(c => c.Citizen)
                .Include(c => c.Category)
                    .ThenInclude(cat => cat.Department)
                .Include(c => c.Images)
                    .ThenInclude(img => img.UploadedBy)
                .Include(c => c.Assignments)
                    .ThenInclude(a => a.Worker)
                .Include(c => c.Assignments)
                    .ThenInclude(a => a.AssignedBy)
                .Include(c => c.StatusHistories)
                    .ThenInclude(h => h.ChangedBy)
                .Include(c => c.Feedback)
                    .ThenInclude(f => f!.Citizen)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (complaint == null)
            {
                return new BaseResponse<ComplaintResponse>("Complaint not found.");
            }

            // Role-based Access Control checks
            if (role == Roles.Citizen && complaint.CitizenId != userId)
            {
                return new BaseResponse<ComplaintResponse>("Access denied. You cannot view complaints submitted by other citizens.");
            }

            if (role == Roles.FieldWorker)
            {
                var isAssigned = complaint.Assignments.Any(a => a.WorkerId == userId);
                if (!isAssigned)
                {
                    return new BaseResponse<ComplaintResponse>("Access denied. You are not assigned to this complaint.");
                }
            }

            var response = new ComplaintResponse
            {
                Id = complaint.Id,
                ComplaintNumber = complaint.ComplaintNumber,
                CitizenName = complaint.Citizen.FullName,
                CitizenEmail = complaint.Citizen.Email ?? string.Empty,
                CategoryId = complaint.CategoryId,
                CategoryName = complaint.Category.Name,
                DepartmentName = complaint.Category.Department.Name,
                Description = complaint.Description,
                Latitude = complaint.Latitude,
                Longitude = complaint.Longitude,
                Status = complaint.Status.ToString(),
                Priority = complaint.Priority.ToString(),
                CreatedAt = complaint.CreatedAt,
                UpdatedAt = complaint.UpdatedAt,
                Images = complaint.Images.Select(img => new ComplaintImageDto
                {
                    Id = img.Id,
                    ImageUrl = img.ImageUrl,
                    ImageType = img.ImageType.ToString(),
                    UploadedByName = img.UploadedBy.FullName,
                    UploadedAt = img.UploadedAt
                }).ToList(),
                Assignments = complaint.Assignments.Select(a => new WorkerAssignmentDto
                {
                    Id = a.Id,
                    WorkerName = a.Worker.FullName,
                    WorkerPhone = a.Worker.PhoneNumber ?? string.Empty,
                    AssignedByName = a.AssignedBy.FullName,
                    AssignedAt = a.AssignedAt,
                    Notes = a.Notes
                }).ToList(),
                StatusHistories = complaint.StatusHistories.Select(h => new StatusHistoryDto
                {
                    Id = h.Id,
                    Status = h.Status.ToString(),
                    Comments = h.Comments,
                    ChangedByName = h.ChangedBy.FullName,
                    ChangedAt = h.ChangedAt
                }).OrderByDescending(h => h.ChangedAt).ToList()
            };

            if (complaint.Feedback != null)
            {
                response.Feedback = new FeedbackDto
                {
                    Id = complaint.Feedback.Id,
                    Rating = complaint.Feedback.Rating,
                    Comments = complaint.Feedback.Comments,
                    CreatedAt = complaint.Feedback.CreatedAt
                };
            }

            return new BaseResponse<ComplaintResponse>(response, "Complaint loaded successfully.");
        }

        public async Task<BaseResponse<List<ComplaintSummaryDto>>> GetCitizenComplaintsAsync(string citizenId)
        {
            var complaints = await _context.Complaints
                .Include(c => c.Category)
                .Where(c => c.CitizenId == citizenId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new ComplaintSummaryDto
                {
                    Id = c.Id,
                    ComplaintNumber = c.ComplaintNumber,
                    CategoryName = c.Category.Name,
                    Status = c.Status.ToString(),
                    Priority = c.Priority.ToString(),
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return new BaseResponse<List<ComplaintSummaryDto>>(complaints, "Complaints loaded successfully.");
        }

        public async Task<BaseResponse<List<ComplaintSummaryDto>>> GetOfficerDashboardQueueAsync(string officerId)
        {
            var officer = await _context.Users.FirstOrDefaultAsync(u => u.Id == officerId);
            if (officer == null)
            {
                return new BaseResponse<List<ComplaintSummaryDto>>("Officer record not found.");
            }

            var query = _context.Complaints.Include(c => c.Category).AsQueryable();

            // BR-01: Route based on department
            if (officer.DepartmentId.HasValue)
            {
                query = query.Where(c => c.Category.DepartmentId == officer.DepartmentId.Value);
            }

            var complaints = await query
                .OrderByDescending(c => c.Priority)
                .ThenByDescending(c => c.CreatedAt)
                .Select(c => new ComplaintSummaryDto
                {
                    Id = c.Id,
                    ComplaintNumber = c.ComplaintNumber,
                    CategoryName = c.Category.Name,
                    Status = c.Status.ToString(),
                    Priority = c.Priority.ToString(),
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return new BaseResponse<List<ComplaintSummaryDto>>(complaints, "Officer complaint queue loaded successfully.");
        }

        public async Task<BaseResponse<List<ComplaintSummaryDto>>> GetWorkerTasksAsync(string workerId)
        {
            var complaints = await _context.Complaints
                .Include(c => c.Category)
                .Where(c => c.Assignments.Any(a => a.WorkerId == workerId) && c.Status != ComplaintStatus.Closed && c.Status != ComplaintStatus.Rejected)
                .OrderByDescending(c => c.Priority)
                .ThenByDescending(c => c.CreatedAt)
                .Select(c => new ComplaintSummaryDto
                {
                    Id = c.Id,
                    ComplaintNumber = c.ComplaintNumber,
                    CategoryName = c.Category.Name,
                    Status = c.Status.ToString(),
                    Priority = c.Priority.ToString(),
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return new BaseResponse<List<ComplaintSummaryDto>>(complaints, "Worker task list loaded successfully.");
        }

        public async Task<BaseResponse<List<WorkerTaskSummaryDto>>> GetWorkerTasksLocalizedAsync(string workerId, decimal workerLatitude, decimal workerLongitude)
        {
            var complaints = await _context.Complaints
                .Include(c => c.Category)
                .Where(c => c.Assignments.Any(a => a.WorkerId == workerId) && c.Status != ComplaintStatus.Closed && c.Status != ComplaintStatus.Rejected)
                .ToListAsync();

            var taskSummaries = complaints.Select(c => new WorkerTaskSummaryDto
            {
                Id = c.Id,
                ComplaintNumber = c.ComplaintNumber,
                CategoryName = c.Category.Name,
                Status = c.Status.ToString(),
                Priority = c.Priority.ToString(),
                CreatedAt = c.CreatedAt,
                DistanceInKm = Math.Round(_geospatialService.CalculateDistance(workerLatitude, workerLongitude, c.Latitude, c.Longitude), 2)
            })
            .OrderBy(t => t.DistanceInKm)
            .ToList();

            return new BaseResponse<List<WorkerTaskSummaryDto>>(taskSummaries, "Worker localized task list loaded successfully.");
        }

        public async Task<BaseResponse<string>> UpdateStatusAsync(int id, string userId, string role, UpdateComplaintStatusRequest request)
        {
            var complaint = await _context.Complaints
                .Include(c => c.Citizen)
                .Include(c => c.Images)
                .Include(c => c.Assignments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (complaint == null)
            {
                return new BaseResponse<string>("Complaint not found.");
            }

            var targetStatus = (ComplaintStatus)request.Status;

            // Worker State Machine Restrictions
            if (role == Roles.FieldWorker)
            {
                var isAssigned = complaint.Assignments.Any(a => a.WorkerId == userId);
                if (!isAssigned)
                {
                    return new BaseResponse<string>("Access denied. You are not assigned to this complaint.");
                }

                // Enforce valid transitions: Assigned -> WorkInProgress, WorkInProgress -> Resolved
                if (complaint.Status == ComplaintStatus.Assigned && targetStatus == ComplaintStatus.WorkInProgress)
                {
                    // Allow transition, progress photo optional
                }
                else if (complaint.Status == ComplaintStatus.WorkInProgress && targetStatus == ComplaintStatus.Resolved)
                {
                    // BR-02: Work Verification Rule (mandatory photo upload)
                    var hasCompletionPhoto = complaint.Images.Any(img => img.ImageType == ImageType.Completion) 
                                             || (!string.IsNullOrEmpty(request.ImageUrl) && request.Status == (byte)ComplaintStatus.Resolved);
                    
                    if (!hasCompletionPhoto)
                    {
                        return new BaseResponse<string>("Validation failed. A completion photograph is mandatory to mark a complaint as resolved.");
                    }
                }
                else
                {
                    return new BaseResponse<string>($"Invalid state transition. Workers can only transition from Assigned to WorkInProgress, or WorkInProgress to Resolved.");
                }
            }

            // Officer State Machine Restrictions
            if (role == Roles.GovernmentOfficer)
            {
                // BR-03: Closure Authority Rule (only Officers can close resolved tickets)
                if (targetStatus == ComplaintStatus.Closed && complaint.Status != ComplaintStatus.Resolved)
                {
                    return new BaseResponse<string>("Invalid transition. Complaints can only be closed after they have been resolved by a field worker.");
                }
            }

            // Apply transition
            complaint.Status = targetStatus;
            complaint.UpdatedAt = DateTime.UtcNow;

            // If an image URL is supplied, attach it to the complaint
            if (!string.IsNullOrEmpty(request.ImageUrl))
            {
                var imgType = targetStatus == ComplaintStatus.Resolved ? ImageType.Completion : ImageType.Progress;
                complaint.Images.Add(new ComplaintImage
                {
                    ImageUrl = request.ImageUrl,
                    ImageType = imgType,
                    UploadedById = userId,
                    UploadedAt = DateTime.UtcNow
                });
            }

            complaint.StatusHistories.Add(new ComplaintStatusHistory
            {
                Status = targetStatus,
                Comments = request.Comments ?? $"Status updated to {targetStatus} by {role}.",
                ChangedById = userId,
                ChangedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            // Dispatch notification email to reporting citizen
            if (complaint.Citizen != null && !string.IsNullOrEmpty(complaint.Citizen.Email))
            {
                await _emailService.SendEmailAsync(
                    complaint.Citizen.Email,
                    $"Greenix - Status Update: {complaint.ComplaintNumber}",
                    $"Dear {complaint.Citizen.FullName},\n\nThe status of your complaint {complaint.ComplaintNumber} has been updated to: {targetStatus}.\n\nComments: {request.Comments ?? $"Status transitioned to {targetStatus}."}"
                );
            }

            return new BaseResponse<string>("Status updated successfully.", "Success");
        }

        public async Task<BaseResponse<string>> AssignWorkerAsync(int id, string officerId, AssignWorkerRequest request)
        {
            var complaint = await _context.Complaints
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (complaint == null)
            {
                return new BaseResponse<string>("Complaint not found.");
            }

            var worker = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.WorkerId);
            if (worker == null || !worker.IsActive)
            {
                return new BaseResponse<string>("The selected field worker does not exist or is suspended.");
            }

            // BR-05: Assignment Restriction (Worker must belong to same department as complaint category)
            if (worker.DepartmentId != complaint.Category.DepartmentId)
            {
                return new BaseResponse<string>("Assignment blocked. Field workers can only be assigned to complaints within their configured department.");
            }

            var assignment = new WorkerAssignment
            {
                ComplaintId = id,
                WorkerId = request.WorkerId,
                AssignedById = officerId,
                AssignedAt = DateTime.UtcNow,
                Notes = request.Notes
            };

            complaint.Assignments.Add(assignment);
            complaint.Status = ComplaintStatus.Assigned;
            complaint.Priority = (PriorityLevel)request.Priority;
            complaint.UpdatedAt = DateTime.UtcNow;

            complaint.StatusHistories.Add(new ComplaintStatusHistory
            {
                Status = ComplaintStatus.Assigned,
                Comments = $"Assigned to worker {worker.FullName} with priority {complaint.Priority}. Notes: {request.Notes}",
                ChangedById = officerId,
                ChangedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            // Dispatch notification email to assigned worker
            if (!string.IsNullOrEmpty(worker.Email))
            {
                await _emailService.SendEmailAsync(
                    worker.Email,
                    $"Greenix - New Task Assigned: {complaint.ComplaintNumber}",
                    $"Dear {worker.FullName},\n\nYou have been assigned a new task on the Greenix platform.\n\nComplaint Number: {complaint.ComplaintNumber}\nDescription: {complaint.Description}\nPriority: {complaint.Priority}\nLocation Coordinates: Lat {complaint.Latitude}, Lon {complaint.Longitude}\nNotes: {request.Notes ?? "No additional assignment notes."}"
                );
            }

            return new BaseResponse<string>("Field worker assigned successfully.", "Success");
        }

        public async Task<BaseResponse<string>> SubmitFeedbackAsync(int id, string citizenId, FeedbackRequest request)
        {
            var complaint = await _context.Complaints
                .Include(c => c.Citizen)
                .Include(c => c.Feedback)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (complaint == null)
            {
                return new BaseResponse<string>("Complaint not found.");
            }

            if (complaint.CitizenId != citizenId)
            {
                return new BaseResponse<string>("Access denied. You cannot submit feedback on complaints reported by other citizens.");
            }

            if (complaint.Status != ComplaintStatus.Closed)
            {
                return new BaseResponse<string>("Feedback can only be submitted for closed complaints.");
            }

            if (complaint.Feedback != null)
            {
                return new BaseResponse<string>("Feedback has already been submitted for this complaint.");
            }

            var feedback = new Feedback
            {
                ComplaintId = id,
                CitizenId = citizenId,
                Rating = request.Rating,
                Comments = request.Comments,
                CreatedAt = DateTime.UtcNow
            };

            complaint.Feedback = feedback;

            // BR-04: Re-Opening Rule (automatic reopen if rating is low: 1 or 2 stars)
            if (request.Rating <= 2)
            {
                complaint.Status = ComplaintStatus.UnderReview;
                complaint.UpdatedAt = DateTime.UtcNow;
                
                complaint.StatusHistories.Add(new ComplaintStatusHistory
                {
                    Status = ComplaintStatus.UnderReview,
                    Comments = $"Ticket automatically re-opened due to low rating ({request.Rating} stars). Citizen comments: {request.Comments}",
                    ChangedById = citizenId,
                    ChangedAt = DateTime.UtcNow
                });
                
                await _context.SaveChangesAsync();

                // Dispatch alert email to administration on low feedback reopening
                await _emailService.SendEmailAsync(
                    "support@greenix.gov",
                    $"Greenix Alert - Complaint Reopened: {complaint.ComplaintNumber}",
                    $"Attention Officer,\n\nCitizen {complaint.Citizen.FullName} has submitted a low rating of {request.Rating} stars on complaint {complaint.ComplaintNumber}.\n\nThe ticket has been automatically re-opened and placed in the Under Review queue.\n\nCitizen Comments: {request.Comments ?? "No comments provided."}"
                );

                return new BaseResponse<string>("Feedback submitted. The ticket has been automatically re-opened due to rating.", "Success (Re-opened)");
            }

            await _context.SaveChangesAsync();
            return new BaseResponse<string>("Feedback submitted successfully.", "Success");
        }
    }
}
