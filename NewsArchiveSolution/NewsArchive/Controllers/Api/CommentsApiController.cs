using BLL;
using BLL.DTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NewsArchive.Auth;
using NewsArchive.Filters;

namespace NewsArchive.Controllers.Api
{
    [Route("api/comments")]
    public class CommentsApiController : ApiControllerBase
    {
        private readonly ICommentService commentService;

        public CommentsApiController(ICommentService commentService)
        {
            this.commentService = commentService;
        }

        [HttpDelete("{id:int}")]
        [ApiAuthorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = commentService.Get(id);
            if (comment == null) return NotFound();

            var user = await RequireApiUserAsync();
            if (comment.UserId != user.UserId && user.RoleId != AppRoles.Admin)
                return Forbidden("You can only delete your own comments.");

            return commentService.Delete(id) ? NoContent() : NotFound();
        }

        [HttpGet("report-reasons")]
        [ProducesResponseType(typeof(List<ReportReasonDTO>), StatusCodes.Status200OK)]
        public IActionResult Reasons()
        {
            return Ok(commentService.GetReasons());
        }

        [HttpPost("{id:int}/report")]
        [ApiAuthorize]
        [EnableRateLimiting(ApiOptions.ReportRateLimit)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Report(int id, [FromBody] CommentReportCreateDTO report)
        {
            var user = await RequireApiUserAsync();
            return commentService.Report(id, user.UserId, report.ReasonId, report.Note) switch
            {
                ReportResult.Reported => Accepted(new { hidden = false }),
                ReportResult.AutoHidden => Accepted(new { hidden = true }),
                ReportResult.AlreadyReported => Problem(statusCode: StatusCodes.Status409Conflict, title: "You have already reported this comment."),
                ReportResult.OwnComment => Forbidden("You cannot report your own comment."),
                ReportResult.InvalidReason => Problem(statusCode: StatusCodes.Status400BadRequest, title: "Please choose a reason for the report."),
                _ => NotFound()
            };
        }

        [HttpGet("reported")]
        [ApiAuthorize(AppRoles.Admin)]
        [ProducesResponseType(typeof(PagedResultDTO<ModerationItemDTO>), StatusCodes.Status200OK)]
        public IActionResult Reported([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);
            var items = commentService.GetReported(page, pageSize, out var total);
            return Ok(new PagedResultDTO<ModerationItemDTO> { Items = items, Page = page, PageSize = pageSize, Total = total });
        }

        [HttpGet("hidden")]
        [ApiAuthorize(AppRoles.Admin)]
        [ProducesResponseType(typeof(PagedResultDTO<ModerationItemDTO>), StatusCodes.Status200OK)]
        public IActionResult Hidden([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);
            var items = commentService.GetHidden(page, pageSize, out var total);
            return Ok(new PagedResultDTO<ModerationItemDTO> { Items = items, Page = page, PageSize = pageSize, Total = total });
        }

        [HttpPost("{id:int}/hide")]
        [ApiAuthorize(AppRoles.Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Hide(int id)
        {
            var user = await RequireApiUserAsync();
            return commentService.Hide(id, user.UserId) ? NoContent() : NotFound();
        }

        [HttpPost("{id:int}/keep")]
        [ApiAuthorize(AppRoles.Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Keep(int id)
        {
            var user = await RequireApiUserAsync();
            return commentService.Keep(id, user.UserId) ? NoContent() : NotFound();
        }
    }
}
