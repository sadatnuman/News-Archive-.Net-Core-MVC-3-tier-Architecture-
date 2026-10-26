namespace BLL.DTOs
{
    public class ModerationItemDTO
    {
        public CommentDTO Comment { get; set; } = null!;

        public string CommenterName { get; set; } = string.Empty;

        public int ArticleId { get; set; }

        public string ArticleTitle { get; set; } = string.Empty;

        public string? ArticleSlug { get; set; }

        public string ArticleLanguage { get; set; } = AppLanguages.English;

        public string? ModeratedByName { get; set; }

        public List<CommentReportDTO> Reports { get; set; } = new();
    }

    public class CommentReportDTO
    {
        public int UserId { get; set; }

        public string ReporterName { get; set; } = string.Empty;

        public int ReasonId { get; set; }

        public string ReasonName { get; set; } = string.Empty;

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class ReportReasonDTO
    {
        public int ReasonId { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    public class CommentReportCreateDTO
    {
        public int ReasonId { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(ModerationRules.MaxNoteLength)]
        public string? Note { get; set; }
    }
}
