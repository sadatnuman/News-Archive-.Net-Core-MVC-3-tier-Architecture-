using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs
{
    public class CategoryDTO
    {
        public int CategoryId { get; set; }

        [Display(Name = "Name (English)")]
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 100 characters")]
        public string Name { get; set; } = null!;

        [Display(Name = "Description (English)")]
        [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
        public string? Description { get; set; }

        [Display(Name = "Name (Bangla)")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        public string? NameBn { get; set; }

        [Display(Name = "Description (Bangla)")]
        [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
        public string? DescriptionBn { get; set; }

        public string? Slug { get; set; }

        public string LocalName(string language) =>
            language == AppLanguages.Bangla && !string.IsNullOrWhiteSpace(NameBn) ? NameBn : Name;

        public string? LocalDescription(string language) =>
            language == AppLanguages.Bangla && !string.IsNullOrWhiteSpace(DescriptionBn) ? DescriptionBn : Description;
    }
}
