using BLL.DTOs;
using BLL.Interfaces;
using DAL;
using DAL.EF.Tables;
using DAL.Interfaces;
using AutoMapper;

namespace BLL.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository repo;
        private readonly IArticleRepository articleRepo;
        private readonly IMapper mapper;

        public CommentService(IDataAccessFactory factory, IMapper mapper)
        {
            repo = factory.GetCommentRepository();
            articleRepo = factory.GetArticleRepository();
            this.mapper = mapper;
        }

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
            if (c.Text.Length < 2 || c.Text.Length > 500) return false;
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
            return repo.Delete(id);
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

            if (ModerationRules.ShouldAutoHide(repo.CountReports(commentId)))
            {
                repo.SetHidden(commentId, true, null, DateTime.Now);
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
            if (!repo.SetHidden(commentId, true, moderatorId, DateTime.Now)) return false;
            repo.ClearReports(commentId);
            return true;
        }

        public bool Keep(int commentId, int moderatorId)
        {
            if (!repo.SetHidden(commentId, false, moderatorId, DateTime.Now)) return false;
            repo.ClearReports(commentId);
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
