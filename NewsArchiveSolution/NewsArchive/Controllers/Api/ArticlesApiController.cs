using BLL;
using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NewsArchive.Auth;
using NewsArchive.Filters;

namespace NewsArchive.Controllers.Api
{
    [Route("api/articles")]
    public class ArticlesApiController : ApiControllerBase
    {
        private readonly ArticleService articleService;
        private readonly CategoryService categoryService;
        private readonly CommentService commentService;
        private readonly MediaService mediaService;

        public ArticlesApiController(ArticleService articleService, CategoryService categoryService, CommentService commentService, MediaService mediaService)
        {
            this.mediaService = mediaService;
            this.articleService = articleService;
            this.categoryService = categoryService;
            this.commentService = commentService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDTO<ArticleDTO>), StatusCodes.Status200OK)]
        public IActionResult Get([FromQuery] string? q, [FromQuery] DateTime? from, [FromQuery] DateTime? to,
            [FromQuery] string? lang, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);
            var items = articleService.Search(q, from, to, page, pageSize, out var total, ParseLanguage(lang));
            return Ok(new PagedResultDTO<ArticleDTO> { Items = items, Page = page, PageSize = pageSize, Total = total });
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ArticleDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var article = articleService.Get(id);
            if (article == null) return NotFound();

            if (!article.IsPublished)
            {
                var user = await GetApiUserAsync();
                if (!ArticleService.CanView(article, user?.UserId, user?.RoleId)) return NotFound();
            }
            return Ok(article);
        }

        [HttpGet("breaking")]
        [ProducesResponseType(typeof(List<ArticleDTO>), StatusCodes.Status200OK)]
        public IActionResult Breaking([FromQuery] string? lang)
        {
            return Ok(articleService.GetBreakingNews(ParseLanguage(lang)));
        }

        [HttpGet("trending")]
        [ProducesResponseType(typeof(List<ArticleDTO>), StatusCodes.Status200OK)]
        public IActionResult Trending([FromQuery] string? lang, [FromQuery] int count = 10)
        {
            return Ok(articleService.GetTrendingArticles(Math.Clamp(count, 1, 50), ParseLanguage(lang)));
        }

        [HttpGet("{id:int}/related")]
        [ProducesResponseType(typeof(List<ArticleDTO>), StatusCodes.Status200OK)]
        public IActionResult Related(int id, [FromQuery] int count = 4)
        {
            var article = articleService.Get(id);
            if (article == null || !article.IsPublished) return NotFound();
            return Ok(articleService.GetRecommendedArticles(id, Math.Clamp(count, 1, 20)));
        }

        [HttpGet("mine")]
        [ApiAuthorize(AppRoles.Author, AppRoles.Admin)]
        [ProducesResponseType(typeof(List<ArticleDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Mine()
        {
            var user = await RequireApiUserAsync();
            return Ok(articleService.GetByAuthor(user.UserId));
        }

        [HttpGet("pending")]
        [ApiAuthorize(AppRoles.Admin)]
        [ProducesResponseType(typeof(List<ArticleDTO>), StatusCodes.Status200OK)]
        public IActionResult Pending()
        {
            return Ok(articleService.GetPending());
        }

        [HttpPost]
        [ApiAuthorize(AppRoles.Admin, AppRoles.Author)]
        [ProducesResponseType(typeof(ArticleDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] ArticleFormDTO form, [FromQuery] bool submit = false)
        {
            if (categoryService.Get(form.CategoryId) == null)
            {
                ModelState.AddModelError(nameof(form.CategoryId), "Category does not exist.");
                return ValidationProblem(ModelState);
            }

            var user = await RequireApiUserAsync();
            var id = articleService.Create(form, user.UserId, user.RoleId, submit);
            if (id == 0) return Problem(statusCode: StatusCodes.Status400BadRequest, title: "The article could not be saved.");

            return CreatedAtAction(nameof(GetById), new { id }, articleService.Get(id));
        }

        [HttpPut("{id:int}")]
        [ApiAuthorize(AppRoles.Admin, AppRoles.Author)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] ArticleFormDTO form, [FromQuery] bool submit = false)
        {
            var article = articleService.Get(id);
            if (article == null) return NotFound();

            var user = await RequireApiUserAsync();
            if (!ArticleService.CanEdit(article.AuthorId, article.StatusId, user.UserId, user.RoleId))
                return Forbidden("You cannot edit this article.");

            if (categoryService.Get(form.CategoryId) == null)
            {
                ModelState.AddModelError(nameof(form.CategoryId), "Category does not exist.");
                return ValidationProblem(ModelState);
            }

            return articleService.Update(id, form, user.UserId, user.RoleId, submit)
                ? NoContent()
                : Problem(statusCode: StatusCodes.Status400BadRequest, title: "The article could not be saved.");
        }

        [HttpDelete("{id:int}")]
        [ApiAuthorize(AppRoles.Admin, AppRoles.Author)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var article = articleService.Get(id);
            if (article == null) return NotFound();

            var user = await RequireApiUserAsync();
            if (!ArticleService.CanEdit(article.AuthorId, article.StatusId, user.UserId, user.RoleId))
                return Forbidden("You cannot delete this article.");

            if (!articleService.Delete(id, user.UserId, user.RoleId))
                return Problem(statusCode: StatusCodes.Status409Conflict, title: "The article could not be deleted.");

            mediaService.DeleteIfUploaded(article.ImagePath);
            return NoContent();
        }

        [HttpPost("{id:int}/submit")]
        [ApiAuthorize(AppRoles.Author)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Submit(int id)
        {
            var user = await RequireApiUserAsync();
            var article = articleService.Get(id);
            if (article == null || article.AuthorId != user.UserId) return NotFound();

            return articleService.Submit(id, user.UserId)
                ? NoContent()
                : Problem(statusCode: StatusCodes.Status409Conflict, title: "Only drafts and rejected articles can be submitted.");
        }

        [HttpPost("{id:int}/approve")]
        [ApiAuthorize(AppRoles.Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Approve(int id, [FromBody] ApproveDTO? approve)
        {
            if (articleService.Get(id) == null) return NotFound();
            var user = await RequireApiUserAsync();

            return articleService.Approve(id, user.UserId, approve?.PublishAt)
                ? NoContent()
                : Problem(statusCode: StatusCodes.Status409Conflict, title: "Only pending articles can be approved.");
        }

        [HttpGet("scheduled")]
        [ApiAuthorize(AppRoles.Admin)]
        [ProducesResponseType(typeof(List<ArticleDTO>), StatusCodes.Status200OK)]
        public IActionResult Scheduled()
        {
            return Ok(articleService.GetScheduled());
        }

        [HttpPost("{id:int}/publish-now")]
        [ApiAuthorize(AppRoles.Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public IActionResult PublishNow(int id)
        {
            if (articleService.Get(id) == null) return NotFound();
            return articleService.PublishNow(id)
                ? NoContent()
                : Problem(statusCode: StatusCodes.Status409Conflict, title: "Only scheduled articles can be published early.");
        }

        [HttpPost("{id:int}/reject")]
        [ApiAuthorize(AppRoles.Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Reject(int id, [FromBody] ReviewDTO? review)
        {
            if (articleService.Get(id) == null) return NotFound();
            var user = await RequireApiUserAsync();

            return articleService.Reject(id, user.UserId, review?.Note)
                ? NoContent()
                : Problem(statusCode: StatusCodes.Status409Conflict, title: "Only pending articles can be rejected.");
        }

        [HttpGet("{id:int}/comments")]
        [ProducesResponseType(typeof(List<CommentDTO>), StatusCodes.Status200OK)]
        public IActionResult Comments(int id)
        {
            var article = articleService.Get(id);
            if (article == null || !article.IsPublished) return NotFound();
            return Ok(commentService.GetByArticleId(id));
        }

        [HttpPost("{id:int}/comments")]
        [ApiAuthorize]
        [EnableRateLimiting(ApiOptions.CommentRateLimit)]
        [ProducesResponseType(typeof(CommentDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddComment(int id, [FromBody] CommentCreateDTO comment)
        {
            var article = articleService.Get(id);
            if (article == null || !article.IsPublished) return NotFound();

            var user = await RequireApiUserAsync();
            var dto = new CommentDTO { ArticleId = id, UserId = user.UserId, Text = comment.Text, CreatedDate = DateTime.Now };
            if (!commentService.Create(dto))
                return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Comments must be between 2 and 500 characters.");

            return CreatedAtAction(nameof(Comments), new { id }, dto);
        }
    }
}
