using DAL.EF.Tables;

namespace DAL.Interfaces
{
    public interface ITokenRepository : IRepository<UserToken>
    {
        Task<UserToken?> GetActiveByHashAsync(string tokenHash);

        Task<bool> RevokeAsync(string tokenHash);

        Task<int> RevokeAllForUserAsync(int userId);

        int DeleteExpired();
    }
}
