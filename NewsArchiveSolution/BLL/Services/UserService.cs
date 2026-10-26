using BLL.Audit;
using BLL.DTOs;
using BLL.Interfaces;
using BLL.Security;
using DAL.EF.Tables;
using DAL.Interfaces;
using AutoMapper;

namespace BLL.Services
{
    public class UserService : IUserService
    {
        public const int MaxBioLength = 500;

        private readonly IUserRepository repo;
        private readonly IMapper mapper;
        private readonly IAuditService audit;

        public UserService(IDataAccessFactory factory, IMapper mapper, IAuditService audit)
        {
            repo = factory.GetUserRepository();
            this.mapper = mapper;
            this.audit = audit;
        }

        private static string? CleanBio(string? bio)
        {
            var trimmed = bio?.Trim();
            if (string.IsNullOrEmpty(trimmed)) return null;
            return trimmed.Length > MaxBioLength ? trimmed[..MaxBioLength] : trimmed;
        }

        private static bool IsKnownRole(int roleId) =>
            roleId == AppRoles.Admin || roleId == AppRoles.Author || roleId == AppRoles.Reader;

        public List<UserDTO> Get()
        {
            return mapper.Map<List<UserDTO>>(repo.Get());
        }

        public UserDTO? Get(int id)
        {
            var data = repo.Get(id);
            return data == null ? null : mapper.Map<UserDTO>(data);
        }

        public bool Create(UserDTO c)
        {
            if (!IsKnownRole(c.RoleId) || string.IsNullOrWhiteSpace(c.PasswordHash)) return false;

            var converted = mapper.Map<User>(c);
            converted.Name = converted.Name.Trim();
            converted.Email = converted.Email.Trim();
            converted.PasswordHash = PasswordHasher.Hash(c.PasswordHash);
            converted.Bio = CleanBio(c.Bio);
            converted.CreatedAt = DateTime.Now;
            converted.AvatarPath = null;
            if (!repo.Create(converted)) return false;

            c.UserId = converted.UserId;
            audit.Record(AuditActions.UserCreated, AuditEntities.User, converted.UserId, $"{converted.Name} ({AppRoles.Name(converted.RoleId)})");
            return true;
        }

        public bool Update(UserDTO c)
        {
            if (!IsKnownRole(c.RoleId)) return false;
            var existing = repo.Get(c.UserId);
            if (existing == null) return false;

            var previousRole = existing.RoleId;
            existing.Name = c.Name.Trim();
            existing.RoleId = c.RoleId;
            existing.Bio = CleanBio(c.Bio);
            if (!repo.Update(existing)) return false;

            if (previousRole != existing.RoleId)
                audit.Record(AuditActions.UserRoleChanged, AuditEntities.User, existing.UserId,
                    $"{existing.Name}: {AppRoles.Name(previousRole)} to {AppRoles.Name(existing.RoleId)}");
            else
                audit.Record(AuditActions.UserUpdated, AuditEntities.User, existing.UserId, existing.Name);
            return true;
        }

        public List<UserDTO> Search(string? term)
        {
            var users = Get();
            if (string.IsNullOrWhiteSpace(term)) return users;
            term = term.Trim();
            return users
                .Where(u => u.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                            u.Email.Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public bool Delete(int id)
        {
            var user = repo.Get(id);
            if (user == null || !repo.Delete(id)) return false;

            audit.Record(AuditActions.UserDeleted, AuditEntities.User, id, $"{user.Name} ({AppRoles.Name(user.RoleId)})");
            return true;
        }

        public UserDTO? GetByEmail(string? email)
        {
            var data = repo.GetByEmail(email?.Trim());
            return data == null ? null : mapper.Map<UserDTO>(data);
        }

        public Dictionary<int, string> GetAvatars(IEnumerable<int> ids)
        {
            return repo.GetAvatars(ids);
        }

        public bool SetAvatar(int userId, string? avatarPath)
        {
            return repo.UpdateAvatar(userId, avatarPath);
        }

        public Dictionary<int, string> GetNames(IEnumerable<int> ids)
        {
            return repo.GetNames(ids);
        }
    }
}
