namespace DAL.Interfaces
{
    public interface IRepository<T> where T : class
    {
        T? Get(int id);

        List<T> Get();

        bool Create(T entity);

        bool Update(T entity);

        bool Delete(int id);

        int Count();

        void SaveChanges();
    }
}
