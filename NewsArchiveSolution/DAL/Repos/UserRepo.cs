using DAL.EF;
using DAL.EF.Tables;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repos
{
    internal sealed class UserRepo : Repository<User>, IUserRepository
    {
        public UserRepo(NewsArchiveDbContext db) : base(db)
        {
        }

        public override User? Get(int id)
        {
            return db.Users.AsNoTracking().FirstOrDefault(u => u.UserId == id);
        }

        public override bool Create(User entity)
        {
            try
            {
                return base.Create(entity);
            }
            catch (DbUpdateException ex) when (DbErrors.IsDuplicateKey(ex))
            {
                db.ChangeTracker.Clear();
                return false;
            }
        }

        public override bool Delete(int id)
        {
            using var transaction = db.Database.BeginTransaction();
            try
            {
                var existing = db.Users.Find(id);
                if (existing == null) return false;
                if (db.Articles.Any(a => a.AuthorId == id || a.ReviewedById == id)) return false;

                db.CommentReports.Where(r => r.UserId == id).ExecuteDelete();
                db.Comments.Where(c => c.ModeratedById == id).ExecuteUpdate(s => s.SetProperty(c => c.ModeratedById, (int?)null));
                db.Comments.Where(c => c.UserId == id).ExecuteDelete();
                db.Users.Remove(existing);
                db.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch (DbUpdateException ex) when (DbErrors.IsMissingReference(ex))
            {
                transaction.Rollback();
                db.ChangeTracker.Clear();
                return false;
            }
        }

        public bool UpdatePasswordHash(int userId, string passwordHash)
        {
            return db.Users
                .Where(u => u.UserId == userId)
                .ExecuteUpdate(s => s.SetProperty(u => u.PasswordHash, passwordHash)) > 0;
        }

        public bool UsesAvatar(string avatarPath)
        {
            return db.Users.Any(u => u.AvatarPath == avatarPath);
        }

        public bool UpdateAvatar(int userId, string? avatarPath)
        {
            return db.Users
                .Where(u => u.UserId == userId)
                .ExecuteUpdate(s => s.SetProperty(u => u.AvatarPath, avatarPath)) > 0;
        }

        public Dictionary<int, string> GetAvatars(IEnumerable<int> ids)
        {
            var idList = ids.Distinct().ToList();
            return db.Users.AsNoTracking()
                .Where(u => idList.Contains(u.UserId) && u.AvatarPath != null)
                .ToDictionary(u => u.UserId, u => u.AvatarPath!);
        }

        public User? GetByEmail(string? email)
        {
            if (string.IsNullOrEmpty(email)) return null;
            return db.Users.AsNoTracking().FirstOrDefault(u => u.Email == email);
        }

        public List<User> SuggestAuthors(string term, string? language, int count)
        {
            return db.Users.AsNoTracking()
                .Where(u => u.Name.Contains(term)
                    && u.Articles.Any(a => a.StatusId == ArticleStatusIds.Published && (language == null || a.LanguageCode == language)))
                .OrderByDescending(u => u.Name.StartsWith(term))
                .ThenBy(u => u.Name)
                .Take(count)
                .Select(u => new User
                {
                    UserId = u.UserId,
                    Name = u.Name,
                    Email = string.Empty,
                    PasswordHash = string.Empty,
                    AvatarPath = u.AvatarPath,
                    RoleId = u.RoleId,
                    CreatedAt = u.CreatedAt
                })
                .ToList();
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
