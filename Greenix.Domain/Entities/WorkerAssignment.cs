using System;
using Greenix.Domain.Common;

namespace Greenix.Domain.Entities
{
    public class WorkerAssignment : BaseEntity
    {
        public int ComplaintId { get; set; }
        public virtual Complaint Complaint { get; set; } = null!;

        public string WorkerId { get; set; } = string.Empty;
        public virtual User Worker { get; set; } = null!;

        public string AssignedById { get; set; } = string.Empty;
        public virtual User AssignedBy { get; set; } = null!;

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }
    }
}
