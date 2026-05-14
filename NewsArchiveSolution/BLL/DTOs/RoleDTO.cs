using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs
{
    public class RoleDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Role name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Role name must be between 2 and 100 characters")]
        public string RoleName { get; set; } = null!;
    }
}
