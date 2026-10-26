using BLL;
using BLL.DTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NewsArchive.Filters;

namespace NewsArchive.Controllers.Api
{
    [Route("api/audit")]
    [ApiAuthorize(AppRoles.Admin)]
    public class AuditApiController : ApiControllerBase
    {
        private readonly IAuditService auditService;

        public AuditApiController(IAuditService auditService)
        {
            this.auditService = auditService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDTO<AuditEntryDTO>), StatusCodes.Status200OK)]
        public IActionResult Get([FromQuery] string? type, [FromQuery] int? user, [FromQuery] int page = 1, [FromQuery] int pageSize = 30)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);
            var items = auditService.Get(type, user, page, pageSize, out var total);
            return Ok(new PagedResultDTO<AuditEntryDTO> { Items = items, Page = page, PageSize = pageSize, Total = total });
        }
    }
}
