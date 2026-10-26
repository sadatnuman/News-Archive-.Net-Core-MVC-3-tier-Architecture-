using BLL.DTOs;

namespace BLL
{
    public static class ArticleRules
    {
        public const int ArchiveMinYear = 1990;
        public const int MaxTagsPerArticle = 8;
        public static readonly TimeSpan MaxScheduleAhead = TimeSpan.FromDays(366);

        public static bool CanEdit(int authorId, int statusId, int userId, int? roleId)
        {
            if (roleId == AppRoles.Admin) return true;
            return roleId == AppRoles.Author && authorId == userId && statusId != ArticleStatus.Published;
        }

        public static bool CanView(ArticleDTO article, int? userId, int? roleId)
        {
            if (article.IsPublished || roleId == AppRoles.Admin) return true;
            return roleId == AppRoles.Author && userId == article.AuthorId;
        }

        public static bool IsFutureSchedule(DateTime? publishAt, DateTime now)
        {
            return publishAt.HasValue && publishAt.Value > now.AddMinutes(1) && publishAt.Value <= now.Add(MaxScheduleAhead);
        }

        public static bool IsValidArchiveDate(int year, int? month = null, int? day = null)
        {
            if (year < ArchiveMinYear || year > DateTime.Now.Year + 1) return false;
            if (month == null) return day == null;
            if (month < 1 || month > 12) return false;
            return day == null || (day >= 1 && day <= DateTime.DaysInMonth(year, month.Value));
        }
    }
}
