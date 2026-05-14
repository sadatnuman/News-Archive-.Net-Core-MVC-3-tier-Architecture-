using AutoMapper;
using BLL.DTOs;
using DAL.EF.Tables;
using DAL.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public ArticleDTO Get(int id)
        {
            var data = repo.Get(id);
            var res = mapper.Map<ArticleDTO>(data);
            return res;

        }
        public bool Create(ArticleDTO c)
        {
            var converted = mapper.Map<Article>(c);
            return repo.Create(converted);
        }

        public bool Update(ArticleDTO c)
        {
            var converted = mapper.Map<Article>(c);
            return repo.Update(converted);
        }
        public bool Delete(int id)
        {
            return repo.Delete(id);
        }

        public List<ArticleDTO> GetByCategoryId(int categoryId)
        {
            var data = repo.GetByCategoryId(categoryId);
            var res = mapper.Map<List<ArticleDTO>>(data);
            return res;
        }

        public ArticleDTO GetWithDetails(int id)
        {
            var data = repo.GetWithDetails(id);
            var res = mapper.Map<ArticleDTO>(data);
            return res;
        }

        public void IncrementViewCount(int articleId)
        {
            repo.IncrementViewCount(articleId);
        }

        public List<ArticleDTO> Search(string keyword, DateTime? from, DateTime? to, int page, int pageSize, out int total)
        {
            var query = repo.Get().AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(a => a.Title.Contains(keyword) || a.Content.Contains(keyword));
            }
            if (from.HasValue)
                query = query.Where(a => a.PublishDate >= from.Value);
            if (to.HasValue)
                query = query.Where(a => a.PublishDate <= to.Value);
            total = query.Count();
            return query.OrderByDescending(a => a.PublishDate)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .Select(a => mapper.Map<ArticleDTO>(a)).ToList();
        }

        public List<ArticleDTO> GetTrendingArticles(int count)
        {
            var articles = repo.Get();
            var trending = articles
                .Select(a => new {
                    Article = a,
                    TrendingScore = a.ViewsCount + (a.Comments?.Count() ?? 0) * 2
                })
                .OrderByDescending(x => x.TrendingScore)
                .Take(count)
                .Select(x => mapper.Map<ArticleDTO>(x.Article))
                .ToList();
            return trending;
        }

        public List<ArticleDTO> GetRecommendedArticles(int articleId, int count = 4)
        {
            var all = repo.Get();
            var target = all.FirstOrDefault(a => a.ArticleId == articleId);
            if (target == null) return new List<ArticleDTO>();
            // 1. Same Category
            var sameCategory = all.Where(a => a.ArticleId != articleId && a.CategoryId == target.CategoryId).ToList();
            if (sameCategory.Count >= count)
                return sameCategory.Take(count).Select(a => mapper.Map<ArticleDTO>(a)).ToList();
            // 2. Same Tag
            var targetTags = target.Tags.Select(t => t.TagId).ToList();
            var sameTag = all.Where(a => a.ArticleId != articleId && a.Tags.Any(t => targetTags.Contains(t.TagId))).ToList();
            var combined = sameCategory.Concat(sameTag).Distinct().Take(count).ToList();
            if (combined.Count >= count)
                return combined.Take(count).Select(a => mapper.Map<ArticleDTO>(a)).ToList();
            // 3. Fallback: latest popular
            var fallback = all.Where(a => a.ArticleId != articleId)
                .OrderByDescending(a => a.ViewsCount)
                .ThenByDescending(a => a.PublishDate)
                .Take(count - combined.Count)
                .ToList();
            return combined.Concat(fallback).Take(count).Select(a => mapper.Map<ArticleDTO>(a)).ToList();
        }
        public int GetCommentCount(int articleId)
        {
            var article = repo.Get(articleId);
            return article?.Comments?.Count() ?? 0;
        }

        public bool HasBreakingNews()
        {
            return repo.Get().Any(a => a.IsBreakingNews);
        }
    }
}
