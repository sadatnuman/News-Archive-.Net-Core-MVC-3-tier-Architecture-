using BLL.Security;
using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs
{
    public class ChangePasswordDTO
    {
        [Required]
        public int UserId { get; set; }

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Current password")]
        [StringLength(500)]
        public string? CurrentPassword { get; set; }

        [Display(Name = "New password")]
        [Required(ErrorMessage = "Password is required")]
        [StringLength(500, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 500 characters")]
        [RegularExpression(PasswordHasher.StrongPasswordPattern, ErrorMessage = "Password must contain at least one letter and one number")]
        public string PasswordHash { get; set; } = null!;

        [Display(Name = "Confirm new password")]
        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("PasswordHash", ErrorMessage = "Passwords do not match")]
        public string ConfPassword { get; set; } = null!;
    }
}
