using DAL.EF;
using DAL.EF.Tables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DAL.Repos
{
    public class CategoryRepo
    {
        NewsArchiveDbContext db;
        public CategoryRepo(NewsArchiveDbContext db)
        {
            this.db = db;
        }
        public bool Create(Category c)
        {
            db.Categories.Add(c);
            return db.SaveChanges() > 0;
        }
        public Category Get(int id)
        {
            return db.Categories.Find(id);
        }
        public List<Category> Get()
        {
            return db.Categories.AsNoTracking().OrderBy(c => c.Name).ToList();
        }
        public bool Update(Category c)
        {
            var exobj = Get(c.CategoryId);
            db.Entry(exobj).CurrentValues.SetValues(c);
            return db.SaveChanges() > 0;
        }
        public bool Delete(int id)
        {
            var exobj = Get(id);
            db.Categories.Remove(exobj);
            return db.SaveChanges() > 0;
        }

        public bool HasArticles(int categoryId)
        {
            return db.Articles.Any(a => a.CategoryId == categoryId);
        }

        public bool SlugExists(string slug, int excludeCategoryId)
        {
            return db.Categories.Any(c => c.Slug == slug && c.CategoryId != excludeCategoryId);
        }

        public bool NameExists(string name, int excludeCategoryId)
        {
            return db.Categories.Any(c => (c.Name == name || c.NameBn == name) && c.CategoryId != excludeCategoryId);
        }

        public int CountArticles(int categoryId)
        {
            return db.Articles.Count(a => a.CategoryId == categoryId);
        }

        public bool DeleteMovingArticles(int categoryId, int targetCategoryId)
        {
            if (categoryId == targetCategoryId || !db.Categories.Any(c => c.CategoryId == targetCategoryId))
                return false;

            using var transaction = db.Database.BeginTransaction();
            db.Articles
                .Where(a => a.CategoryId == categoryId)
                .ExecuteUpdate(s => s.SetProperty(a => a.CategoryId, targetCategoryId));
            var deleted = db.Categories.Where(c => c.CategoryId == categoryId).ExecuteDelete() > 0;
            transaction.Commit();
            return deleted;
        }

        public List<Category> GetWithoutSlug()
        {
            return db.Categories.Where(c => c.Slug == null).ToList();
        }

        public void SaveChanges()
        {
            db.SaveChanges();
        }
    }
}
