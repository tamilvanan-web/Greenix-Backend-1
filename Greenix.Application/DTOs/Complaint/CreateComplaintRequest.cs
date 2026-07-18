namespace Greenix.Application.DTOs.Complaint
{
    public class CreateComplaintRequest
    {
        public int CategoryId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? ImageUrl { get; set; }
    }
}
