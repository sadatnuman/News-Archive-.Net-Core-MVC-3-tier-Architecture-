using BLL.DTOs;
using BLL.Interfaces;
using DAL;
using DAL.EF.Tables;
using DAL.Interfaces;
using AutoMapper;

namespace BLL.Services
{
    public class ArticleService : IArticleService
    {
        private readonly IArticleRepository repo;
        private readonly ITagRepository tagRepo;
        private readonly IMapper mapper;

        public ArticleService(IDataAccessFactory factory, IMapper mapper)
        {
            repo = factory.GetArticleRepository();
            tagRepo = factory.GetTagRepository();
            this.mapper = mapper;
        }

        private void SaveTags(int articleId, IEnumerable<int>? tagIds)
        {
            tagRepo.SetArticleTags(articleId, (tagIds ?? Enumerable.Empty<int>()).Distinct().Take(ArticleRules.MaxTagsPerArticle));
        }

        public List<ArticleDTO> Get()
        {
            var data = repo.Get();
            var res = mapper.Map<List<ArticleDTO>>(data);
            return res;
        }

        public ArticleDTO? Get(int id)
        {
            var data = repo.Get(id);
            return data == null ? null : mapper.Map<ArticleDTO>(data);
        }

        public int Create(ArticleFormDTO form, int authorId, int roleId, bool submitForReview)
        {
            var isAdmin = roleId == AppRoles.Admin;
            if (!isAdmin && roleId != AppRoles.Author) return 0;

            var now = DateTime.Now;
            var article = new Article
            {
                Title = form.Title.Trim(),
                Slug = SlugHelper.Generate(form.Title),
                Summary = form.Summary.Trim(),
                Content = form.Content,
                ImagePath = string.IsNullOrWhiteSpace(form.ImagePath) ? null : form.ImagePath.Trim(),
                CategoryId = form.CategoryId,
                IsBreakingNews = isAdmin && form.IsBreakingNews,
                LanguageCode = AppLanguages.Normalize(form.LanguageCode),
                AuthorId = authorId,
                CreatedAt = now,
                PublishDate = now,
                ViewsCount = 0
            };

            if (isAdmin)
            {
                PublishOrSchedule(article, form.PublishAt, now);
                article.ReviewedById = authorId;
                article.ReviewedAt = now;
            }
            else
            {
                article.StatusId = submitForReview ? ArticleStatus.Pending : ArticleStatus.Draft;
            }

            if (!repo.Create(article)) return 0;
            SaveTags(article.ArticleId, form.TagIds);
            return article.ArticleId;
        }

        public bool Update(int articleId, ArticleFormDTO form, int editorId, int roleId, bool submitForReview)
        {
            var article = repo.Get(articleId);
            if (article == null || !ArticleRules.CanEdit(article.AuthorId, article.StatusId, editorId, roleId)) return false;

            var isAdmin = roleId == AppRoles.Admin;
            article.Title = form.Title.Trim();
            article.Slug = SlugHelper.Generate(form.Title);
            article.Summary = form.Summary.Trim();
            article.Content = form.Content;
            article.ImagePath = string.IsNullOrWhiteSpace(form.ImagePath) ? null : form.ImagePath.Trim();
            article.CategoryId = form.CategoryId;
            article.LanguageCode = AppLanguages.Normalize(form.LanguageCode);
            article.UpdatedAt = DateTime.Now;

            if (isAdmin)
            {
                article.IsBreakingNews = form.IsBreakingNews;
                if (article.StatusId != ArticleStatus.Published && ArticleRules.IsFutureSchedule(form.PublishAt, DateTime.Now))
                    PublishOrSchedule(article, form.PublishAt, DateTime.Now);
                else if (article.StatusId == ArticleStatus.Scheduled && form.PublishAt == null)
                    PublishOrSchedule(article, null, DateTime.Now);
            }
            else
            {
                article.StatusId = submitForReview ? ArticleStatus.Pending : ArticleStatus.Draft;
            }

            if (!repo.Update(article)) return false;
            SaveTags(articleId, form.TagIds);
            return true;
        }

