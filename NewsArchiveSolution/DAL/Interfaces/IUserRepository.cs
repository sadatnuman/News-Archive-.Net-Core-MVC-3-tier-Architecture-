using DAL.EF.Tables;

namespace DAL.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        User? GetByEmail(string? email);

        Dictionary<int, string> GetNames(IEnumerable<int> ids);

        bool UpdatePasswordHash(int userId, string passwordHash);
    }
}
