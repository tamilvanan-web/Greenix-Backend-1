using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Greenix.Shared.Models;
using Greenix.Shared.Constants;
using Greenix.Domain.Entities;
using Greenix.Domain.Enums;
using Greenix.Application.Interfaces;
using Greenix.Application.DTOs.Report;

namespace Greenix.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IApplicationDbContext _context;

        public ReportService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BaseResponse<byte[]>> ExportComplaintsToCsvAsync(ReportFilterRequest filters, string userId, string role)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return new BaseResponse<byte[]>("User record not found.");
            }

            var query = _context.Complaints
                .Include(c => c.Category)
                .Include(c => c.Citizen)
                .AsQueryable();

            // BR-01: Route and filter queries based on Department Scopes for Officers
            if (role == Roles.GovernmentOfficer && user.DepartmentId.HasValue)
            {
                query = query.Where(c => c.Category.DepartmentId == user.DepartmentId.Value);
            }

            // Apply Filters
            if (filters.Status.HasValue)
            {
                var statusEnum = (ComplaintStatus)filters.Status.Value;
                query = query.Where(c => c.Status == statusEnum);
            }

            if (filters.Priority.HasValue)
            {
                var priorityEnum = (PriorityLevel)filters.Priority.Value;
                query = query.Where(c => c.Priority == priorityEnum);
            }

            if (filters.CategoryId.HasValue)
            {
                query = query.Where(c => c.CategoryId == filters.CategoryId.Value);
            }

            if (filters.StartDate.HasValue)
            {
                query = query.Where(c => c.CreatedAt >= filters.StartDate.Value);
            }

            if (filters.EndDate.HasValue)
            {
                query = query.Where(c => c.CreatedAt <= filters.EndDate.Value);
            }

            var complaints = await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            // Formulate RFC 4180 CSV string
            var csvBuilder = new StringBuilder();
            
            // Header Row definition
            csvBuilder.AppendLine("ComplaintNumber,Category,Description,Status,Priority,CitizenName,CitizenEmail,Latitude,Longitude,CreatedAt,UpdatedAt");

            foreach (var complaint in complaints)
            {
                var row = $"\"{EscapeCsvValue(complaint.ComplaintNumber)}\"," +
                          $"\"{EscapeCsvValue(complaint.Category.Name)}\"," +
                          $"\"{EscapeCsvValue(complaint.Description)}\"," +
                          $"\"{EscapeCsvValue(complaint.Status.ToString())}\"," +
                          $"\"{EscapeCsvValue(complaint.Priority.ToString())}\"," +
                          $"\"{EscapeCsvValue(complaint.Citizen.FullName)}\"," +
                          $"\"{EscapeCsvValue(complaint.Citizen.Email ?? string.Empty)}\"," +
                          $"{complaint.Latitude}," +
                          $"{complaint.Longitude}," +
                          $"\"{complaint.CreatedAt:yyyy-MM-dd HH:mm:ss}\"," +
                          $"\"{(complaint.UpdatedAt.HasValue ? complaint.UpdatedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty)}\"";
                
                csvBuilder.AppendLine(row);
            }

            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            
            return new BaseResponse<byte[]>(bytes, "CSV data generated successfully.");
        }

        private static string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            
            // Escape double quotes inside values by doubling them
            return value.Replace("\"", "\"\"");
        }
    }
}
