namespace BLL.DTOs
{
    public class AuthorDTO
    {
        public int UserId { get; set; }

        public string Name { get; set; } = null!;

        public string Slug { get; set; } = null!;

        public string? Bio { get; set; }

        public string? AvatarPath { get; set; }

        public int RoleId { get; set; }

        public string RoleName => AppRoles.Name(RoleId);

        public int ArticleCount { get; set; }

        public DateTime JoinedAt { get; set; }
    }
}
