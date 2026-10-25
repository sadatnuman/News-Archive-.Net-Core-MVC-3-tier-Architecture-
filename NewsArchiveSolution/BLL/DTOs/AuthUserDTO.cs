using System.Text.Json.Serialization;

namespace BLL.DTOs
{
    public class AuthUserDTO
    {
        public int UserId { get; set; }

        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        public int RoleId { get; set; }

        public string RoleName { get; set; } = null!;

        public string? AvatarPath { get; set; }

        [JsonIgnore]
        public string SecurityStamp { get; set; } = string.Empty;
    }
}
