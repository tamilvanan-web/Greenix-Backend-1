using System.Collections.Generic;
using Greenix.Domain.Common;

namespace Greenix.Domain.Entities
{
    public class Department : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Navigation Properties
        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
        public virtual ICollection<User> Staff { get; set; } = new List<User>();
    }
}
