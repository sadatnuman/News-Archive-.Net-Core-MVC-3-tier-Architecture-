using DAL.EF;
using DAL.EF.Tables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DAL.Repos
{
    public class CommentRepo
    {
        NewsArchiveDbContext db;
        public CommentRepo(NewsArchiveDbContext db)
        {
            this.db = db;
        }
        public bool Create(Comment c)
        {
            db.Comments.Add(c);
            return db.SaveChanges() > 0;
        }
        public Comment Get(int id)
        {
            return db.Comments.Find(id);
        }
        public List<Comment> Get()
        {
            return db.Comments.ToList();
        }
        public bool Update(Comment c)
        {
            var exobj = Get(c.CommentId);
            db.Entry(exobj).CurrentValues.SetValues(c);
            return db.SaveChanges() > 0;
        }
        public bool Delete(int id)
        {
            var exobj = Get(id);
            db.Comments.Remove(exobj);
            return db.SaveChanges() > 0;
        }

        // NEW METHOD TO GET COMMENTS BY ARTICLE WITH USER INFO
        public List<Comment> GetByArticleId(int articleId)
        {
            return db.Comments
                .Include(c => c.User)
                .Where(c => c.ArticleId == articleId)
                .OrderByDescending(c => c.CreatedDate)
                .ToList();
        }
    }
}
