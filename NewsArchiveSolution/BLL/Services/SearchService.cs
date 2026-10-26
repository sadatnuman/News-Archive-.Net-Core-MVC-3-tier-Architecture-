using BLL.DTOs;
using BLL.Interfaces;
using DAL.Interfaces;
using AutoMapper;

namespace BLL.Services
{
    public class SearchService : ISearchService
    {
        private readonly IArticleRepository articleRepo;
        private readonly ITagRepository tagRepo;
        private readonly ICategoryRepository categoryRepo;
        private readonly IUserRepository userRepo;
        private readonly IMapper mapper;

        public SearchService(IDataAccessFactory factory, IMapper mapper)
        {
            articleRepo = factory.GetArticleRepository();
            tagRepo = factory.GetTagRepository();
            categoryRepo = factory.GetCategoryRepository();
            userRepo = factory.GetUserRepository();
            this.mapper = mapper;
        }

        public SearchSuggestionsDTO Suggest(string? query, string language, int storyCount = 6)
        {
            var q = SearchRules.NormalizeQuery(query);
            if (q == null) return new SearchSuggestionsDTO();

            var lang = AppLanguages.Normalize(language);
            var count = Math.Clamp(storyCount, 1, SearchRules.MaxStories);

            var sections = categoryRepo.Get()
                .Where(c => c.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                    || (c.NameBn != null && c.NameBn.Contains(q, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(c => c.Name.StartsWith(q, StringComparison.OrdinalIgnoreCase)
                    || (c.NameBn != null && c.NameBn.StartsWith(q, StringComparison.OrdinalIgnoreCase)))
                .ThenBy(c => c.Name)
                .Take(SearchRules.SectionCount);

            var authors = userRepo.SuggestAuthors(q, lang, SearchRules.AuthorCount)
                .Select(u => new AuthorDTO
                {
                    UserId = u.UserId,
                    Name = u.Name,
                    Slug = SlugHelper.Generate(u.Name),
                    AvatarPath = u.AvatarPath,
                    RoleId = u.RoleId,
                    JoinedAt = u.CreatedAt
                })
                .ToList();

            return new SearchSuggestionsDTO
            {
                Query = q,
                Stories = mapper.Map<List<ArticleDTO>>(articleRepo.Suggest(q, lang, count)),
                Topics = mapper.Map<List<TagDTO>>(tagRepo.Suggest(q, SearchRules.TopicCount)),
                Sections = mapper.Map<List<CategoryDTO>>(sections),
                Authors = authors
            };
        }
    }
}
