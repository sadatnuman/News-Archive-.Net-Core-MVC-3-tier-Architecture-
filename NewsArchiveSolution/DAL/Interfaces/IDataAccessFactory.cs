using DAL.EF.Tables;

namespace DAL.Interfaces
{
    public interface IDataAccessFactory
    {
        bool CanConnect();

        IArticleRepository GetArticleRepository();

        ICategoryRepository GetCategoryRepository();

        ICommentRepository GetCommentRepository();

        IUserRepository GetUserRepository();

        ITokenRepository GetTokenRepository();

        IRepository<Role> GetRoleRepository();

        ITagRepository GetTagRepository();

        IBookmarkRepository GetBookmarkRepository();

        IReadingHistoryRepository GetReadingHistoryRepository();

        IAuditLogRepository GetAuditLogRepository();

        ISubscriberRepository GetSubscriberRepository();
    }
}
