using DAL.EF;
using DAL.EF.Tables;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repos
{
    internal sealed class TokenRepo : Repository<UserToken>, ITokenRepository
    {
        public TokenRepo(NewsArchiveDbContext db) : base(db)
        {
        }

        public Task<UserToken?> GetActiveByHashAsync(string tokenHash)
        {
            var now = DateTime.UtcNow;
            return db.UserTokens.AsNoTracking()
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && t.RevokedAt == null && t.ExpiresAt > now);
        }

        public async Task<bool> RevokeAsync(string tokenHash)
        {
            var now = DateTime.UtcNow;
            return await db.UserTokens
                .Where(t => t.TokenHash == tokenHash && t.RevokedAt == null)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now)) > 0;
        }

        public Task<int> RevokeAllForUserAsync(int userId)
        {
            var now = DateTime.UtcNow;
            return db.UserTokens
                .Where(t => t.UserId == userId && t.RevokedAt == null)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now));
        }

        public int DeleteExpired()
        {
            var cutoff = DateTime.UtcNow.AddDays(-30);
            return db.UserTokens
                .Where(t => t.ExpiresAt < cutoff || (t.RevokedAt != null && t.RevokedAt < cutoff))
                .ExecuteDelete();
        }
    }
}
