using DAL.EF.Tables;

namespace DAL.Interfaces
{
    public interface ICommentRepository : IRepository<Comment>
    {
        List<Comment> GetByArticleId(int articleId);
    }
}
