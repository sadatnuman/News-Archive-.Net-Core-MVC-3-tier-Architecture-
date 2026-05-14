using BLL.Validations;
using System;
using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs
{
    public class UserDTO
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [UniqueEmail]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(500, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 500 characters")]
        public string PasswordHash { get; set; } = null!;

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("PasswordHash", ErrorMessage = "Passwords do not match")]
        public string ConfPassword { get; set; } = null!;

        [Required(ErrorMessage = "Role is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Role ID must be greater than 0")]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Created date is required")]
        public DateTime CreatedAt { get; set; }
    }
}
