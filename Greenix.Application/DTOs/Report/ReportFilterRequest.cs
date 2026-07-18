using System;

namespace Greenix.Application.DTOs.Report
{
    public class ReportFilterRequest
    {
        public byte? Status { get; set; }
        public byte? Priority { get; set; }
        public int? CategoryId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
