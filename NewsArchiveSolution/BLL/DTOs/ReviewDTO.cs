using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs
{
    public class ReviewDTO
    {
        [StringLength(500, ErrorMessage = "Note cannot exceed 500 characters")]
        public string? Note { get; set; }
    }
}
