using Greenix.Application.DTOs.Admin;

namespace Greenix.Application.Interfaces
{
    public interface IOfficerService
    {
        Task<List<ComplaintListDto>> GetComplaintsAsync();

        Task<ComplaintDetailsDto?> GetComplaintByIdAsync(int id);

        Task<bool> UpdateComplaintStatusAsync(
            int complaintId,
            UpdateComplaintStatusDto request);
    }
}