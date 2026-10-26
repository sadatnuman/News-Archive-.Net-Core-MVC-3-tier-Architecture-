using BLL;

namespace NewsArchive.Personalization
{
    public static class SessionReadingList
    {
        private const string Key = "ReadIds";

        public static List<int> Get(ISession session)
        {
            var raw = session.GetString(Key);
            if (string.IsNullOrEmpty(raw)) return new List<int>();
            return raw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out var id) ? id : 0)
                .Where(id => id > 0)
                .Distinct()
                .Take(RecommendationRules.SessionHistorySize)
                .ToList();
        }

        public static List<int> Add(ISession session, int articleId)
        {
            var ids = RecommendationRules.MergeRecent(Get(session), articleId, RecommendationRules.SessionHistorySize);
            session.SetString(Key, string.Join(',', ids));
            return ids;
        }

        public static void Clear(ISession session)
        {
            session.Remove(Key);
        }
    }
}
