namespace Greenix.Application.DTOs.Complaint
{
    public class AssignWorkerRequest
    {
        public string WorkerId { get; set; } = string.Empty;
        public byte Priority { get; set; }
        public string? Notes { get; set; }
    }
}
