using DAL.EF;
using DAL.EF.Tables;
using System.Collections.Generic;

namespace DAL.Repos
{
    public class RoleRepo
    {
        NewsArchiveDbContext db;

        public RoleRepo(NewsArchiveDbContext db)
        {
            this.db = db;
        }

        public bool Create(Role c)
        {
            db.Roles.Add(c);
            return db.SaveChanges() > 0;
        }

        public Role Get(int id)
        {
            return db.Roles.Find(id);
        }

        public List<Role> Get()
        {
            return db.Roles.ToList();
        }

        public bool Update(Role c)
        {
            var exobj = Get(c.Id);
            db.Entry(exobj).CurrentValues.SetValues(c);
            return db.SaveChanges() > 0;
        }

        public bool Delete(int id)
        {
            var exobj = Get(id);
            db.Roles.Remove(exobj);
            return db.SaveChanges() > 0;
        }
    }
}
