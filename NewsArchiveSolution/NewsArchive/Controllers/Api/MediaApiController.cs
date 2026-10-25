using BLL;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using NewsArchive.Filters;

namespace NewsArchive.Controllers.Api
{
    [Route("api/media")]
    public class MediaApiController : ApiControllerBase
    {
        private readonly MediaService mediaService;

        public MediaApiController(MediaService mediaService)
        {
            this.mediaService = mediaService;
        }

        [HttpPost("images")]
        [ApiAuthorize(AppRoles.Admin, AppRoles.Author)]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(MediaService.MaxRequestBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = MediaService.MaxRequestBytes)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public IActionResult UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Choose an image to upload.");

            using var stream = file.OpenReadStream();
            var result = mediaService.SaveArticleImage(stream, file.Length);
            if (!result.Success)
                return Problem(statusCode: StatusCodes.Status400BadRequest, title: result.Error);

            return Created(result.Path!, new { path = result.Path });
        }
    }
}
