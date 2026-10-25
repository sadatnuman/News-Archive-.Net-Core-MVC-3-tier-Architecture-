using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs
{
    public class CommentCreateDTO
    {
        [Required(ErrorMessage = "Comment text is required")]
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Comment must be between 2 and 500 characters")]
        public string Text { get; set; } = string.Empty;
    }
}
