using BLL.DTOs;
using BLL.Interfaces;
using DAL.EF.Tables;
using DAL.Interfaces;
using AutoMapper;

namespace BLL.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly IArticleRepository articleRepo;
        private readonly IReadingHistoryRepository historyRepo;
        private readonly IMapper mapper;

        public RecommendationService(IDataAccessFactory factory, IMapper mapper)
        {
            articleRepo = factory.GetArticleRepository();
            historyRepo = factory.GetReadingHistoryRepository();
            this.mapper = mapper;
        }

        private List<int> RecentIds(int? userId, IEnumerable<int>? recentIds)
        {
            return userId.HasValue
                ? historyRepo.GetRecentArticleIds(userId.Value, RecommendationRules.ProfileSize)
                : (recentIds ?? Enumerable.Empty<int>()).Where(id => id > 0).Distinct().Take(RecommendationRules.ProfileSize).ToList();
        }

        private List<RecommendationDTO> Rank(IEnumerable<(Article Article, double Score, RecommendationReason Reason, int? TagId)> scored, int count)
        {
            return scored
                .OrderByDescending(s => s.Score)
                .ThenByDescending(s => s.Article.PublishDate)
                .Take(count)
                .Select(s => new RecommendationDTO
                {
                    Article = mapper.Map<ArticleDTO>(s.Article),
                    Reason = s.Reason,
                    ReasonTagId = s.TagId,
                    Score = Math.Round(s.Score, 3)
                })
                .ToList();
        }

        private static double Freshness(Article article, DateTime now)
        {
            return RecommendationRules.RecencyWeight * RecommendationRules.Recency(article.PublishDate, now)
                + RecommendationRules.PopularityWeight * RecommendationRules.Popularity(article.ViewsCount)
                + (article.IsBreakingNews ? RecommendationRules.BreakingBonus : 0);
        }

        public List<RecommendationDTO> ForArticle(int articleId, int count, int? userId = null, IEnumerable<int>? recentIds = null)
        {
            count = Math.Clamp(count, 1, RecommendationRules.MaxCount);
            var source = articleRepo.GetManyWithTags(new[] { articleId }).FirstOrDefault();
            if (source == null || source.StatusId != ArticleStatus.Published) return new List<RecommendationDTO>();

            var read = RecentIds(userId, recentIds).ToHashSet();
            var sourceTags = source.Tags.Select(t => t.TagId).ToHashSet();
            var sourceWords = RecommendationRules.Keywords(source.Title, source.Summary);
            var now = DateTime.Now;

            var candidates = articleRepo.GetRecommendationCandidates(source.LanguageCode, new[] { articleId },
                new[] { source.CategoryId }, sourceTags, new[] { source.AuthorId },
                RecommendationRules.RelatedPool, RecommendationRules.PopularPool);

            var scored = candidates.Select(c =>
            {
                var shared = c.Tags.Where(t => sourceTags.Contains(t.TagId)).Select(t => t.TagId).ToList();
                var tag = RecommendationRules.TagWeight * shared.Count;
                var text = RecommendationRules.TextWeight * RecommendationRules.Similarity(sourceWords, RecommendationRules.Keywords(c.Title, c.Summary));
                var category = c.CategoryId == source.CategoryId ? RecommendationRules.CategoryWeight : 0;
                var author = c.AuthorId == source.AuthorId ? RecommendationRules.AuthorWeight : 0;

                var score = tag + text + category + author + Freshness(c, now);
                if (read.Contains(c.ArticleId)) score *= RecommendationRules.AlreadyReadFactor;

                var reason = RecommendationRules.PickReason(tag, text, category, author);
                int? tagId = reason == RecommendationReason.SharedTopic ? shared[0] : null;
                return (c, score, reason, tagId);
            });

            return Rank(scored, count);
        }

        public List<RecommendationDTO> ForReader(int? userId, IEnumerable<int>? recentIds, string language, int count, IEnumerable<int>? excludeIds = null, int minHistory = 1)
        {
            count = Math.Clamp(count, 1, RecommendationRules.MaxCount);
            var recent = RecentIds(userId, recentIds);
            if (recent.Count == 0 || recent.Count < minHistory) return new List<RecommendationDTO>();

            var position = recent.Select((id, i) => (id, i)).ToDictionary(x => x.id, x => x.i);
            var readArticles = articleRepo.GetManyWithTags(recent);
            if (readArticles.Count == 0) return new List<RecommendationDTO>();

            var tagAffinity = new Dictionary<int, double>();
            var categoryAffinity = new Dictionary<int, double>();
            var authorAffinity = new Dictionary<int, double>();
            var profileWords = new HashSet<string>();
            var total = 0.0;

            foreach (var a in readArticles)
            {
                var pos = position.TryGetValue(a.ArticleId, out var p) ? p : recent.Count;
                var w = RecommendationRules.ProfileWeight(pos);
                total += w;
                foreach (var t in a.Tags)
                    tagAffinity[t.TagId] = tagAffinity.GetValueOrDefault(t.TagId) + w;
                categoryAffinity[a.CategoryId] = categoryAffinity.GetValueOrDefault(a.CategoryId) + w;
                authorAffinity[a.AuthorId] = authorAffinity.GetValueOrDefault(a.AuthorId) + w;
                if (pos < 5)
                    profileWords.UnionWith(RecommendationRules.Keywords(a.Title, a.Summary));
            }

            foreach (var map in new[] { tagAffinity, categoryAffinity, authorAffinity })
                foreach (var key in map.Keys.ToList())
                    map[key] /= total;

            var topTags = tagAffinity.OrderByDescending(x => x.Value).Take(15).Select(x => x.Key).ToList();
            var now = DateTime.Now;
            var exclude = recent.Concat(excludeIds ?? Enumerable.Empty<int>()).ToList();

            var candidates = articleRepo.GetRecommendationCandidates(AppLanguages.Normalize(language), exclude,
                categoryAffinity.Keys, topTags, authorAffinity.Keys,
                RecommendationRules.RelatedPool + 20, RecommendationRules.PopularPool);

            var scored = candidates.Select(c =>
            {
                var matched = c.Tags
                    .Where(t => tagAffinity.ContainsKey(t.TagId))
                    .OrderByDescending(t => tagAffinity[t.TagId])
                    .ToList();
                var tag = 2 * RecommendationRules.TagWeight * matched.Take(3).Sum(t => tagAffinity[t.TagId]);
                var category = 2 * RecommendationRules.CategoryWeight * categoryAffinity.GetValueOrDefault(c.CategoryId);
                var author = RecommendationRules.AuthorWeight * authorAffinity.GetValueOrDefault(c.AuthorId);
                var text = RecommendationRules.TextWeight * RecommendationRules.Similarity(profileWords, RecommendationRules.Keywords(c.Title, c.Summary));

                var score = tag + category + author + text + Freshness(c, now);
                var reason = RecommendationRules.PickReason(tag, text, category, author);
                int? tagId = reason == RecommendationReason.SharedTopic ? matched[0].TagId : null;
                return (c, score, reason, tagId);
            });

            return Rank(scored, count);
        }

        public bool RecordRead(int userId, int articleId)
        {
            var article = articleRepo.Get(articleId);
            if (article == null || article.StatusId != ArticleStatus.Published) return false;

            historyRepo.Record(userId, articleId, DateTime.Now);
            if (historyRepo.Count(userId) > RecommendationRules.HistorySize)
                historyRepo.Trim(userId, RecommendationRules.HistorySize);
            return true;
        }

        public List<int> GetRecentIds(int userId, int count)
        {
            return historyRepo.GetRecentArticleIds(userId, Math.Clamp(count, 1, RecommendationRules.HistorySize));
        }

        public List<ReadingHistoryEntryDTO> GetHistory(int userId, int page, int pageSize, out int total)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 12;
            return historyRepo.GetEntries(userId, page, pageSize, out total)
                .Select(h => new ReadingHistoryEntryDTO
                {
                    Article = mapper.Map<ArticleDTO>(h.Article),
                    FirstReadAt = h.FirstReadAt,
                    LastReadAt = h.LastReadAt,
                    ReadCount = h.ReadCount
                })
                .ToList();
        }

        public bool RemoveFromHistory(int userId, int articleId)
        {
            return historyRepo.Remove(userId, articleId);
        }

        public int ClearHistory(int userId)
        {
            return historyRepo.Clear(userId);
        }
    }
}
