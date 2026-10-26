using BLL.DTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NewsArchive.Filters;

namespace NewsArchive.Controllers.Api
{
    [Route("api/bookmarks")]
    [ApiAuthorize]
    public class BookmarksApiController : ApiControllerBase
    {
        private readonly IBookmarkService bookmarkService;

        public BookmarksApiController(IBookmarkService bookmarkService)
        {
            this.bookmarkService = bookmarkService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDTO<ArticleDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 12)
        {
            var user = await RequireApiUserAsync();
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);
            var items = bookmarkService.GetSaved(user.UserId, page, pageSize, out var total);
            return Ok(new PagedResultDTO<ArticleDTO> { Items = items, Page = page, PageSize = pageSize, Total = total });
        }

        [HttpPost("{articleId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Add(int articleId)
        {
            var user = await RequireApiUserAsync();
            return bookmarkService.Add(user.UserId, articleId)
                ? NoContent()
                : Problem(statusCode: StatusCodes.Status400BadRequest, title: "This story could not be saved.");
        }

        [HttpDelete("{articleId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Remove(int articleId)
        {
            var user = await RequireApiUserAsync();
            return bookmarkService.Remove(user.UserId, articleId) ? NoContent() : NotFound();
        }
    }
}
