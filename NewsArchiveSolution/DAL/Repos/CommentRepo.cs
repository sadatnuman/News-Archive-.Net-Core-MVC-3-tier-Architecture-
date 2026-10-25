using DAL.EF;
using DAL.EF.Tables;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repos
{
    internal class CommentRepo : Repository<Comment>, ICommentRepository
    {
        public CommentRepo(NewsArchiveDbContext db) : base(db)
        {
        }

        public List<Comment> GetByArticleId(int articleId)
        {
            return db.Comments
                .Include(c => c.User)
                .Where(c => c.ArticleId == articleId)
                .OrderByDescending(c => c.CreatedDate)
                .ToList();
        }
    }
}
