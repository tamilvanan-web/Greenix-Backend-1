namespace Greenix.Application.DTOs.Complaint
{
    public class UpdateComplaintStatusRequest
    {
        public byte Status { get; set; }
        public string? Comments { get; set; }
        public string? ImageUrl { get; set; }
    }
}
