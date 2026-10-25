using BLL.Security;
using BLL.Validations;
using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs
{
    public class RegisterDTO
    {
        [Display(Name = "Name")]
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [UniqueEmail]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Password")]
        [Required(ErrorMessage = "Password is required")]
        [StringLength(500, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 500 characters")]
        [RegularExpression(PasswordHasher.StrongPasswordPattern, ErrorMessage = "Password must contain at least one letter and one number")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Confirm password")]
        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
