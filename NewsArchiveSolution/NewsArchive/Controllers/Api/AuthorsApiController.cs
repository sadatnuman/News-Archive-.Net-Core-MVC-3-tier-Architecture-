using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace NewsArchive.Controllers.Api
{
    [Route("api/authors")]
    public class AuthorsApiController : ApiControllerBase
    {
        private readonly AuthorService authorService;

        public AuthorsApiController(AuthorService authorService)
        {
            this.authorService = authorService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<AuthorDTO>), StatusCodes.Status200OK)]
        public IActionResult Get([FromQuery] string? lang)
        {
            return Ok(authorService.GetAuthors(ParseLanguage(lang)));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(AuthorDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public IActionResult GetById(int id)
        {
            var author = authorService.GetAuthor(id);
            return author == null ? NotFound() : Ok(author);
        }

        [HttpGet("{id:int}/articles")]
        [ProducesResponseType(typeof(PagedResultDTO<ArticleDTO>), StatusCodes.Status200OK)]
        public IActionResult Articles(int id, [FromQuery] string? lang, [FromQuery] int page = 1, [FromQuery] int pageSize = 12)
        {
            if (authorService.GetAuthor(id) == null) return NotFound();

            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);
            var items = authorService.GetArticles(id, ParseLanguage(lang), page, pageSize, out var total);
            return Ok(new PagedResultDTO<ArticleDTO> { Items = items, Page = page, PageSize = pageSize, Total = total });
        }
    }
}
