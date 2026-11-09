namespace BLL
{
    public static class SiteClock
    {
        private static TimeZoneInfo zone = TimeZoneInfo.Local;

        public static TimeZoneInfo Zone => zone;

        public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zone);

        public static DateTime ToUtc(DateTime siteTime) =>
            TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(siteTime, DateTimeKind.Unspecified), zone);

        public static void Use(string? timeZoneId)
        {
            if (!string.IsNullOrWhiteSpace(timeZoneId))
                zone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
    }
}
