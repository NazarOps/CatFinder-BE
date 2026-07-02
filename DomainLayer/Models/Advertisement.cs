using DomainLayer.Models.Enum;
using System.ComponentModel.DataAnnotations;

namespace DomainLayer.Models
{
    public class Advertisement
    {
        public int AdvertisementId { get; set; }

        public int AccountId { get; set; }

        public int CatId { get; set; }

        public int LocationId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Phone]
        [MaxLength(20)]
        public string? ContactPhoneNumber { get; set; }

        [EmailAddress]
        [MaxLength(256)]
        public string? ContactEmail { get; set; }

        public DateTime LastSeenAt { get; set; }

        public AdvertisementType Type { get; set; }

        public AdvertisementStatus Status { get; set; }

        public ModerationStatus ModerationStatus { get; set; } = ModerationStatus.Pending;

        public bool IsVisible { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public virtual Account Account { get; set; } = null!;
        public virtual Cat Cat { get; set; } = null!;
        public virtual Location Location { get; set; } = null!;
        public virtual ICollection<AdvertisementImage> AdvertisementImages { get; set; } = new List<AdvertisementImage>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<SavedAdvertisement> SavedAdvertisements { get; set; } = new List<SavedAdvertisement>();
        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}
