using System.Text.RegularExpressions;
using BLL.Audit;
using BLL.DTOs;
using BLL.Interfaces;
using BLL.Security;
using DAL.EF.Tables;
using DAL.Interfaces;
using AutoMapper;

namespace BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository userRepo;
        private readonly ITokenRepository tokenRepo;
        private readonly IMapper mapper;
        private readonly IAuditService audit;

        public AuthService(IDataAccessFactory factory, IMapper mapper, IAuditService audit)
        {
            userRepo = factory.GetUserRepository();
            tokenRepo = factory.GetTokenRepository();
            this.mapper = mapper;
            this.audit = audit;
        }

        public AuthUserDTO? Authenticate(string? email, string? password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password)) return null;

            var user = userRepo.GetByEmail(email.Trim());
            if (user == null)
            {
                PasswordHasher.SimulateVerify(password);
                audit.Record(AuditActions.SignInFailed, AuditEntities.Security, null, "Unknown account " + AuditRules.MaskEmail(email));
                return null;
            }
            if (!PasswordHasher.Verify(password, user.PasswordHash, out var needsUpgrade))
            {
                audit.Record(AuditActions.SignInFailed, AuditEntities.Security, user.UserId, "Wrong password", user.UserId);
                return null;
            }

            if (needsUpgrade)
            {
                user.PasswordHash = PasswordHasher.Hash(password);
                userRepo.UpdatePasswordHash(user.UserId, user.PasswordHash);
            }

            audit.Record(AuditActions.SignedIn, AuditEntities.Security, user.UserId, needsUpgrade ? "Password hash upgraded" : null, user.UserId);
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
                CreatedAt = DateTime.Now
            };
            if (!userRepo.Create(user)) return null;

            audit.Record(AuditActions.AccountRegistered, AuditEntities.Security, user.UserId, user.Name, user.UserId);
            return mapper.Map<AuthUserDTO>(user);
        }

        public bool ChangePassword(int userId, string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword) || !Regex.IsMatch(newPassword, PasswordHasher.StrongPasswordPattern)) return false;
            if (!userRepo.UpdatePasswordHash(userId, PasswordHasher.Hash(newPassword))) return false;

            audit.Record(AuditActions.PasswordChanged, AuditEntities.Security, userId);
            return true;
        }

        public bool VerifyPassword(int userId, string? password)
        {
            var user = userRepo.Get(userId);
            if (user == null || string.IsNullOrEmpty(password))
            {
                PasswordHasher.SimulateVerify(password);
                return false;
            }
            if (PasswordHasher.Verify(password, user.PasswordHash, out _)) return true;

            audit.Record(AuditActions.PasswordChangeRejected, AuditEntities.Security, userId, "Wrong current password", userId);
            return false;
        }

        public TokenDTO? IssueToken(AuthUserDTO user, string clientType, TimeSpan lifetime)
        {
            var token = PasswordHasher.NewToken();
            var now = DateTime.UtcNow;
            var entity = new UserToken
            {
                UserId = user.UserId,
                TokenHash = PasswordHasher.HashToken(token),
                ClientType = clientType == TokenClients.Web ? TokenClients.Web : TokenClients.Api,
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

        public async Task<int> RevokeAllAsync(int userId)
        {
            var count = await tokenRepo.RevokeAllForUserAsync(userId);
            audit.Record(AuditActions.AllSessionsRevoked, AuditEntities.Security, userId, $"{count} tokens revoked");
            return count;
        }

        public int PurgeExpiredTokens()
        {
            return tokenRepo.DeleteExpired();
        }
    }
}
