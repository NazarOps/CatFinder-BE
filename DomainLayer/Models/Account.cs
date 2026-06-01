using System.ComponentModel.DataAnnotations;

namespace DomainLayer.Models
{
    public class Account
    {
        public int AccountId { get; set; }

        [Required]
        [MaxLength(50)]
        [MinLength(3)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        public string? PasswordResetCode { get; set; }
        public DateTime? PasswordResetCodeExpiresAt { get; set; }

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public Role Role { get; set; }

        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<Advertisement> Advertisements { get; set; } = new List<Advertisement>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<SavedAdvertisement> SavedAdvertisements { get; set; } = new List<SavedAdvertisement>();
        public virtual ICollection<Cat> Cats { get; set; } = new List<Cat>();
    }
}
