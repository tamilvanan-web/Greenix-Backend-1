using System;
using System.Collections.Generic;
using Greenix.Domain.Common;
using Greenix.Domain.Enums;

namespace Greenix.Domain.Entities
{
    public class Complaint : BaseEntity
    {
        public string ComplaintNumber { get; set; } = string.Empty;
        public string CitizenId { get; set; } = string.Empty;
        public virtual User Citizen { get; set; } = null!;

        public int CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;

        public string Description { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public ComplaintStatus Status { get; set; } = ComplaintStatus.Submitted;
        public PriorityLevel Priority { get; set; } = PriorityLevel.Low;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public virtual ICollection<ComplaintImage> Images { get; set; } = new List<ComplaintImage>();
        public virtual ICollection<WorkerAssignment> Assignments { get; set; } = new List<WorkerAssignment>();
        public virtual ICollection<ComplaintStatusHistory> StatusHistories { get; set; } = new List<ComplaintStatusHistory>();
        public virtual Feedback? Feedback { get; set; }
    }
}
