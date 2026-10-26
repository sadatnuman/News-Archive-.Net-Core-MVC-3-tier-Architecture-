using BLL;
using BLL.DTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NewsArchive.Filters;

namespace NewsArchive.Controllers.Api
{
    [Route("api/categories")]
    public class CategoriesApiController : ApiControllerBase
    {
        private readonly ICategoryService categoryService;
        private readonly IArticleService articleService;

        public CategoriesApiController(ICategoryService categoryService, IArticleService articleService)
        {
            this.categoryService = categoryService;
            this.articleService = articleService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<CategoryDTO>), StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            return Ok(categoryService.Get());
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(CategoryDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public IActionResult GetById(int id)
        {
            var category = categoryService.Get(id);
            return category == null ? NotFound() : Ok(category);
        }

        [HttpGet("{id:int}/articles")]
        [ProducesResponseType(typeof(PagedResultDTO<ArticleDTO>), StatusCodes.Status200OK)]
        public IActionResult Articles(int id, [FromQuery] string? lang, [FromQuery] int page = 1, [FromQuery] int pageSize = 12)
        {
            if (categoryService.Get(id) == null) return NotFound();
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);
            var items = articleService.GetByCategoryId(id, ParseLanguage(lang), page, pageSize, out var total);
            return Ok(new PagedResultDTO<ArticleDTO> { Items = items, Page = page, PageSize = pageSize, Total = total });
        }

        [HttpPost]
        [ApiAuthorize(AppRoles.Admin)]
        [ProducesResponseType(typeof(CategoryDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public IActionResult Create([FromBody] CategoryDTO category)
        {
            category.CategoryId = 0;
            if (categoryService.NameExists(category.Name, 0))
                return Problem(statusCode: StatusCodes.Status409Conflict, title: "A section with this name already exists.");

            if (!categoryService.Create(category))
                return Problem(statusCode: StatusCodes.Status400BadRequest, title: "The section could not be saved.");

            return CreatedAtAction(nameof(GetById), new { id = category.CategoryId }, category);
        }

        [HttpPut("{id:int}")]
        [ApiAuthorize(AppRoles.Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public IActionResult Update(int id, [FromBody] CategoryDTO category)
        {
            if (categoryService.Get(id) == null) return NotFound();
            category.CategoryId = id;
            if (categoryService.NameExists(category.Name, id))
                return Problem(statusCode: StatusCodes.Status409Conflict, title: "A section with this name already exists.");

            return categoryService.Update(category)
                ? NoContent()
                : Problem(statusCode: StatusCodes.Status400BadRequest, title: "The section could not be saved.");
        }

        [HttpDelete("{id:int}")]
        [ApiAuthorize(AppRoles.Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public IActionResult Delete(int id, [FromQuery] int? moveArticlesTo)
        {
            if (categoryService.Get(id) == null) return NotFound();

            return categoryService.Delete(id, moveArticlesTo)
                ? NoContent()
                : Problem(statusCode: StatusCodes.Status409Conflict,
                    title: "This section has articles.",
                    detail: "Pass moveArticlesTo with another section id to move its articles before deleting.");
        }
    }
}
