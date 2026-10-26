using BLL.Audit;
using BLL.DTOs;
using BLL.Interfaces;
using DAL.EF.Tables;
using DAL.Interfaces;
using AutoMapper;

namespace BLL.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository repo;
        private readonly IMapper mapper;
        private readonly IAuditService audit;

        public TagService(IDataAccessFactory factory, IMapper mapper, IAuditService audit)
        {
            repo = factory.GetTagRepository();
            this.mapper = mapper;
            this.audit = audit;
        }

        public List<TagDTO> Get()
        {
            return mapper.Map<List<TagDTO>>(repo.Get());
        }

        public List<TagDTO> GetWithCounts(string? language = null)
        {
            var counts = repo.CountPublished(language);
            var tags = Get();
            foreach (var tag in tags)
                tag.ArticleCount = counts.TryGetValue(tag.TagId, out var count) ? count : 0;
            return tags;
        }

        public TagDTO? Get(int id)
        {
            var data = repo.Get(id);
            return data == null ? null : mapper.Map<TagDTO>(data);
        }

        public List<TagDTO> GetPopular(int count, string? language = null)
        {
            return mapper.Map<List<TagDTO>>(repo.GetPopular(count, language));
        }

        public List<TagDTO> GetForArticle(int articleId)
        {
            return mapper.Map<List<TagDTO>>(repo.GetForArticle(articleId));
        }

        public List<ArticleDTO> GetArticles(int tagId, string? language, int page, int pageSize, out int total)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 12;
            return mapper.Map<List<ArticleDTO>>(repo.GetArticles(tagId, language, page, pageSize, out total));
        }

        public bool NameExists(string? name, int tagId)
        {
            return !string.IsNullOrWhiteSpace(name) && repo.NameExists(name.Trim(), tagId);
        }

        public bool Create(TagDTO tag)
        {
            var entity = new Tag
            {
                Name = tag.Name.Trim(),
                NameBn = string.IsNullOrWhiteSpace(tag.NameBn) ? null : tag.NameBn.Trim(),
                Slug = UniqueSlug(tag.Name, 0)
            };
            if (!repo.Create(entity)) return false;

            tag.TagId = entity.TagId;
            tag.Slug = entity.Slug;
            audit.Record(AuditActions.TagCreated, AuditEntities.Tag, entity.TagId, entity.Name);
            return true;
        }

        public bool Update(TagDTO tag)
        {
            var entity = repo.Get(tag.TagId);
            if (entity == null) return false;

            entity.Name = tag.Name.Trim();
            entity.NameBn = string.IsNullOrWhiteSpace(tag.NameBn) ? null : tag.NameBn.Trim();
            entity.Slug = UniqueSlug(entity.Name, entity.TagId);
            if (!repo.Update(entity)) return false;

            audit.Record(AuditActions.TagUpdated, AuditEntities.Tag, entity.TagId, entity.Name);
            return true;
        }

        public bool Delete(int id)
        {
            var name = repo.Get(id)?.Name;
            if (name == null || !repo.Delete(id)) return false;

            audit.Record(AuditActions.TagDeleted, AuditEntities.Tag, id, name);
            return true;
        }

        public bool SetArticleTags(int articleId, IEnumerable<int>? tagIds)
        {
            return repo.SetArticleTags(articleId, (tagIds ?? Enumerable.Empty<int>()).Distinct().Take(ArticleRules.MaxTagsPerArticle));
        }

        public void EnsureSlugs()
        {
            var missing = repo.GetWithoutSlug();
            if (missing.Count == 0) return;
            foreach (var tag in missing)
                tag.Slug = UniqueSlug(tag.Name, tag.TagId);
            repo.SaveChanges();
        }

        private string UniqueSlug(string name, int tagId)
        {
            var baseSlug = SlugHelper.Generate(name);
            var slug = baseSlug;
            var suffix = 2;
            while (repo.SlugExists(slug, tagId))
                slug = $"{baseSlug}-{suffix++}";
            return slug;
        }
    }
}
