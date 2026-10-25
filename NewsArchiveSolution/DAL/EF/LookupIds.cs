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
    }
}
