using BLL;
using BLL.DTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NewsArchive.Filters;

namespace NewsArchive.Controllers.Api
{
    [Route("api")]
    [ApiAuthorize]
    public class ReadingApiController : ApiControllerBase
    {
        private readonly IRecommendationService recommendationService;

        public ReadingApiController(IRecommendationService recommendationService)
        {
            this.recommendationService = recommendationService;
        }

        [HttpGet("recommendations")]
        [ProducesResponseType(typeof(List<RecommendationDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ForMe([FromQuery] string? lang, [FromQuery] int count = 6)
        {
            var user = await RequireApiUserAsync();
            var language = ParseLanguage(lang) ?? AppLanguages.English;
            return Ok(recommendationService.ForReader(user.UserId, null, language, Math.Clamp(count, 1, RecommendationRules.MaxCount)));
        }

        [HttpGet("history")]
        [ProducesResponseType(typeof(PagedResultDTO<ReadingHistoryEntryDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> History([FromQuery] int page = 1, [FromQuery] int pageSize = 12)
        {
            var user = await RequireApiUserAsync();
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);
            var items = recommendationService.GetHistory(user.UserId, page, pageSize, out var total);
            return Ok(new PagedResultDTO<ReadingHistoryEntryDTO> { Items = items, Page = page, PageSize = pageSize, Total = total });
        }

        [HttpPost("history/{articleId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Record(int articleId)
        {
            var user = await RequireApiUserAsync();
            return recommendationService.RecordRead(user.UserId, articleId) ? NoContent() : NotFound();
        }

        [HttpDelete("history/{articleId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Remove(int articleId)
        {
            var user = await RequireApiUserAsync();
            return recommendationService.RemoveFromHistory(user.UserId, articleId) ? NoContent() : NotFound();
        }

        [HttpDelete("history")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Clear()
        {
            var user = await RequireApiUserAsync();
            return Ok(new { removed = recommendationService.ClearHistory(user.UserId) });
        }
    }
}
