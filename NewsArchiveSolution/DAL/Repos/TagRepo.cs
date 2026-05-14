using DAL.EF;
using DAL.EF.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Repos
{
    public class TagRepo
    {
        NewsArchiveDbContext db;
        public TagRepo(NewsArchiveDbContext db)
        {
            this.db = db;
        }
        public bool Create(Tag t)
        {
            db.Tags.Add(t);
            return db.SaveChanges() > 0;
        }
        public Tag Get(int id)
        {
            return db.Tags.Find(id);
        }
        public List<Tag> Get()
        {
            return db.Tags.ToList();
        }
        public bool Update(Tag c)
        {
            var exobj = Get(c.TagId);
            db.Entry(exobj).CurrentValues.SetValues(c);
            return db.SaveChanges() > 0;
        }
        public bool Delete(int id)
        {
            var exobj = Get(id);
            db.Tags.Remove(exobj);
            return db.SaveChanges() > 0;
        }
    }
}
