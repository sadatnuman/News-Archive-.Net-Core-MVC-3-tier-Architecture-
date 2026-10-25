using DAL.EF;

namespace BLL
{
    public static class AppRoles
    {
        public const int Admin = RoleIds.Admin;
        public const int Reader = RoleIds.Reader;
        public const int Author = RoleIds.Author;

        public static string Name(int? roleId) => roleId switch
        {
            Admin => "Admin",
            Reader => "Reader",
            Author => "Author",
            _ => "Guest"
        };
    }

    public static class ArticleStatus
    {
        public const int Draft = ArticleStatusIds.Draft;
        public const int Pending = ArticleStatusIds.Pending;
        public const int Published = ArticleStatusIds.Published;
        public const int Rejected = ArticleStatusIds.Rejected;

        public static string Name(int statusId) => statusId switch
        {
            Draft => "Draft",
            Pending => "Pending review",
            Published => "Published",
            Rejected => "Rejected",
            _ => "Unknown"
        };
    }
}
