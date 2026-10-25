using BLL;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using NewsArchive.Filters;

namespace NewsArchive.Controllers.Api
{
    [Route("api/comments")]
    public class CommentsApiController : ApiControllerBase
    {
        private readonly CommentService commentService;

        public CommentsApiController(CommentService commentService)
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

            commentService.Delete(id);
            return NoContent();
        }
    }
}