        public bool Submit(int articleId, int authorId)
        {
            var article = repo.Get(articleId);
            if (article == null || article.AuthorId != authorId) return false;
            if (article.StatusId != ArticleStatus.Draft && article.StatusId != ArticleStatus.Rejected) return false;

            article.StatusId = ArticleStatus.Pending;
            article.UpdatedAt = DateTime.Now;
            return repo.Update(article);
        }

        private static void PublishOrSchedule(Article article, DateTime? publishAt, DateTime now)
        {
            if (ArticleRules.IsFutureSchedule(publishAt, now))
            {
                article.StatusId = ArticleStatus.Scheduled;
                article.ScheduledAt = publishAt;
                article.PublishDate = publishAt!.Value;
            }
            else
            {
                article.StatusId = ArticleStatus.Published;
                article.ScheduledAt = null;
                article.PublishDate = now;
            }
        }

        public bool Approve(int articleId, int adminId, DateTime? publishAt = null)
        {
            var article = repo.Get(articleId);
            if (article == null || article.StatusId != ArticleStatus.Pending) return false;

            var now = DateTime.Now;
            PublishOrSchedule(article, publishAt, now);
            article.ReviewedById = adminId;
            article.ReviewedAt = now;
            article.ReviewNote = null;
            return repo.Update(article);
        }

        public bool PublishNow(int articleId)
        {
            var article = repo.Get(articleId);
            if (article == null || article.StatusId != ArticleStatus.Scheduled) return false;
            PublishOrSchedule(article, null, DateTime.Now);
            return repo.Update(article);
        }

        public bool Unschedule(int articleId)
        {
            var article = repo.Get(articleId);
            if (article == null || article.StatusId != ArticleStatus.Scheduled) return false;
            article.StatusId = ArticleStatus.Pending;
            article.ScheduledAt = null;
            return repo.Update(article);
        }

        public int PublishDue()
        {
            return repo.PublishDue(DateTime.Now);
        }

        public List<ArticleDTO> GetScheduled()
        {
            return mapper.Map<List<ArticleDTO>>(repo.GetByStatus(ArticleStatus.Scheduled).OrderBy(a => a.ScheduledAt));
        }

        public bool Reject(int articleId, int adminId, string? note)
        {
            var article = repo.Get(articleId);
            if (article == null || article.StatusId != ArticleStatus.Pending) return false;

            article.StatusId = ArticleStatus.Rejected;
            article.ReviewedById = adminId;
            article.ReviewedAt = DateTime.Now;
            article.ReviewNote = string.IsNullOrWhiteSpace(note) ? "No reason given." : note.Trim();
            return repo.Update(article);
        }

        public bool Delete(int articleId, int userId, int roleId)
        {
            var article = repo.Get(articleId);
            if (article == null || !ArticleRules.CanEdit(article.AuthorId, article.StatusId, userId, roleId)) return false;
            return repo.Delete(articleId);
        }

        public ArticleFormDTO? GetForm(int id)
        {
            var a = repo.Get(id);
            if (a == null) return null;
            return new ArticleFormDTO
            {
                ArticleId = a.ArticleId,
                Title = a.Title,
                Summary = a.Summary,
                Content = a.Content,
                ImagePath = a.ImagePath,
                CategoryId = a.CategoryId,
                IsBreakingNews = a.IsBreakingNews,
                LanguageCode = a.LanguageCode,
                StatusId = a.StatusId,
                PublishAt = a.ScheduledAt,
                ReviewNote = a.ReviewNote,
                TagIds = tagRepo.GetForArticle(a.ArticleId).Select(t => t.TagId).ToList()
            };
        }

        public List<ArticleDTO> GetByAuthor(int authorId)
        {
            return mapper.Map<List<ArticleDTO>>(repo.GetByAuthor(authorId));
        }

