using DAL.EF;
using DAL.EF.Tables;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repos
{
    internal sealed class TagRepo : Repository<Tag>, ITagRepository
    {
        public TagRepo(NewsArchiveDbContext db) : base(db)
        {
        }

        private IQueryable<Article> Published(string? language)
        {
            var query = db.Articles.AsNoTracking().Where(a => a.StatusId == ArticleStatusIds.Published);
            return string.IsNullOrEmpty(language) ? query : query.Where(a => a.LanguageCode == language);
        }

        public override List<Tag> Get()
        {
            return db.Tags.AsNoTracking().OrderBy(t => t.Name).ToList();
        }

        public Dictionary<int, int> CountPublished(string? language = null)
        {
            return Published(language)
                .SelectMany(a => a.Tags)
                .GroupBy(t => t.TagId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionary(x => x.Key, x => x.Count);
        }

        public List<Tag> GetPopular(int count, string? language = null)
        {
            var published = Published(language);
            return db.Tags.AsNoTracking()
                .Select(t => new { Tag = t, Count = published.Count(a => a.Tags.Any(x => x.TagId == t.TagId)) })
                .Where(x => x.Count > 0)
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.Tag.Name)
                .Take(count)
                .Select(x => x.Tag)
                .ToList();
        }

        public List<Tag> GetForArticle(int articleId)
        {
            return db.Articles.AsNoTracking()
                .Where(a => a.ArticleId == articleId)
                .SelectMany(a => a.Tags)
                .OrderBy(t => t.Name)
                .ToList();
        }

        public bool SetArticleTags(int articleId, IEnumerable<int> tagIds)
        {
            var article = db.Articles.Include(a => a.Tags).FirstOrDefault(a => a.ArticleId == articleId);
            if (article == null) return false;

            var ids = tagIds.Distinct().ToList();
            var tags = db.Tags.Where(t => ids.Contains(t.TagId)).ToList();

            foreach (var removed in article.Tags.Where(t => !ids.Contains(t.TagId)).ToList())
                article.Tags.Remove(removed);
            foreach (var added in tags.Where(t => article.Tags.All(x => x.TagId != t.TagId)))
                article.Tags.Add(added);

            db.SaveChanges();
            return true;
        }

        public List<Article> GetArticles(int tagId, string? language, int page, int pageSize, out int total)
        {
            var query = Published(language).Where(a => a.Tags.Any(t => t.TagId == tagId));
            total = query.Count();
            return query
                .OrderByDescending(a => a.PublishDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public bool NameExists(string name, int excludeTagId)
        {
            return db.Tags.Any(t => (t.Name == name || t.NameBn == name) && t.TagId != excludeTagId);
        }

        public bool SlugExists(string slug, int excludeTagId)
        {
            return db.Tags.Any(t => t.Slug == slug && t.TagId != excludeTagId);
        }

        public List<Tag> GetWithoutSlug()
        {
            return db.Tags.Where(t => t.Slug == null).ToList();
        }

        public List<Tag> Suggest(string term, int count)
        {
            return db.Tags.AsNoTracking()
                .Where(t => t.Name.Contains(term) || (t.NameBn != null && t.NameBn.Contains(term)))
                .OrderByDescending(t => t.Name.StartsWith(term) || (t.NameBn != null && t.NameBn.StartsWith(term)))
                .ThenBy(t => t.Name)
                .Take(count)
                .ToList();
        }
    }
}
