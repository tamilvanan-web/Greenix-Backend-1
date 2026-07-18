using System;
using System.Collections.Generic;

namespace Greenix.Application.DTOs.Complaint
{
    public class ComplaintResponse
    {
        public int Id { get; set; }
        public string ComplaintNumber { get; set; } = string.Empty;
        public string CitizenName { get; set; } = string.Empty;
        public string CitizenEmail { get; set; } = string.Empty;
        
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<ComplaintImageDto> Images { get; set; } = new List<ComplaintImageDto>();
        public List<WorkerAssignmentDto> Assignments { get; set; } = new List<WorkerAssignmentDto>();
        public List<StatusHistoryDto> StatusHistories { get; set; } = new List<StatusHistoryDto>();
        public FeedbackDto? Feedback { get; set; }
    }

    public class ComplaintImageDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string ImageType { get; set; } = string.Empty;
        public string UploadedByName { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }

    public class WorkerAssignmentDto
    {
        public int Id { get; set; }
        public string WorkerName { get; set; } = string.Empty;
        public string WorkerPhone { get; set; } = string.Empty;
        public string AssignedByName { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
        public string? Notes { get; set; }
    }

    public class StatusHistoryDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Comments { get; set; }
        public string ChangedByName { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
    }

    public class FeedbackDto
    {
        public int Id { get; set; }
        public byte Rating { get; set; }
        public string? Comments { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
