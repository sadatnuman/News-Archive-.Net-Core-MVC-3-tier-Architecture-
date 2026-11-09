using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface IArticleArchive
    {
        List<ArticleDTO> GetArchive(int year, int? month, int? day, string? language, int page, int pageSize, out int total);

        List<ArchiveMonthDTO> GetArchiveMonths(string? language = null);

        Dictionary<int, int> GetArchiveDayCounts(int year, int month, string? language = null);
    }
}
