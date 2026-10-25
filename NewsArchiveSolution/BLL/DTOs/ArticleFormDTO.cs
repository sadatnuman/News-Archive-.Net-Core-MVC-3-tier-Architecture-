using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs
{
    public class ArticleFormDTO
    {
        public int ArticleId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Summary is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Summary must be between 10 and 500 characters")]
        public string Summary { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required")]
        [StringLength(20000, MinimumLength = 20, ErrorMessage = "Content must be between 20 and 20000 characters")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Image path")]
        [StringLength(255, ErrorMessage = "Image path cannot exceed 255 characters")]
        public string? ImagePath { get; set; }

        [Display(Name = "Category")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category")]
        public int CategoryId { get; set; }

        [Display(Name = "Breaking news")]
        public bool IsBreakingNews { get; set; }

        public int StatusId { get; set; }

        public string? ReviewNote { get; set; }
    }
}
