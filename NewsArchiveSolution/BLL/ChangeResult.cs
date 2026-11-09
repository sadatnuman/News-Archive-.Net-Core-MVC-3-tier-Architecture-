namespace BLL
{
    public enum ChangeResult
    {
        Done,
        NotAllowed,
        Conflict
    }

    public static class ArticleMessages
    {
        public const string ChangedElsewhere = "Someone else changed this article after you opened it. Your changes were not saved. Reload the page to see the latest version.";
        public const string VersionRequired = "Send the version returned by GET /api/articles/{id} in the JSON property \"version\".";
        public const string SectionMissing = "This section no longer exists. Choose another section; your text has been kept.";
        public const string DecidedElsewhere = "Another editor acted on this story at the same moment. Your decision was not saved. Reload the queue to see its current state.";
    }
}
