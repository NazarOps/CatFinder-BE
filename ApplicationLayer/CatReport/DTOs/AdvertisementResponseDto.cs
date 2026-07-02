using ApplicationLayer.Cat.DTOs;
using ApplicationLayer.Location.DTOs;
using ApplicationLayer.Reports.DTOs;
using DomainLayer.Models;
using DomainLayer.Models.Enum;

namespace ApplicationLayer.CatReport.DTOs
{
    public class AdvertisementResponseDto
    {
        public int AdvertisementId { get; set; }
        public int AccountId { get; set; }
        public int CatId { get; set; }
        public int LocationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ContactPhoneNumber { get; set; }
        public string? ContactEmail { get; set; }
        public CatResponseDto? Cat { get; set; }
        public LocationResponseDto? Location { get; set; }
        public DateTime LastSeenAt { get; set; }
        public AdvertisementType Type { get; set; }
        public AdvertisementStatus Status { get; set; }
        public ModerationStatus ModerationStatus { get; set; }
        public bool IsVisible { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<ReportResponseDto> Reports { get; set; } = new();
    }
}