        public List<ArticleDTO> GetPending()
        {
            return mapper.Map<List<ArticleDTO>>(repo.GetByStatus(ArticleStatus.Pending));
        }

        public int CountPending()
        {
            return repo.CountByStatus(ArticleStatus.Pending);
        }

        public List<ArticleDTO> GetByCategoryId(int categoryId, string? language = null)
        {
            var data = repo.GetByCategoryId(categoryId, language);
            var res = mapper.Map<List<ArticleDTO>>(data);
            return res;
        }

        public ArticleDTO? GetWithDetails(int id)
        {
            var data = repo.GetWithDetails(id);
            return data == null ? null : mapper.Map<ArticleDTO>(data);
        }

        public void IncrementViewCount(int articleId)
        {
            repo.IncrementViewCount(articleId);
        }

        public List<ArticleDTO> Search(string? keyword, DateTime? from, DateTime? to, int page, int pageSize, out int total, string? language = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 9;
            var data = repo.Search(keyword, from, to, page, pageSize, out total, language);
            return mapper.Map<List<ArticleDTO>>(data);
        }

        public List<ArticleDTO> GetArchive(int year, int? month, int? day, string? language, int page, int pageSize, out int total)
        {
            total = 0;
            if (!ArticleRules.IsValidArchiveDate(year, month, day)) return new List<ArticleDTO>();
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 12;

            var from = new DateTime(year, month ?? 1, day ?? 1);
            var to = day.HasValue ? from.AddDays(1) : month.HasValue ? from.AddMonths(1) : from.AddYears(1);
            return mapper.Map<List<ArticleDTO>>(repo.GetByPeriod(from, to, language, page, pageSize, out total));
        }

        public List<ArchiveMonthDTO> GetArchiveMonths(string? language = null)
        {
            return repo.CountByMonth(language)
                .Select(m => new ArchiveMonthDTO { Year = m.Year, Month = m.Month, Count = m.Count })
                .ToList();
        }

        public Dictionary<int, int> GetArchiveDayCounts(int year, int month, string? language = null)
        {
            return ArticleRules.IsValidArchiveDate(year, month) ? repo.CountByDay(year, month, language) : new Dictionary<int, int>();
        }

        public List<ArticleDTO> GetOnThisDay(DateTime today, int count, string? language = null)
        {
            return mapper.Map<List<ArticleDTO>>(repo.GetOnThisDay(today.Month, today.Day, today.Year, count, language));
        }

        public List<ArticleDTO> GetBreakingNews(string? language = null)
        {
            return mapper.Map<List<ArticleDTO>>(repo.GetBreaking(language));
        }

        public List<ArticleDTO> GetTrendingArticles(int count, string? language = null)
        {
            return mapper.Map<List<ArticleDTO>>(repo.GetTrending(count, language));
        }

        public List<ArticleDTO> GetLatest(int count, string? language = null)
        {
            return mapper.Map<List<ArticleDTO>>(repo.GetLatest(count, language));
        }

        public List<ArticleDTO> GetRecommendedArticles(int articleId, int count = 4)
        {
            return mapper.Map<List<ArticleDTO>>(repo.GetRecommended(articleId, count));
        }

        public List<ArticleDTO> GetPublishedForSitemap()
        {
            return mapper.Map<List<ArticleDTO>>(repo.GetPublishedForSitemap());
        }

        public int GetCommentCount(int articleId)
        {
            return repo.GetCommentCount(articleId);
        }

        public bool HasBreakingNews(string? language = null)
        {
            return repo.HasBreakingNews(language);
        }

        public void EnsureSlugs()
        {
            var missing = repo.GetWithoutSlug();
            if (missing.Count == 0) return;
            foreach (var a in missing)
                a.Slug = SlugHelper.Generate(a.Title);
            repo.SaveChanges();
        }
    }
}
