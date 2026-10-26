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
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Comment must be between 2 and 500 characters")]
        public string Text { get; set; } = null!;

        [Required(ErrorMessage = "Created date is required")]
        public DateTime CreatedDate { get; set; }

        public bool IsHidden { get; set; }

        public int? ModeratedById { get; set; }

        public DateTime? ModeratedAt { get; set; }
    }
}
