using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Greenix.Domain.Entities
{
    public class User : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Foreign keys for municipal staff (Officers & Workers)
        public int? DepartmentId { get; set; }
        public virtual Department? Department { get; set; }

        // Navigation Properties
        public virtual ICollection<Complaint> CitizenComplaints { get; set; } = new List<Complaint>();
        public virtual ICollection<WorkerAssignment> WorkerAssignments { get; set; } = new List<WorkerAssignment>();
        public virtual ICollection<WorkerAssignment> CreatedAssignments { get; set; } = new List<WorkerAssignment>();
        public virtual ICollection<ComplaintImage> UploadedImages { get; set; } = new List<ComplaintImage>();
        public virtual ICollection<ComplaintStatusHistory> StatusChanges { get; set; } = new List<ComplaintStatusHistory>();
        public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    }
}
