using BLL.DTOs;
using DAL.Repos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL.Services
{
    public class SearchService
    {
        private readonly ArticleRepo repo;
        public SearchService(ArticleRepo repo)
        {
            this.repo = repo;
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
                        .Select(a => new ArticleDTO
                        {
                            ArticleId = a.ArticleId,
                            Title = a.Title,
                            Summary = a.Summary,
                            Content = a.Content,
                            PublishDate = a.PublishDate,
                            ViewsCount = a.ViewsCount,
                            CategoryId = a.CategoryId,
                            AuthorId = a.AuthorId,
                            ImagePath = a.ImagePath
                        }).ToList();
        }
    }
}
