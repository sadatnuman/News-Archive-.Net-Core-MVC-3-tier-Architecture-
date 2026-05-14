using System;
using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs
{
    public class ArticleDTO
    {
        public int ArticleId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Summary is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Summary must be between 10 and 500 characters")]
        public string Summary { get; set; } = null!;

        [Required(ErrorMessage = "Content is required")]
        [StringLength(5000, MinimumLength = 20, ErrorMessage = "Content must be between 20 and 5000 characters")]
        public string Content { get; set; } = null!;

        [Required(ErrorMessage = "Publish date is required")]
        public DateTime PublishDate { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Views count cannot be negative")]
        public int ViewsCount { get; set; }

        public bool IsBreakingNews { get; set; }

        [StringLength(500, ErrorMessage = "Image path cannot exceed 500 characters")]
        public string? ImagePath { get; set; }

        [Required(ErrorMessage = "Category ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Category ID must be greater than 0")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Author ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Author ID must be greater than 0")]
        public int AuthorId { get; set; }
    }
}
