using Greenix.Application.DTOs.Admin;
using Greenix.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Greenix.Domain.Entities;

namespace Greenix.Infrastructure.Services
{
    public class AdminService : IAdminService
    {
        private readonly IApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public AdminService(
    IApplicationDbContext context,
    UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<ComplaintListDto>> GetAllComplaintsAsync()
        {
            return await _context.Complaints
                .Include(c => c.Citizen)
                .Include(c => c.Category)
                    .ThenInclude(c => c.Department)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new ComplaintListDto
                {
                    Id = c.Id,
                    ComplaintNumber = c.ComplaintNumber,
                    CitizenName = c.Citizen.FullName,
                    DepartmentName = c.Category.Department.Name,
                    CategoryName = c.Category.Name,
                    Priority = c.Priority.ToString(),
                    Status = c.Status.ToString(),
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<ComplaintDetailsDto?> GetComplaintByIdAsync(int id)
        {
            return await _context.Complaints
                .Include(c => c.Citizen)
                .Include(c => c.Category)
                    .ThenInclude(c => c.Department)
                .Where(c => c.Id == id)
                .Select(c => new ComplaintDetailsDto
                {
                    Id = c.Id,
                    ComplaintNumber = c.ComplaintNumber,
                    CitizenName = c.Citizen.FullName,
                    CitizenEmail = c.Citizen.Email ?? string.Empty,
                    DepartmentName = c.Category.Department.Name,
                    CategoryName = c.Category.Name,
                    Description = c.Description,
                    Priority = c.Priority.ToString(),
                    Status = c.Status.ToString(),
                    Latitude = c.Latitude,
                    Longitude = c.Longitude,
                    CreatedAt = c.CreatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateComplaintStatusAsync(
    int complaintId,
    UpdateComplaintStatusDto request)
        {
            var complaint = await _context.Complaints
                .FirstOrDefaultAsync(c => c.Id == complaintId);

            if (complaint == null)
            {
                return false;
            }

            complaint.Status = request.Status;
            complaint.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<UserListDto>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .ToListAsync();

            var result = new List<UserListDto>();

            foreach (var user in users)
            {
                result.Add(new UserListDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    IsActive = user.IsActive,
                    Role = "Citizen", // We'll improve this in the next step
                    ComplaintCount = await _context.Complaints
                        .CountAsync(c => c.CitizenId == user.Id)
                });
            }

            return result;
        }


        public async Task<UserDetailsDto?> GetUserByIdAsync(string userId)
        {
            var user = await _context.Users
                .Include(u => u.CitizenComplaints)
                    .ThenInclude(c => c.Category)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDetailsDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Role = roles.FirstOrDefault() ?? "No Role",
                IsActive = user.IsActive,
                TotalComplaints = user.CitizenComplaints.Count,

                Complaints = user.CitizenComplaints
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new UserComplaintDto
                    {
                        Id = c.Id,
                        ComplaintNumber = c.ComplaintNumber,
                        Category = c.Category.Name,
                        Status = c.Status.ToString(),
                        Priority = c.Priority.ToString(),
                        CreatedAt = c.CreatedAt
                    })
                    .ToList()
            };
        }







    }
}