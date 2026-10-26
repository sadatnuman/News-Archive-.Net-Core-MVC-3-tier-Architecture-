namespace BLL
{
    public static class MediaLimits
    {
        public const long MaxArticleImageBytes = 5 * 1024 * 1024;
        public const long MaxAvatarBytes = 2 * 1024 * 1024;
        public const long MaxRequestBytes = 12 * 1024 * 1024;
        public const string UploadPrefix = "/uploads/";
        public const string AcceptedTypes = "image/jpeg,image/png,image/webp,image/gif";
    }
}
