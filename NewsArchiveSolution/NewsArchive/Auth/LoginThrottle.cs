using Microsoft.Extensions.Caching.Memory;

namespace NewsArchive.Auth
{
    public class LoginThrottle
    {
        public const int MaxFailures = 5;
        public static readonly TimeSpan LockWindow = TimeSpan.FromMinutes(15);

        private readonly IMemoryCache cache;
        private readonly IHttpContextAccessor http;

        public LoginThrottle(IMemoryCache cache, IHttpContextAccessor http)
        {
            this.cache = cache;
            this.http = http;
        }

        public bool IsLocked(string? email)
        {
            return cache.TryGetValue(Key(email), out int failures) && failures >= MaxFailures;
        }

        public void RecordFailure(string? email)
        {
            var key = Key(email);
            var failures = cache.TryGetValue(key, out int current) ? current : 0;
            cache.Set(key, failures + 1, LockWindow);
        }

        public void Reset(string? email)
        {
            cache.Remove(Key(email));
        }

        private string Key(string? email)
        {
            var ip = http.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return "login-failures:" + (email ?? string.Empty).Trim().ToLowerInvariant() + "|" + ip;
        }
    }
}
