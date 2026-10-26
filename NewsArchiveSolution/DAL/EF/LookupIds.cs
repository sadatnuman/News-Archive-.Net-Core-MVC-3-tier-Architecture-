namespace DAL.EF
{
    public static class RoleIds
    {
        public const int Admin = 1;
        public const int Reader = 2;
        public const int Author = 3;
    }

    public static class LanguageCodes
    {
        public const string English = "en";
        public const string Bangla = "bn";
    }

    public static class ArticleStatusIds
    {
        public const int Draft = 1;
        public const int Pending = 2;
        public const int Published = 3;
        public const int Rejected = 4;
        public const int Scheduled = 5;
    }

    public static class ReportReasonIds
    {
        public const int Spam = 1;
        public const int Abuse = 2;
        public const int OffTopic = 3;
        public const int Misinformation = 4;
        public const int Other = 5;
    }
}
