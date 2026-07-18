using System.Collections.Generic;
using Greenix.Domain.Common;

namespace Greenix.Domain.Entities
{
    public class Category : AuditableEntity
    {
        public int DepartmentId { get; set; }
        public virtual Department Department { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Navigation Properties
        public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    }
}
