using System;
using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs
{
    public class CommentDTO
    {
        public int CommentId { get; set; }

        [Required(ErrorMessage = "Article ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Article ID must be greater than 0")]
        public int ArticleId { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be greater than 0")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Comment text is required")]
        [StringLength(2000, MinimumLength = 5, ErrorMessage = "Comment must be between 5 and 2000 characters")]
        public string Text { get; set; } = null!;

        [Required(ErrorMessage = "Created date is required")]
        public DateTime CreatedDate { get; set; }
    }
}
