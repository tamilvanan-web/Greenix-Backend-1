using System.Collections.Generic;
using Greenix.Application.DTOs.Complaint;

namespace Greenix.Application.DTOs.Dashboard
{
    public class DashboardSummaryResponse
    {
        public int TotalComplaints { get; set; }
        public int ActiveComplaints { get; set; }
        public int ResolvedComplaints { get; set; }
        public int ClosedComplaints { get; set; }
        public double AverageResolutionTimeInHours { get; set; }
        public double AverageFeedbackRating { get; set; }
        
        public List<CategoryCountDto> ComplaintsByCategory { get; set; } = new List<CategoryCountDto>();
        public List<StatusCountDto> ComplaintsByStatus { get; set; } = new List<StatusCountDto>();
        public List<PriorityCountDto> ComplaintsByPriority { get; set; } = new List<PriorityCountDto>();
        public List<ComplaintSummaryDto> RecentComplaints { get; set; } = new List<ComplaintSummaryDto>();
    }

    public class CategoryCountDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class StatusCountDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class PriorityCountDto
    {
        public string Priority { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
