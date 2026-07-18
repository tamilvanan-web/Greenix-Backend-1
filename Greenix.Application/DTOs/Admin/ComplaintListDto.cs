using System;

namespace Greenix.Application.DTOs.Admin
{
    public class ComplaintListDto
    {
        public int Id { get; set; }

        public string ComplaintNumber { get; set; } = string.Empty;

        public string CitizenName { get; set; } = string.Empty;

        public string DepartmentName { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}