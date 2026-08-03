namespace ApplicationLayer.Comments.DTOs
{
    public class CommentResponseDto
    {
        public int CommentId { get; set; }
        public int AdvertisementId { get; set; }
        public string Username { get; set; } = string.Empty;    
        public int? ParentCommentId { get; set; }
        public string Body { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
