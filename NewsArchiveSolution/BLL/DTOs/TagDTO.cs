using System;
using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs
{
    public class TagDTO
    {
        public int TagId { get; set; }

        [Required(ErrorMessage = "Tag name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Tag name must be between 2 and 50 characters")]
        public string Name { get; set; } = null!;
    }
}
