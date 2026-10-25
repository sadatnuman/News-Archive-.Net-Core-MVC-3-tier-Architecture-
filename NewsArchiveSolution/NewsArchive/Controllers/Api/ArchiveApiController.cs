using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace NewsArchive.Controllers.Api
{
    [Route("api/archive")]
    public class ArchiveApiController : ApiControllerBase
    {
        private readonly ArticleService articleService;

        public ArchiveApiController(ArticleService articleService)
        {
            this.articleService = articleService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ArchiveMonthDTO>), StatusCodes.Status200OK)]
        public IActionResult Months([FromQuery] string? lang)
        {
            return Ok(articleService.GetArchiveMonths(ParseLanguage(lang)));
        }

        [HttpGet("{year:int}/{month:int}")]
        [ProducesResponseType(typeof(PagedResultDTO<ArticleDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public IActionResult Period(int year, int month, [FromQuery] int? day, [FromQuery] string? lang, [FromQuery] int page = 1, [FromQuery] int pageSize = 12)
        {
            if (!ArticleService.IsValidArchiveDate(year, month, day)) return NotFound();

            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);
            var items = articleService.GetArchive(year, month, day, ParseLanguage(lang), page, pageSize, out var total);
            return Ok(new PagedResultDTO<ArticleDTO> { Items = items, Page = page, PageSize = pageSize, Total = total });
        }

        [HttpGet("{year:int}/{month:int}/days")]
        [ProducesResponseType(typeof(Dictionary<int, int>), StatusCodes.Status200OK)]
        public IActionResult Days(int year, int month, [FromQuery] string? lang)
        {
            if (!ArticleService.IsValidArchiveDate(year, month)) return NotFound();
            return Ok(articleService.GetArchiveDayCounts(year, month, ParseLanguage(lang)));
        }
    }
}
