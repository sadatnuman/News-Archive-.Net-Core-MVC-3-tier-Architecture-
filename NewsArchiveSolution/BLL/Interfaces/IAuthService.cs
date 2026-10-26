using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface IAuthService
    {
        AuthUserDTO? Authenticate(string? email, string? password);

        AuthUserDTO? GetUser(int userId);

        AuthUserDTO? Register(RegisterDTO dto);

        bool ChangePassword(int userId, string newPassword);

        bool VerifyPassword(int userId, string? password);

        TokenDTO? IssueToken(AuthUserDTO user, string clientType, TimeSpan lifetime);

        Task<AuthUserDTO?> ValidateTokenAsync(string? token);

        Task<bool> LogoutAsync(string? token);

        Task<int> RevokeAllAsync(int userId);

        int PurgeExpiredTokens();
    }
}
