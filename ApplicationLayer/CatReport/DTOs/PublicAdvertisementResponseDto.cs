using ApplicationLayer.Cat.DTOs;
using ApplicationLayer.Location.DTOs;
using DomainLayer.Models;

namespace ApplicationLayer.CatReport.DTOs
{
    public class PublicAdvertisementResponseDto
    {
        public int AdvertisementId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ContactPhoneNumber { get; set; }
        public string? ContactEmail { get; set; }
        public PublicCatResponseDto? Cat { get; set; }
        public LocationResponseDto? Location { get; set; }
        public DateTime LastSeenAt { get; set; }
        public AdvertisementType Type { get; set; }
        public AdvertisementStatus Status { get; set; }
    }
}
