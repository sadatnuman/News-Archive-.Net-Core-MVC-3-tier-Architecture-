using BLL.Audit;
using BLL.DTOs;
using BLL.Interfaces;
using DAL.EF.Tables;
using DAL.Interfaces;
using AutoMapper;

namespace BLL.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository repo;
        private readonly IMapper mapper;
        private readonly IAuditService audit;

        public CategoryService(IDataAccessFactory factory, IMapper mapper, IAuditService audit)
        {
            repo = factory.GetCategoryRepository();
            this.mapper = mapper;
            this.audit = audit;
        }

        public List<CategoryDTO> Get()
        {
            return mapper.Map<List<CategoryDTO>>(repo.Get());
        }

        public CategoryDTO? Get(int id)
        {
            var data = repo.Get(id);
            return data == null ? null : mapper.Map<CategoryDTO>(data);
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

        private static string? Clean(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        public bool NameExists(string name, int categoryId)
        {
            return !string.IsNullOrWhiteSpace(name) && repo.NameExists(name.Trim(), categoryId);
        }

        public bool Create(CategoryDTO c)
        {
            c.Name = c.Name.Trim();
            c.NameBn = Clean(c.NameBn);
            c.Description = Clean(c.Description);
            c.DescriptionBn = Clean(c.DescriptionBn);
            c.Slug = UniqueSlug(c.Name, 0);
            var converted = mapper.Map<Category>(c);
            if (!repo.Create(converted)) return false;

            c.CategoryId = converted.CategoryId;
            audit.Record(AuditActions.SectionCreated, AuditEntities.Section, c.CategoryId, c.Name);
            return true;
        }

        public bool Update(CategoryDTO c)
        {
            c.Name = c.Name.Trim();
            c.NameBn = Clean(c.NameBn);
            c.Description = Clean(c.Description);
            c.DescriptionBn = Clean(c.DescriptionBn);
            c.Slug = UniqueSlug(c.Name, c.CategoryId);
            if (!repo.Update(mapper.Map<Category>(c))) return false;

            audit.Record(AuditActions.SectionUpdated, AuditEntities.Section, c.CategoryId, c.Name);
            return true;
        }

        public bool Delete(int id, int? moveArticlesTo)
        {
            var name = repo.Get(id)?.Name;
            if (name == null) return false;

            bool deleted;
            var details = name;
            if (!repo.HasArticles(id))
            {
                deleted = repo.Delete(id);
            }
            else
            {
                if (moveArticlesTo == null || moveArticlesTo == id || repo.Get(moveArticlesTo.Value) == null) return false;
                deleted = repo.DeleteMovingArticles(id, moveArticlesTo.Value);
                details = $"{name}; articles moved to section {moveArticlesTo}";
            }

            if (deleted)
                audit.Record(AuditActions.SectionDeleted, AuditEntities.Section, id, details);
            return deleted;
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
                c.Slug = UniqueSlug(c.Name, c.CategoryId);
            repo.SaveChanges();
        }
    }
}
