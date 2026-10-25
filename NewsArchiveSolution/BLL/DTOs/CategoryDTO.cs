using System;
using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs
{
    public class CategoryDTO
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 100 characters")]
        public string Name { get; set; } = null!;

        [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
        public string? Description { get; set; }

        public string? Slug { get; set; }
    }
}
