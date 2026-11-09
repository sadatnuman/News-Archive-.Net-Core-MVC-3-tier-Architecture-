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

        public static string Version(int statusId, DateTime? updatedAt, DateTime createdAt)
        {
            return string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{statusId}-{(updatedAt ?? createdAt).Ticks}");
        }

        public static bool IsWellFormedVersion(string? version)
        {
            var parts = version?.Split('-');
            return parts is { Length: 2 }
                && int.TryParse(parts[0], System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out _)
                && long.TryParse(parts[1], System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out _);
        }

        public static bool IsFutureSchedule(DateTime? publishAt, DateTime now)
        {
            return publishAt.HasValue && publishAt.Value > now.AddMinutes(1) && publishAt.Value <= now.Add(MaxScheduleAhead);
        }

        public static bool IsValidArchiveDate(int year, int? month = null, int? day = null)
        {
            if (year < ArchiveMinYear || year > SiteClock.Now.Year + 1) return false;
            if (month == null) return day == null;
            if (month < 1 || month > 12) return false;
            return day == null || (day >= 1 && day <= DateTime.DaysInMonth(year, month.Value));
        }
    }
}
