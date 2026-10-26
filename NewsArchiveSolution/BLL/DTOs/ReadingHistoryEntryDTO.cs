namespace BLL.DTOs
{
    public class ReadingHistoryEntryDTO
    {
        public ArticleDTO Article { get; set; } = null!;

        public DateTime FirstReadAt { get; set; }

        public DateTime LastReadAt { get; set; }

        public int ReadCount { get; set; }
    }
}
