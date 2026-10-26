using BLL;
using BLL.DTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NewsArchive.Filters;

namespace NewsArchive.Controllers.Api
{
    [Route("api/tags")]
    public class TagsApiController : ApiControllerBase
    {
        private readonly ITagService tagService;

        public TagsApiController(ITagService tagService)
        {
            this.tagService = tagService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<TagDTO>), StatusCodes.Status200OK)]
        public IActionResult Get([FromQuery] string? lang)
        {
            return Ok(tagService.GetWithCounts(ParseLanguage(lang)));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(TagDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public IActionResult GetById(int id)
        {
            var tag = tagService.Get(id);
            return tag == null ? NotFound() : Ok(tag);
        }

        [HttpGet("{id:int}/articles")]
        [ProducesResponseType(typeof(PagedResultDTO<ArticleDTO>), StatusCodes.Status200OK)]
        public IActionResult Articles(int id, [FromQuery] string? lang, [FromQuery] int page = 1, [FromQuery] int pageSize = 12)
        {
            if (tagService.Get(id) == null) return NotFound();

            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);
            var items = tagService.GetArticles(id, ParseLanguage(lang), page, pageSize, out var total);
            return Ok(new PagedResultDTO<ArticleDTO> { Items = items, Page = page, PageSize = pageSize, Total = total });
        }

        [HttpPost]
        [ApiAuthorize(AppRoles.Admin)]
        [ProducesResponseType(typeof(TagDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public IActionResult Create([FromBody] TagDTO tag)
        {
            tag.TagId = 0;
            if (tagService.NameExists(tag.Name, 0) || tagService.NameExists(tag.NameBn, 0))
                return Problem(statusCode: StatusCodes.Status409Conflict, title: "A tag with this name already exists.");

            if (!tagService.Create(tag))
                return Problem(statusCode: StatusCodes.Status400BadRequest, title: "The tag could not be saved.");

            return CreatedAtAction(nameof(GetById), new { id = tag.TagId }, tag);
        }

        [HttpPut("{id:int}")]
        [ApiAuthorize(AppRoles.Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public IActionResult Update(int id, [FromBody] TagDTO tag)
        {
            if (tagService.Get(id) == null) return NotFound();
            tag.TagId = id;
            if (tagService.NameExists(tag.Name, id) || tagService.NameExists(tag.NameBn, id))
                return Problem(statusCode: StatusCodes.Status409Conflict, title: "A tag with this name already exists.");

            return tagService.Update(tag)
                ? NoContent()
                : Problem(statusCode: StatusCodes.Status400BadRequest, title: "The tag could not be saved.");
        }

        [HttpDelete("{id:int}")]
        [ApiAuthorize(AppRoles.Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public IActionResult Delete(int id)
        {
            if (tagService.Get(id) == null) return NotFound();
            return tagService.Delete(id)
                ? NoContent()
                : Problem(statusCode: StatusCodes.Status409Conflict, title: "The tag could not be deleted.");
        }
    }
}
