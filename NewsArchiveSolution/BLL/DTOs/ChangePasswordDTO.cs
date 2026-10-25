using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs
{
    public class ChangePasswordDTO
    {
        [Required]
        public int UserId { get; set; }

        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(500, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 500 characters")]
        public string PasswordHash { get; set; } = null!;

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("PasswordHash", ErrorMessage = "Passwords do not match")]
        public string ConfPassword { get; set; } = null!;
    }
}
