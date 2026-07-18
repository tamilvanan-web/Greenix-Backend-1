using System;

namespace Greenix.Application.DTOs.Admin
{
    public class ComplaintDetailsDto
    {
        public int Id { get; set; }

        public string ComplaintNumber { get; set; } = string.Empty;

        public string CitizenName { get; set; } = string.Empty;

        public string CitizenEmail { get; set; } = string.Empty;

        public string DepartmentName { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}