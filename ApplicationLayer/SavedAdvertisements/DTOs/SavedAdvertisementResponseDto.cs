using ApplicationLayer.CatReport.DTOs;

namespace ApplicationLayer.SavedAdvertisements.DTOs
{
    public class SavedAdvertisementResponseDto
    {
        public int SavedAdvertisementId { get; set; }
        public int AccountId { get; set; }
        public int AdvertisementId { get; set; }
        public DateTime CreatedAt { get; set; }
        public PublicAdvertisementResponseDto? Advertisement { get; set; }
    }
}
