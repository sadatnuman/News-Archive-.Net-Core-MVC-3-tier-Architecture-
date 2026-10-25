using AutoMapper;
using BLL.DTOs;
using BLL.Security;
using DAL;
using DAL.EF.Tables;
using DAL.Interfaces;

namespace BLL.Services
{
    public class AuthService
    {
        public const string WebClient = "web";
        public const string ApiClient = "api";

        private readonly IUserRepository userRepo;
        private readonly ITokenRepository tokenRepo;
        private readonly IMapper mapper;

        public AuthService(DataAccessFactory factory, IMapper mapper)
        {
            userRepo = factory.GetUserRepository();
            tokenRepo = factory.GetTokenRepository();
            this.mapper = mapper;
        }

        public AuthUserDTO? Authenticate(string? email, string? password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password)) return null;

            var user = userRepo.GetByEmail(email.Trim());
            if (user == null)
            {
                PasswordHasher.SimulateVerify(password);
                return null;
            }
            if (!PasswordHasher.Verify(password, user.PasswordHash, out var needsUpgrade)) return null;

            if (needsUpgrade)
            {
                user.PasswordHash = PasswordHasher.Hash(password);
                userRepo.UpdatePasswordHash(user.UserId, user.PasswordHash);
            }

            return mapper.Map<AuthUserDTO>(user);
        }

        public AuthUserDTO? GetUser(int userId)
        {
            var user = userRepo.Get(userId);
            return user == null ? null : mapper.Map<AuthUserDTO>(user);
        }

        public AuthUserDTO? Register(RegisterDTO dto)
        {
            var email = dto.Email.Trim();
            if (userRepo.GetByEmail(email) != null) return null;

            var user = new User
            {
                Name = dto.Name.Trim(),
                Email = email,
                PasswordHash = PasswordHasher.Hash(dto.Password),
                RoleId = AppRoles.Reader,
                CreatedAt = DateTime.UtcNow
            };
            return userRepo.Create(user) ? mapper.Map<AuthUserDTO>(user) : null;
        }

        public bool ChangePassword(int userId, string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword) || !System.Text.RegularExpressions.Regex.IsMatch(newPassword, PasswordHasher.StrongPasswordPattern)) return false;
            return userRepo.UpdatePasswordHash(userId, PasswordHasher.Hash(newPassword));
        }

        public TokenDTO? IssueToken(AuthUserDTO user, string clientType, TimeSpan lifetime)
        {
            var token = PasswordHasher.NewToken();
            var now = DateTime.UtcNow;
            var entity = new UserToken
            {
                UserId = user.UserId,
                TokenHash = PasswordHasher.HashToken(token),
                ClientType = clientType == WebClient ? WebClient : ApiClient,
                CreatedAt = now,
                ExpiresAt = now.Add(lifetime)
            };
            if (!tokenRepo.Create(entity)) return null;

            return new TokenDTO { Token = token, ExpiresAt = entity.ExpiresAt, User = user };
        }

        public async Task<AuthUserDTO?> ValidateTokenAsync(string? token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;
            var entity = await tokenRepo.GetActiveByHashAsync(PasswordHasher.HashToken(token.Trim()));
            return entity == null ? null : mapper.Map<AuthUserDTO>(entity.User);
        }

        public Task<bool> LogoutAsync(string? token)
        {
            if (string.IsNullOrWhiteSpace(token)) return Task.FromResult(false);
            return tokenRepo.RevokeAsync(PasswordHasher.HashToken(token.Trim()));
        }

        public Task<int> RevokeAllAsync(int userId)
        {
            return tokenRepo.RevokeAllForUserAsync(userId);
        }

        public int PurgeExpiredTokens()
        {
            return tokenRepo.DeleteExpired();
        }
    }
}
