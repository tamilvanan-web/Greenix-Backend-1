using System;
using Greenix.Domain.Common;
using Greenix.Domain.Enums;

namespace Greenix.Domain.Entities
{
    public class ComplaintStatusHistory : BaseEntity
    {
        public int ComplaintId { get; set; }
        public virtual Complaint Complaint { get; set; } = null!;

        public ComplaintStatus Status { get; set; }
        public string? Comments { get; set; }

        public string ChangedById { get; set; } = string.Empty;
        public virtual User ChangedBy { get; set; } = null!;

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
