using Microsoft.Extensions.Caching.Memory;

namespace NewsArchive.Auth
{
    public class LoginThrottle
    {
        public const int MaxFailures = 5;
        public static readonly TimeSpan LockWindow = TimeSpan.FromMinutes(15);

        public enum Outcome
        {
            Locked,
            Failed,
            Succeeded
        }

        private sealed class Counter
        {
            public int Failures { get; set; }
            public int InFlight { get; set; }
        }

        private readonly IMemoryCache cache;
        private readonly IHttpContextAccessor http;
        private readonly object gate = new();

        public LoginThrottle(IMemoryCache cache, IHttpContextAccessor http)
        {
            this.cache = cache;
            this.http = http;
        }

        public bool IsLocked(string? email)
        {
            lock (gate)
            {
                return cache.TryGetValue(Key(email), out Counter? counter) && counter!.Failures >= MaxFailures;
            }
        }

        public Outcome Attempt(string? email, Func<bool> verify)
        {
            var key = Key(email);
            lock (gate)
            {
                var counter = CounterFor(key);
                if (counter.Failures + counter.InFlight >= MaxFailures) return Outcome.Locked;
                counter.InFlight++;
            }

            var finished = false;
            var succeeded = false;
            try
            {
                succeeded = verify();
                finished = true;
            }
            finally
            {
                lock (gate)
                {
                    var counter = CounterFor(key);
                    counter.InFlight = Math.Max(0, counter.InFlight - 1);
                    if (finished && succeeded)
                        counter.Failures = 0;
                    else if (finished)
                    {
                        counter.Failures++;
                        cache.Set(key, counter, LockWindow);
                    }
                }
            }
            return succeeded ? Outcome.Succeeded : Outcome.Failed;
        }

        public void Reset(string? email)
        {
            lock (gate)
            {
                if (cache.TryGetValue(Key(email), out Counter? counter))
                    counter!.Failures = 0;
            }
        }

        private Counter CounterFor(string key)
        {
            return cache.GetOrCreate(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = LockWindow;
                return new Counter();
            })!;
        }

        private string Key(string? email)
        {
            var ip = http.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return "login-failures:" + (email ?? string.Empty).Trim().ToLowerInvariant() + "|" + ip;
        }
    }
}
