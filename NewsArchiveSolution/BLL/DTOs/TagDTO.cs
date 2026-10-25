using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs
{
    public class TagDTO
    {
        public int TagId { get; set; }

        [Display(Name = "Name (English)")]
        [Required(ErrorMessage = "Tag name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Tag name must be between 2 and 50 characters")]
        public string Name { get; set; } = null!;

        [Display(Name = "Name (Bangla)")]
        [StringLength(50, ErrorMessage = "Tag name must be between 2 and 50 characters")]
        public string? NameBn { get; set; }

        public string? Slug { get; set; }

        public int ArticleCount { get; set; }

        public string LocalName(string language) =>
            language == AppLanguages.Bangla && !string.IsNullOrWhiteSpace(NameBn) ? NameBn : Name;
    }
}
