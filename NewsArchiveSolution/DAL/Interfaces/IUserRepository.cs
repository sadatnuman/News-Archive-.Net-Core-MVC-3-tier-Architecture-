using DAL.EF.Tables;

namespace DAL.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        User? GetByEmail(string? email);

        Dictionary<int, string> GetNames(IEnumerable<int> ids);

        Dictionary<int, string> GetAvatars(IEnumerable<int> ids);

        bool UpdatePasswordHash(int userId, string passwordHash);

        bool UpdateAvatar(int userId, string? avatarPath);
    }
}
