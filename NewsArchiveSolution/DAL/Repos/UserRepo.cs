using DAL.EF;
using DAL.EF.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Repos
{
    public class UserRepo
    {
        NewsArchiveDbContext db;
        public UserRepo(NewsArchiveDbContext db)
        {
            this.db = db;
        }
        public bool Create(User c)
        {
            db.Users.Add(c);
            return db.SaveChanges() > 0;
        }
        public User Get(int id)
        {
            return db.Users.Find(id);
        }
        public List<User> Get()
        {
            return db.Users.ToList();
        }
        public bool Update(User c)
        {
            var exobj = Get(c.UserId);
            db.Entry(exobj).CurrentValues.SetValues(c);
            return db.SaveChanges() > 0;
        }
        public bool Delete(int id)
        {
            var exobj = Get(id);
            db.Users.Remove(exobj);
            return db.SaveChanges() > 0;
        }
    }
}
