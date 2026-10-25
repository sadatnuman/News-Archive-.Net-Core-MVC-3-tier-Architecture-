using Microsoft.AspNetCore.Mvc;

namespace NewsArchive.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class ApiAuthorize : TypeFilterAttribute
    {
        public ApiAuthorize(params int[] roles) : base(typeof(TokenAuthorizationFilter))
        {
            Arguments = new object[] { roles };
        }
    }
}
