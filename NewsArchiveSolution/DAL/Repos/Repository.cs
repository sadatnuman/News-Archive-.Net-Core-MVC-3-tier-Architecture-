using DAL.EF;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repos
{
    internal class Repository<T> : IRepository<T> where T : class
    {
        protected readonly NewsArchiveDbContext db;
        protected readonly DbSet<T> set;

        public Repository(NewsArchiveDbContext db)
        {
            this.db = db;
            set = db.Set<T>();
        }

        public virtual T? Get(int id)
        {
            return set.Find(id);
        }

        public virtual List<T> Get()
        {
            return set.AsNoTracking().ToList();
        }

        public virtual bool Create(T entity)
        {
            set.Add(entity);
            return db.SaveChanges() > 0;
        }

        public virtual bool Update(T entity)
        {
            var key = db.Model.FindEntityType(typeof(T))!.FindPrimaryKey()!;
            var keyValues = key.Properties.Select(p => p.PropertyInfo!.GetValue(entity)).ToArray();
            var existing = set.Find(keyValues);
            if (existing == null) return false;

            db.Entry(existing).CurrentValues.SetValues(entity);
            db.SaveChanges();
            return true;
        }

        public virtual bool Delete(int id)
        {
            var existing = set.Find(id);
            if (existing == null) return false;

            set.Remove(existing);
            return db.SaveChanges() > 0;
        }

        public virtual int Count()
        {
            return set.Count();
        }

        public void SaveChanges()
        {
            db.SaveChanges();
        }
    }
}
