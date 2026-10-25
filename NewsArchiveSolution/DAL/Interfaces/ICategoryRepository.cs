using DAL.EF.Tables;

namespace DAL.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        bool HasArticles(int categoryId);

        bool SlugExists(string slug, int excludeCategoryId);

        bool NameExists(string name, int excludeCategoryId);

        int CountArticles(int categoryId);

        bool DeleteMovingArticles(int categoryId, int targetCategoryId);

        List<Category> GetWithoutSlug();
    }
}
