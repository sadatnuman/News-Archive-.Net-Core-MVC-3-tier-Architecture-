using AutoMapper;
using BLL.DTOs;
using DAL;
using DAL.EF.Tables;
using DAL.Interfaces;

namespace BLL.Services
{
    public class AuthorService
    {
        private readonly IArticleRepository articleRepo;
        private readonly IUserRepository userRepo;
        private readonly IMapper mapper;

        public AuthorService(DataAccessFactory factory, IMapper mapper)
        {
            articleRepo = factory.GetArticleRepository();
            userRepo = factory.GetUserRepository();
            this.mapper = mapper;
        }

        public List<AuthorDTO> GetAuthors(string? language = null)
        {
            var counts = articleRepo.CountPublishedByAuthor(language);
            if (counts.Count == 0) return new List<AuthorDTO>();

            return userRepo.Get()
                .Where(u => counts.ContainsKey(u.UserId))
                .Select(u => ToAuthor(u, counts[u.UserId]))
                .OrderByDescending(a => a.ArticleCount)
                .ThenBy(a => a.Name)
                .ToList();
        }

        public AuthorDTO? GetAuthor(int userId, string? language = null)
        {
            var user = userRepo.Get(userId);
            if (user == null) return null;

            var count = articleRepo.CountPublishedByAuthor(language).TryGetValue(userId, out var c) ? c : 0;
            var anyLanguage = count > 0 || articleRepo.CountPublishedByAuthor().ContainsKey(userId);
            return anyLanguage ? ToAuthor(user, count) : null;
        }

        public List<ArticleDTO> GetArticles(int userId, string? language, int page, int pageSize, out int total)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 12;
            return mapper.Map<List<ArticleDTO>>(articleRepo.GetPublishedByAuthor(userId, language, page, pageSize, out total));
        }

        private static AuthorDTO ToAuthor(User user, int count)
        {
            return new AuthorDTO
            {
                UserId = user.UserId,
                Name = user.Name,
                Slug = SlugHelper.Generate(user.Name),
                Bio = user.Bio,
                AvatarPath = user.AvatarPath,
                RoleId = user.RoleId,
                ArticleCount = count,
                JoinedAt = user.CreatedAt
            };
        }
    }
}
