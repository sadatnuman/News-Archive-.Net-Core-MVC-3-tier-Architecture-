using AutoMapper;
using BLL.DTOs;
using DAL.EF.Tables;
using DAL.Repos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL.Services
{
    public class ArticleService
    {
        ArticleRepo repo;
        Mapper mapper;
        public ArticleService(ArticleRepo repo)
        {
            this.repo = repo;
            mapper = MapperConfig.GetMapper();
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

        public bool Create(ArticleFormDTO form, int authorId, int roleId, bool submitForReview)
        {
            var isAdmin = roleId == AppRoles.Admin;
            if (!isAdmin && roleId != AppRoles.Author) return false;

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
                article.StatusId = ArticleStatus.Published;
                article.ReviewedById = authorId;
                article.ReviewedAt = now;
            }
            else
            {
                article.StatusId = submitForReview ? ArticleStatus.Pending : ArticleStatus.Draft;
            }

            return repo.Create(article);
        }

        public bool Update(int articleId, ArticleFormDTO form, int editorId, int roleId, bool submitForReview)
        {
            var article = repo.Get(articleId);
            if (article == null || !CanEdit(article.AuthorId, article.StatusId, editorId, roleId)) return false;

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
            }
            else
            {
                article.StatusId = submitForReview ? ArticleStatus.Pending : ArticleStatus.Draft;
            }

            return repo.Update(article);
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

        public bool Approve(int articleId, int adminId)
        {
            var article = repo.Get(articleId);
            if (article == null || article.StatusId != ArticleStatus.Pending) return false;

            var now = DateTime.Now;
            article.StatusId = ArticleStatus.Published;
            article.PublishDate = now;
            article.ReviewedById = adminId;
            article.ReviewedAt = now;
            article.ReviewNote = null;
            return repo.Update(article);
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
            if (article == null || !CanEdit(article.AuthorId, article.StatusId, userId, roleId)) return false;
            return repo.Delete(articleId);
        }

        public static bool CanEdit(int authorId, int statusId, int userId, int? roleId)
        {
            if (roleId == AppRoles.Admin) return true;
            return roleId == AppRoles.Author && authorId == userId && statusId != ArticleStatus.Published;
        }

        public static bool CanView(ArticleDTO article, int? userId, int? roleId)
        {
            if (article.IsPublished || roleId == AppRoles.Admin) return true;
            return roleId == AppRoles.Author && userId == article.AuthorId;
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
                ReviewNote = a.ReviewNote
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
