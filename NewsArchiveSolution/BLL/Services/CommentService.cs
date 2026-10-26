using BLL.Audit;
using BLL.Caching;
using BLL.DTOs;
using BLL.Interfaces;
using DAL.EF.Tables;
using DAL.Interfaces;
using AutoMapper;

namespace BLL.Services
{
    public class CommentService : ICommentService
    {
        public const int MinLength = 2;
        public const int MaxLength = 500;

        private readonly ICommentRepository repo;
        private readonly IArticleRepository articleRepo;
        private readonly IMapper mapper;
        private readonly IAuditService audit;
        private readonly IActorContext actor;
        private readonly IAppCache cache;

        public CommentService(IDataAccessFactory factory, IMapper mapper, IAuditService audit, IActorContext actor, IAppCache cache)
        {
            this.cache = cache;
            repo = factory.GetCommentRepository();
            articleRepo = factory.GetArticleRepository();
            this.mapper = mapper;
            this.audit = audit;
            this.actor = actor;
        }

        private static string Excerpt(string text) => text.Length > 120 ? text[..120] + "…" : text;

        public List<CommentDTO> Get()
        {
            return mapper.Map<List<CommentDTO>>(repo.Get());
        }

        public CommentDTO? Get(int id)
        {
            var data = repo.Get(id);
            return data == null ? null : mapper.Map<CommentDTO>(data);
        }

        public bool Create(CommentDTO c)
        {
            c.Text = c.Text?.Trim() ?? string.Empty;
            if (c.Text.Length < MinLength || c.Text.Length > MaxLength) return false;
            c.IsHidden = false;
            c.ModeratedById = null;
            c.ModeratedAt = null;
            var converted = mapper.Map<Comment>(c);
            if (!repo.Create(converted)) return false;
            c.CommentId = converted.CommentId;
            return true;
        }

        public bool Update(CommentDTO c)
        {
            return repo.Update(mapper.Map<Comment>(c));
        }

        public bool Delete(int id)
        {
            var comment = repo.Get(id);
            if (comment == null || !repo.Delete(id)) return false;
            cache.Invalidate(CacheRegions.Content);

            if (actor.UserId != comment.UserId)
                audit.Record(AuditActions.CommentRemoved, AuditEntities.Comment, id, Excerpt(comment.Text));
            return true;
        }

        public List<CommentDTO> GetByArticleId(int articleId, bool includeHidden = false)
        {
            return mapper.Map<List<CommentDTO>>(repo.GetByArticleId(articleId, includeHidden));
        }

        public List<ReportReasonDTO> GetReasons()
        {
            return repo.GetReasons().Select(r => new ReportReasonDTO { ReasonId = r.ReasonId, Name = r.Name }).ToList();
        }

        public ReportResult Report(int commentId, int userId, int reasonId, string? note)
        {
            if (!ModerationRules.IsValidReason(reasonId)) return ReportResult.InvalidReason;

            var comment = repo.Get(commentId);
            if (comment == null || comment.IsHidden) return ReportResult.NotFound;
            if (comment.UserId == userId) return ReportResult.OwnComment;

            var article = articleRepo.Get(comment.ArticleId);
            if (article == null || article.StatusId != ArticleStatus.Published) return ReportResult.NotFound;

            if (!repo.AddReport(commentId, userId, reasonId, ModerationRules.CleanNote(note), DateTime.Now))
                return ReportResult.AlreadyReported;

            var reports = repo.CountReports(commentId);
            if (ModerationRules.ShouldAutoHide(reports))
            {
                repo.SetHidden(commentId, true, null, DateTime.Now);
                cache.Invalidate(CacheRegions.Content);
                audit.Record(AuditActions.CommentAutoHidden, AuditEntities.Comment, commentId, $"{reports} reports; {Excerpt(comment.Text)}");
                return ReportResult.AutoHidden;
            }
            return ReportResult.Reported;
        }

        public HashSet<int> GetReportedIds(int userId, IEnumerable<int> commentIds)
        {
            return repo.GetReportedIds(userId, commentIds);
        }

        public bool Hide(int commentId, int moderatorId)
        {
            return Moderate(commentId, true, moderatorId, AuditActions.CommentHidden);
        }

        public bool Keep(int commentId, int moderatorId)
        {
            return Moderate(commentId, false, moderatorId, AuditActions.CommentKept);
        }

        private bool Moderate(int commentId, bool hidden, int moderatorId, string action)
        {
            var comment = repo.Get(commentId);
            if (comment == null || !repo.SetHidden(commentId, hidden, moderatorId, DateTime.Now)) return false;

            var cleared = repo.ClearReports(commentId);
            cache.Invalidate(CacheRegions.Content);
            audit.Record(action, AuditEntities.Comment, commentId, $"{cleared} reports cleared; {Excerpt(comment.Text)}", moderatorId);
            return true;
        }

        private ModerationItemDTO ToItem(Comment c)
        {
            return new ModerationItemDTO
            {
                Comment = mapper.Map<CommentDTO>(c),
                CommenterName = c.User?.Name ?? string.Empty,
                ArticleId = c.ArticleId,
                ArticleTitle = c.Article?.Title ?? string.Empty,
                ArticleSlug = c.Article?.Slug,
                ArticleLanguage = c.Article?.LanguageCode ?? AppLanguages.English,
                ModeratedByName = c.ModeratedBy?.Name,
                Reports = c.Reports
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new CommentReportDTO
                    {
                        UserId = r.UserId,
                        ReporterName = r.User?.Name ?? string.Empty,
                        ReasonId = r.ReasonId,
                        ReasonName = r.Reason?.Name ?? ModerationRules.ReasonName(r.ReasonId),
                        Note = r.Note,
                        CreatedAt = r.CreatedAt
                    })
                    .ToList()
            };
        }

        public List<ModerationItemDTO> GetReported(int page, int pageSize, out int total)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 20;
            return repo.GetReported(page, pageSize, out total).Select(ToItem).ToList();
        }

        public List<ModerationItemDTO> GetHidden(int page, int pageSize, out int total)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 20;
            return repo.GetHidden(page, pageSize, out total).Select(ToItem).ToList();
        }

        public int CountReported()
        {
            return repo.CountReported();
        }
    }
}
