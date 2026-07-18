using System;
using Greenix.Domain.Common;
using Greenix.Domain.Enums;

namespace Greenix.Domain.Entities
{
    public class ComplaintImage : BaseEntity
    {
        public int ComplaintId { get; set; }
        public virtual Complaint Complaint { get; set; } = null!;

        public string ImageUrl { get; set; } = string.Empty;
        public ImageType ImageType { get; set; }
        
        public string UploadedById { get; set; } = string.Empty;
        public virtual User UploadedBy { get; set; } = null!;
        
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
