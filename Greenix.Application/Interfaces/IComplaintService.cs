using System.Collections.Generic;
using System.Threading.Tasks;
using Greenix.Shared.Models;
using Greenix.Application.DTOs.Complaint;

namespace Greenix.Application.Interfaces
{
    public interface IComplaintService
    {
        Task<BaseResponse<ComplaintResponse>> CreateComplaintAsync(string citizenId, CreateComplaintRequest request);
        Task<BaseResponse<ComplaintResponse>> GetComplaintByIdAsync(int id, string userId, string role);
        Task<BaseResponse<List<ComplaintSummaryDto>>> GetCitizenComplaintsAsync(string citizenId);
        Task<BaseResponse<List<ComplaintSummaryDto>>> GetOfficerDashboardQueueAsync(string officerId);
        Task<BaseResponse<List<ComplaintSummaryDto>>> GetWorkerTasksAsync(string workerId);
        Task<BaseResponse<List<WorkerTaskSummaryDto>>> GetWorkerTasksLocalizedAsync(string workerId, decimal workerLatitude, decimal workerLongitude);
        Task<BaseResponse<string>> UpdateStatusAsync(int id, string userId, string role, UpdateComplaintStatusRequest request);
        Task<BaseResponse<string>> AssignWorkerAsync(int id, string officerId, AssignWorkerRequest request);
        Task<BaseResponse<string>> SubmitFeedbackAsync(int id, string citizenId, FeedbackRequest request);
    }
}
