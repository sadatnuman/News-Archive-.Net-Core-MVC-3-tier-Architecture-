using System.Text.RegularExpressions;

namespace BLL
{
    public static partial class SearchRules
    {
        public const int MinQueryLength = 2;
        public const int MaxQueryLength = 60;
        public const int MaxStories = 8;
        public const int TopicCount = 3;
        public const int SectionCount = 2;
        public const int AuthorCount = 2;

        [GeneratedRegex(@"\s+")]
        private static partial Regex Spaces();

        public static string? NormalizeQuery(string? query)
        {
            if (string.IsNullOrWhiteSpace(query)) return null;
            var q = Spaces().Replace(query.Trim(), " ");
            if (q.Length > MaxQueryLength) q = q[..MaxQueryLength].TrimEnd();
            return q.Length < MinQueryLength ? null : q;
        }
    }
}
