using DAL.EF;
using DAL.EF.Tables;
using DAL.Interfaces;
using DAL.Repos;

namespace DAL
{
    public class DataAccessFactory
    {
        private readonly NewsArchiveDbContext db;

        public DataAccessFactory(NewsArchiveDbContext db)
        {
            this.db = db;
        }

        public IArticleRepository GetArticleRepository() => new ArticleRepo(db);

        public ICategoryRepository GetCategoryRepository() => new CategoryRepo(db);

        public ICommentRepository GetCommentRepository() => new CommentRepo(db);

        public IUserRepository GetUserRepository() => new UserRepo(db);

        public ITokenRepository GetTokenRepository() => new TokenRepo(db);

        public IRepository<Role> GetRoleRepository() => new Repository<Role>(db);

        public ITagRepository GetTagRepository() => new TagRepo(db);
    }
}
