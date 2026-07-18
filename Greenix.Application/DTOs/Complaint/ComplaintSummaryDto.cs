using System;

namespace Greenix.Application.DTOs.Complaint
{
    public class ComplaintSummaryDto
    {
        public int Id { get; set; }
        public string ComplaintNumber { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
