namespace ApplicationLayer.Reports.DTOs
{
    public class ReportResponseDto
    {
        public int ReportId { get; set; }
        public int AdvertisementId { get; set; }
        public int? CommentId { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
