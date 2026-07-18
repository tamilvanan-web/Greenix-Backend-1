using System;
using System.Collections.Generic;

namespace Greenix.Application.DTOs.Admin
{
    public class UserDetailsDto
    {
        public string Id { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public int TotalComplaints { get; set; }

        public List<UserComplaintDto> Complaints { get; set; } = new();
    }
}