namespace BLL.DTOs
{
    public class TokenDTO
    {
        public string Token { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }

        public AuthUserDTO User { get; set; } = null!;
    }
}
