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
using Greenix.Application.DTOs.Dashboard;

namespace Greenix.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IApplicationDbContext _context;

        public DashboardService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BaseResponse<DashboardSummaryResponse>> GetDashboardSummaryAsync(string userId, string role)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return new BaseResponse<DashboardSummaryResponse>("User record not found.");
            }

            var query = _context.Complaints
                .Include(c => c.Category)
                .AsQueryable();

            // BR-01: Auto-Routing/Department Scoping Rule.
            // If the caller is an officer, restrict results to their assigned department.
            if (role == Roles.GovernmentOfficer && user.DepartmentId.HasValue)
            {
                query = query.Where(c => c.Category.DepartmentId == user.DepartmentId.Value);
            }

            // 1. Calculate general counts
            var total = await query.CountAsync();
            var active = await query.CountAsync(c => 
                c.Status == ComplaintStatus.Submitted || 
                c.Status == ComplaintStatus.UnderReview || 
                c.Status == ComplaintStatus.Assigned || 
                c.Status == ComplaintStatus.WorkInProgress);
            
            var resolvedCount = await query.CountAsync(c => c.Status == ComplaintStatus.Resolved);
            var closedCount = await query.CountAsync(c => c.Status == ComplaintStatus.Closed);

            // 2. Groupings (Category, Status, Priority)
            var categoryStats = await query
                .GroupBy(c => c.Category.Name)
                .Select(g => new CategoryCountDto
                {
                    CategoryName = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var statusStats = await query
                .GroupBy(c => c.Status)
                .Select(g => new StatusCountDto
                {
                    Status = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToListAsync();

            var priorityStats = await query
                .GroupBy(c => c.Priority)
                .Select(g => new PriorityCountDto
                {
                    Priority = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToListAsync();

            // 3. Calculate Average Resolution Time in Hours
            // Query resolved timestamps using the status history logs
            var resolvedTimes = await query
                .Where(c => c.Status == ComplaintStatus.Resolved || c.Status == ComplaintStatus.Closed)
                .Select(c => new
                {
                    c.CreatedAt,
                    ResolvedAt = c.StatusHistories
                        .Where(h => h.Status == ComplaintStatus.Resolved)
                        .OrderBy(h => h.ChangedAt)
                        .Select(h => (DateTime?)h.ChangedAt)
                        .FirstOrDefault()
                })
                .ToListAsync();

            double averageResolutionHours = 0;
            if (resolvedTimes.Any())
            {
                var durations = resolvedTimes
                    .Where(x => x.ResolvedAt.HasValue)
                    .Select(x => (x.ResolvedAt!.Value - x.CreatedAt).TotalHours)
                    .ToList();

                if (durations.Any())
                {
                    averageResolutionHours = Math.Round(durations.Average(), 1);
                }
            }

            // 4. Calculate Average Feedback Rating
            var feedbackQuery = query.Where(c => c.Feedback != null).Select(c => (double)c.Feedback!.Rating);
            double averageRating = 0;
            if (await feedbackQuery.AnyAsync())
            {
                averageRating = Math.Round(await feedbackQuery.AverageAsync(), 1);
            }

            // 5. Fetch Recent 5 Complaints
            var recentComplaints = await query
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
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

            var response = new DashboardSummaryResponse
            {
                TotalComplaints = total,
                ActiveComplaints = active,
                ResolvedComplaints = resolvedCount,
                ClosedComplaints = closedCount,
                AverageResolutionTimeInHours = averageResolutionHours,
                AverageFeedbackRating = averageRating,
                ComplaintsByCategory = categoryStats,
                ComplaintsByStatus = statusStats,
                ComplaintsByPriority = priorityStats,
                RecentComplaints = recentComplaints
            };

            return new BaseResponse<DashboardSummaryResponse>(response, "Dashboard summary loaded successfully.");
        }
    }
}
