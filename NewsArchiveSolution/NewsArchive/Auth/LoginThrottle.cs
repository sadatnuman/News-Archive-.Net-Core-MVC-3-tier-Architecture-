using Microsoft.Extensions.Caching.Memory;

namespace NewsArchive.Auth
{
    public class LoginThrottle
    {
        public const int MaxFailures = 5;
        public static readonly TimeSpan LockWindow = TimeSpan.FromMinutes(15);

        private readonly IMemoryCache cache;

        public LoginThrottle(IMemoryCache cache)
        {
            this.cache = cache;
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

        private static string Key(string? email)
        {
            return "login-failures:" + (email ?? string.Empty).Trim().ToLowerInvariant();
        }
    }
}
