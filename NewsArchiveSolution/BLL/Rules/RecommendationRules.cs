using System.Text;
using System.Text.RegularExpressions;

namespace BLL
{
    public static partial class RecommendationRules
    {
        public const int MaxCount = 20;
        public const int HistorySize = 200;
        public const int ProfileSize = 20;
        public const int SessionHistorySize = 20;
        public const int RelatedPool = 60;
        public const int PopularPool = 12;

        public const double TagWeight = 3.0;
        public const double CategoryWeight = 2.0;
        public const double AuthorWeight = 1.0;
        public const double TextWeight = 6.0;
        public const double RecencyWeight = 1.5;
        public const double PopularityWeight = 0.6;
        public const double BreakingBonus = 0.3;
        public const double AlreadyReadFactor = 0.25;
        public const double RecencyDecayDays = 30;
        public const double ProfileDecay = 0.15;

        private static readonly HashSet<string> StopWords = new[]
        {
            "the", "and", "for", "with", "from", "that", "this", "these", "those", "into", "about", "after", "before", "over",
            "under", "than", "then", "has", "have", "had", "will", "would", "can", "could", "new", "more", "most", "says", "said",
            "who", "what", "when", "where", "how", "why", "not", "but", "also", "their", "there", "they", "its", "are", "was",
            "were", "been", "being", "our", "your", "out", "off", "per", "via", "amid", "while", "which", "all", "any", "each",
            "other", "some", "such", "only", "own", "same", "very", "just", "now", "get", "gets", "make", "makes", "may", "one",
            "two", "three", "first", "year", "years", "day", "days", "people", "according",
            "এবং", "এর", "এই", "করে", "করা", "হয়", "হয়েছে", "থেকে", "জন্য", "নিয়ে", "একটি", "তার", "সঙ্গে", "আর", "হবে",
            "করেছেন", "বলেন", "দিয়ে", "মধ্যে", "কাছে", "এমন", "যা", "যে", "করতে", "পারে", "পারছেন", "হচ্ছে", "ছিল", "তারা",
            "তাদের", "আমাদের", "কয়েকটি", "নতুন", "আরও", "বলছেন", "বলেছেন"
        }.Select(w => w.Normalize(NormalizationForm.FormC)).ToHashSet();

        [GeneratedRegex(@"[\p{L}\p{M}\p{Nd}]+")]
        private static partial Regex WordPattern();

        public static HashSet<string> Keywords(string? title, string? summary)
        {
            var text = ((title ?? string.Empty) + " " + (title ?? string.Empty) + " " + (summary ?? string.Empty))
                .Normalize(NormalizationForm.FormC)
                .ToLowerInvariant();
            var words = new HashSet<string>();
            foreach (Match m in WordPattern().Matches(text))
            {
                var word = m.Value;
                if (word.Length < 3 || StopWords.Contains(word) || word.All(char.IsDigit)) continue;
                if (word.Length > 4 && word[^1] == 's' && word[^2] != 's' && word.All(c => c < 128))
                    word = word[..^1];
                words.Add(word);
            }
            return words;
        }

        public static double Similarity(IReadOnlySet<string> a, IReadOnlySet<string> b)
        {
            if (a.Count == 0 || b.Count == 0) return 0;
            var shared = a.Count(b.Contains);
            return shared == 0 ? 0 : shared / (double)(a.Count + b.Count - shared);
        }

        public static double Recency(DateTime published, DateTime now)
        {
            return Math.Exp(-Math.Max(0, (now - published).TotalDays) / RecencyDecayDays);
        }

        public static double Popularity(int views)
        {
            return Math.Log10(1 + Math.Max(0, views));
        }

        public static double ProfileWeight(int position)
        {
            return 1.0 / (1 + position * ProfileDecay);
        }

        public static RecommendationReason PickReason(double tag, double text, double category, double author)
        {
            var best = Math.Max(Math.Max(tag, text), Math.Max(category, author));
            if (best <= 0) return RecommendationReason.Popular;
            if (tag == best) return RecommendationReason.SharedTopic;
            if (category == best) return RecommendationReason.SameSection;
            if (text == best) return RecommendationReason.SimilarStory;
            return RecommendationReason.SameAuthor;
        }

        public static List<int> MergeRecent(IEnumerable<int> recent, int articleId, int size)
        {
            return recent.Where(id => id != articleId && id > 0).Prepend(articleId).Distinct().Take(size).ToList();
        }
    }
}
