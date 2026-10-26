using DAL.EF;

namespace BLL
{
    public static class ModerationRules
    {
        public const int AutoHideThreshold = 3;
        public const int MaxNoteLength = 300;

        public static readonly int[] ReasonIds =
        {
            ReportReasonIds.Spam, ReportReasonIds.Abuse, ReportReasonIds.OffTopic, ReportReasonIds.Misinformation, ReportReasonIds.Other
        };

        public static bool IsValidReason(int reasonId) => ReasonIds.Contains(reasonId);

        public static string ReasonName(int reasonId) => reasonId switch
        {
            ReportReasonIds.Spam => "Spam",
            ReportReasonIds.Abuse => "Abuse or harassment",
            ReportReasonIds.OffTopic => "Off topic",
            ReportReasonIds.Misinformation => "Misinformation",
            _ => "Other"
        };

        public static string? CleanNote(string? note)
        {
            if (string.IsNullOrWhiteSpace(note)) return null;
            var trimmed = note.Trim();
            return trimmed.Length > MaxNoteLength ? trimmed[..MaxNoteLength] : trimmed;
        }

        public static bool ShouldAutoHide(int reportCount) => reportCount >= AutoHideThreshold;
    }

    public enum ReportResult
    {
        Reported,
        AutoHidden,
        AlreadyReported,
        OwnComment,
        NotFound,
        InvalidReason
    }
}
