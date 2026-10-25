using AutoMapper;
using BLL.DTOs;
using DAL.EF.Tables;
using DAL;
using DAL.Interfaces;

namespace BLL.Services
{
    public class CategoryService
    {
        private readonly ICategoryRepository repo;
        private readonly IMapper mapper;

        public CategoryService(DataAccessFactory factory, IMapper mapper)
        {
            repo = factory.GetCategoryRepository();
            this.mapper = mapper;
        }

        public List<CategoryDTO> Get()
        {
            var data = repo.Get();
            var res = mapper.Map<List<CategoryDTO>>(data);
            return res;
        }

        public CategoryDTO Get(int id)
        {
            var data = repo.Get(id);
            var res = mapper.Map<CategoryDTO>(data);
            return res;
        }
        private string UniqueSlug(string name, int categoryId)
        {
            var baseSlug = SlugHelper.Generate(name);
            var slug = baseSlug;
            var suffix = 2;
            while (repo.SlugExists(slug, categoryId))
                slug = $"{baseSlug}-{suffix++}";
            return slug;
        }

        public bool NameExists(string name, int categoryId)
        {
            return repo.NameExists(name.Trim(), categoryId);
        }

        public bool Create(CategoryDTO c)
        {
            c.Name = c.Name.Trim();
            c.Slug = UniqueSlug(c.Name, 0);
            var converted = mapper.Map<Category>(c);
            if (!repo.Create(converted)) return false;
            c.CategoryId = converted.CategoryId;
            return true;
        }

        public bool Update(CategoryDTO c)
        {
            c.Name = c.Name.Trim();
            c.Slug = UniqueSlug(c.Name, c.CategoryId);
            var converted = mapper.Map<Category>(c);
            return repo.Update(converted);
        }

        public bool Delete(int id, int? moveArticlesTo)
        {
            if (!repo.HasArticles(id)) return repo.Delete(id);
            return moveArticlesTo.HasValue && repo.DeleteMovingArticles(id, moveArticlesTo.Value);
        }

        public int CountArticles(int id)
        {
            return repo.CountArticles(id);
        }

        public void EnsureSlugs()
        {
            var missing = repo.GetWithoutSlug();
            if (missing.Count == 0) return;
            foreach (var c in missing)
                c.Slug = SlugHelper.Generate(c.Name);
            repo.SaveChanges();
        }
    }
}
