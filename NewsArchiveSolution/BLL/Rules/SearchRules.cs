using System.Globalization;
using System.Text;
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
        public const int MaxWordLength = 40;
        public const int MaxQueryTerms = 5;

        [GeneratedRegex(@"\s+")]
        private static partial Regex Spaces();

        public static string? NormalizeQuery(string? query)
        {
            if (string.IsNullOrWhiteSpace(query)) return null;
            var q = Spaces().Replace(query.Trim(), " ");
            if (q.Length > MaxQueryLength) q = q[..MaxQueryLength].TrimEnd();
            return q.Length < MinQueryLength ? null : q;
        }

        private static List<string> Words(string? text, int minLength, int maxWords)
        {
            var words = new List<string>();
            var seen = new HashSet<string>();
            var word = new StringBuilder();
            foreach (var c in (text ?? string.Empty) + " ")
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category == UnicodeCategory.Format) continue;
                if (char.IsLetterOrDigit(c) || category is UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark)
                {
                    if (word.Length < MaxWordLength) word.Append(char.ToLowerInvariant(c));
                    continue;
                }
                if (word.Length >= minLength && seen.Add(word.ToString()))
                {
                    words.Add(word.ToString());
                    if (words.Count == maxWords) break;
                }
                word.Clear();
            }
            return words;
        }

        public static List<string> IndexWords(string? text) => Words(text, 2, int.MaxValue);

        public static List<string> QueryTerms(string? query) => Words(query, 1, MaxQueryTerms);
    }
}
