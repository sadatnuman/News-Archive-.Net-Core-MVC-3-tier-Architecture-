using AutoMapper;
using BLL.DTOs;
using DAL;
using DAL.Interfaces;

namespace BLL.Services
{
    public class BookmarkService
    {
        public const int MaxBookmarks = 500;

        private readonly IBookmarkRepository repo;
        private readonly IArticleRepository articleRepo;
        private readonly IMapper mapper;

        public BookmarkService(DataAccessFactory factory, IMapper mapper)
        {
            repo = factory.GetBookmarkRepository();
            articleRepo = factory.GetArticleRepository();
            this.mapper = mapper;
        }

        public bool Add(int userId, int articleId)
        {
            var article = articleRepo.Get(articleId);
            if (article == null || article.StatusId != ArticleStatus.Published) return false;
            if (!repo.Exists(userId, articleId) && repo.Count(userId) >= MaxBookmarks) return false;
            return repo.Add(userId, articleId);
        }

        public bool Remove(int userId, int articleId)
        {
            return repo.Remove(userId, articleId);
        }

        public bool Toggle(int userId, int articleId, out bool saved)
        {
            if (repo.Exists(userId, articleId))
            {
                saved = false;
                return repo.Remove(userId, articleId);
            }
            saved = Add(userId, articleId);
            return saved;
        }

        public bool IsSaved(int userId, int articleId)
        {
            return repo.Exists(userId, articleId);
        }

        public HashSet<int> GetSavedIds(int userId, IEnumerable<int> articleIds)
        {
            return repo.GetArticleIds(userId, articleIds);
        }

        public List<ArticleDTO> GetSaved(int userId, int page, int pageSize, out int total)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 12;
            return mapper.Map<List<ArticleDTO>>(repo.GetArticles(userId, page, pageSize, out total));
        }
    }
}
