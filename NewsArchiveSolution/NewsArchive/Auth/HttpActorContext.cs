using BLL.Audit;
using NewsArchive.Filters;

namespace NewsArchive.Auth
{
    public class HttpActorContext : IActorContext
    {
        private readonly IHttpContextAccessor accessor;

        public HttpActorContext(IHttpContextAccessor accessor)
        {
            this.accessor = accessor;
        }

        public int? UserId
        {
            get
            {
                var http = accessor.HttpContext;
                if (http == null) return null;
                return TokenAuthorizationFilter.CurrentUser(http)?.UserId ?? http.User.UserId();
            }
        }

        public string? IpAddress => accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    }
}
