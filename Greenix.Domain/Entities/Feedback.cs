using System;
using Greenix.Domain.Common;

namespace Greenix.Domain.Entities
{
    public class Feedback : BaseEntity
    {
        public int ComplaintId { get; set; }
        public virtual Complaint Complaint { get; set; } = null!;

        public string CitizenId { get; set; } = string.Empty;
        public virtual User Citizen { get; set; } = null!;

        public byte Rating { get; set; }
        public string? Comments { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
