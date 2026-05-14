using DAL.EF;
using DAL.EF.Tables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DAL.Repos
{
    public class ArticleRepo
    {
        NewsArchiveDbContext db;
        public ArticleRepo(NewsArchiveDbContext db)
        {
            this.db = db;
        }
        public bool Create(Article a)
        {
            db.Articles.Add(a);
            return db.SaveChanges() > 0;
        }
        public Article Get(int id)
        {
            return db.Articles.Find(id);
        }
        public List<Article> Get()
        {
            return db.Articles.OrderByDescending(a => a.PublishDate).ToList();
        }
        public bool Update(Article a)
        {
            var exobj = Get(a.ArticleId);
            db.Entry(exobj).CurrentValues.SetValues(a);
            return db.SaveChanges() > 0;
        }
        public bool Delete(int id)
        {
            var exobj = Get(id);
            db.Articles.Remove(exobj);
            return db.SaveChanges() > 0;
        }

        // NEW METHODS FOR FILTERING & RELATIONSHIPS
        public List<Article> GetByCategoryId(int categoryId)
        {
            return db.Articles
                .Where(a => a.CategoryId == categoryId)
                .OrderByDescending(a => a.PublishDate)
                .ToList();
        }

        public Article GetWithDetails(int id)
        {
            return db.Articles
                .Include(a => a.Author)
                .Include(a => a.Category)
                .Include(a => a.Comments)
                .ThenInclude(c => c.User)
                .FirstOrDefault(a => a.ArticleId == id);
        }

        public void IncrementViewCount(int articleId)
        {
            var article = db.Articles.Find(articleId);
            if (article != null)
            {
                article.ViewsCount++;
                db.SaveChanges();
            }
        }
    }
}
