using System;

namespace Greenix.Application.DTOs.Admin
{
    public class UserComplaintDto
    {
        public int Id { get; set; }

        public string ComplaintNumber { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}