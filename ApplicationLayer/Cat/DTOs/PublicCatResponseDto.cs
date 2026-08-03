using DomainLayer.Models;

namespace ApplicationLayer.Cat.DTOs
{
    public class PublicCatResponseDto
    {
        public int CatId { get; set; }
        public string? Name { get; set; }
        public string? Breed { get; set; }
        public string FurColor { get; set; } = string.Empty;
        public int? Age { get; set; }
        public CatGender? Gender { get; set; }
        public bool IsChipped { get; set; }
        public bool IsRegistered { get; set; }
        public string? Description { get; set; }
    }
}
