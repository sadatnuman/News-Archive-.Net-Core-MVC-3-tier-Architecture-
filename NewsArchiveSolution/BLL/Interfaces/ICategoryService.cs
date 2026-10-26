using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface ICategoryService
    {
        List<CategoryDTO> Get();

        CategoryDTO Get(int id);

        bool NameExists(string name, int categoryId);

        bool Create(CategoryDTO c);

        bool Update(CategoryDTO c);

        bool Delete(int id, int? moveArticlesTo);

        int CountArticles(int id);

        void EnsureSlugs();
    }
}
