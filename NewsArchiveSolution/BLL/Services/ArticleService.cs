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

        public List<ArticleDTO> Search(string? keyword, DateTime? from, DateTime? to, int page, int pageSize, out int total)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 9;
            var data = repo.Search(keyword, from, to, page, pageSize, out total);
            return mapper.Map<List<ArticleDTO>>(data);
        }

        public List<ArticleDTO> GetBreakingNews()
        {
            return mapper.Map<List<ArticleDTO>>(repo.GetBreaking());
        }

        public List<ArticleDTO> GetTrendingArticles(int count)
        {
            return mapper.Map<List<ArticleDTO>>(repo.GetTrending(count));
        }

        public List<ArticleDTO> GetRecommendedArticles(int articleId, int count = 4)
        {
            return mapper.Map<List<ArticleDTO>>(repo.GetRecommended(articleId, count));
        }

        public int GetCommentCount(int articleId)
        {
            return repo.GetCommentCount(articleId);
        }

        public bool HasBreakingNews()
        {
            return repo.HasBreakingNews();
        }
    }
}
