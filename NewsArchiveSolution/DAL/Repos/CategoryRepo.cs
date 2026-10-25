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
            return db.Categories.ToList();
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

        public Category GetWithArticles(int categoryId)
        {
            return db.Categories
                .Include(c => c.Articles.OrderByDescending(a => a.PublishDate))
                .FirstOrDefault(c => c.CategoryId == categoryId);
        }
    }
}
