using Greenix.Application.DTOs.Admin;

namespace Greenix.Application.Interfaces
{
    public interface IAdminService
    {
        Task<List<ComplaintListDto>> GetAllComplaintsAsync();

        Task<ComplaintDetailsDto?> GetComplaintByIdAsync(int id);

        Task<bool> UpdateComplaintStatusAsync(int complaintId, UpdateComplaintStatusDto request);

        Task<List<UserListDto>> GetAllUsersAsync();

        Task<UserDetailsDto?> GetUserByIdAsync(string userId);
    }
}