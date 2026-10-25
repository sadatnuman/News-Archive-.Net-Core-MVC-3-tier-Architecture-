using DAL.EF;
using DAL.EF.Tables;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repos
{
    internal class UserRepo : Repository<User>, IUserRepository
    {
        public UserRepo(NewsArchiveDbContext db) : base(db)
        {
        }

        public override User? Get(int id)
        {
            return db.Users.AsNoTracking().FirstOrDefault(u => u.UserId == id);
        }

        public override bool Delete(int id)
        {
            var existing = db.Users.Find(id);
            if (existing == null) return false;
            if (db.Articles.Any(a => a.AuthorId == id || a.ReviewedById == id)) return false;

            db.Comments.Where(c => c.UserId == id).ExecuteDelete();
            db.Users.Remove(existing);
            return db.SaveChanges() > 0;
        }

        public bool UpdatePasswordHash(int userId, string passwordHash)
        {
            return db.Users
                .Where(u => u.UserId == userId)
                .ExecuteUpdate(s => s.SetProperty(u => u.PasswordHash, passwordHash)) > 0;
        }

        public User? GetByEmail(string? email)
        {
            if (string.IsNullOrEmpty(email)) return null;
            return db.Users.AsNoTracking().FirstOrDefault(u => u.Email == email);
        }

        public Dictionary<int, string> GetNames(IEnumerable<int> ids)
        {
            var idList = ids.Distinct().ToList();
            return db.Users.AsNoTracking()
                .Where(u => idList.Contains(u.UserId))
                .ToDictionary(u => u.UserId, u => u.Name);
        }
    }
}
