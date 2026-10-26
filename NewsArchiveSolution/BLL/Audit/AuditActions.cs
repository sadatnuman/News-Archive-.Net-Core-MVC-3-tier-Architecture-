namespace BLL.Audit
{
    public static class AuditEntities
    {
        public const string Article = "Article";
        public const string Comment = "Comment";
        public const string User = "User";
        public const string Section = "Section";
        public const string Tag = "Tag";
        public const string Security = "Security";
        public const string Newsletter = "Newsletter";

        public static readonly string[] All = { Article, Comment, User, Section, Tag, Newsletter, Security };
    }

    public static class AuditActions
    {
        public const string ArticlePublished = "ArticlePublished";
        public const string ArticleApproved = "ArticleApproved";
        public const string ArticleScheduled = "ArticleScheduled";
        public const string ArticleRejected = "ArticleRejected";
        public const string ArticlePublishedEarly = "ArticlePublishedEarly";
        public const string ArticleUnscheduled = "ArticleUnscheduled";
        public const string ArticlePublishedOnSchedule = "ArticlePublishedOnSchedule";
        public const string ArticleEdited = "ArticleEdited";
        public const string ArticleDeleted = "ArticleDeleted";

        public const string CommentHidden = "CommentHidden";
        public const string CommentAutoHidden = "CommentAutoHidden";
        public const string CommentKept = "CommentKept";
        public const string CommentRemoved = "CommentRemoved";

        public const string UserCreated = "UserCreated";
        public const string UserUpdated = "UserUpdated";
        public const string UserRoleChanged = "UserRoleChanged";
        public const string UserDeleted = "UserDeleted";

        public const string SectionCreated = "SectionCreated";
        public const string SectionUpdated = "SectionUpdated";
        public const string SectionDeleted = "SectionDeleted";

        public const string TagCreated = "TagCreated";
        public const string TagUpdated = "TagUpdated";
        public const string TagDeleted = "TagDeleted";

        public const string SignedIn = "SignedIn";
        public const string SignInFailed = "SignInFailed";
        public const string PasswordChanged = "PasswordChanged";
        public const string AllSessionsRevoked = "AllSessionsRevoked";
        public const string AccountRegistered = "AccountRegistered";
        public const string PasswordResetRequested = "PasswordResetRequested";
        public const string PasswordChangeRejected = "PasswordChangeRejected";

        public const string NewsletterSent = "NewsletterSent";
        public const string SubscriberRemoved = "SubscriberRemoved";
    }

    public static class AuditRules
    {
        public const int RetentionDays = 365;
        public const int MaxDetailsLength = 500;

        public static string? Clip(string? details)
        {
            if (string.IsNullOrWhiteSpace(details)) return null;
            var trimmed = details.Trim();
            return trimmed.Length > MaxDetailsLength ? trimmed[..MaxDetailsLength] : trimmed;
        }

        public static string MaskEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return "(empty)";
            var e = email.Trim().ToLowerInvariant();
            if (e.Length > 150) e = e[..150];
            var at = e.IndexOf('@');
            if (at <= 1) return "***" + (at >= 0 ? e[at..] : string.Empty);
            return e[0] + new string('*', Math.Min(at - 1, 6)) + e[at..];
        }
    }
}
