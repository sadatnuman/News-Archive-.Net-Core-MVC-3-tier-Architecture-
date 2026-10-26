using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace NewsArchive.Auth
{
    public class PasswordResetStore
    {
        public const int MaxAttempts = 5;
        public const int MaxCodesPerWindow = 3;
        public static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan RequestWindow = TimeSpan.FromHours(1);

        private readonly IMemoryCache cache;
        private readonly object gate = new();

        public PasswordResetStore(IMemoryCache cache)
        {
            this.cache = cache;
        }

        private sealed class Entry
        {
            public byte[] CodeHash { get; init; } = Array.Empty<byte>();
            public DateTime ExpiresAt { get; init; }
            public int Attempts { get; set; }
            public bool Used { get; set; }
        }

        private sealed class Counter
        {
            public int Issued { get; set; }
            public int FailedAttempts { get; set; }
        }

        public enum VerifyResult
        {
            Valid,
            Invalid,
            Expired,
            TooManyAttempts,
            NotRequested
        }

        private static string Normalize(string email) => email.Trim().ToLowerInvariant();

        private static byte[] Hash(string code) => SHA256.HashData(Encoding.UTF8.GetBytes(code));

        private Counter CounterFor(string email)
        {
            return cache.GetOrCreate("PwdReset:Count:" + Normalize(email), e =>
            {
                e.AbsoluteExpirationRelativeToNow = RequestWindow;
                return new Counter();
            })!;
        }

        public string? Issue(int userId, string email)
        {
            lock (gate)
            {
                var counter = CounterFor(email);
                if (counter.Issued >= MaxCodesPerWindow || counter.FailedAttempts >= MaxAttempts * MaxCodesPerWindow) return null;
                counter.Issued++;

                var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString(System.Globalization.CultureInfo.InvariantCulture);
                cache.Set("PwdReset:Code:" + Normalize(email), new Entry
                {
                    CodeHash = Hash(code),
                    ExpiresAt = DateTime.UtcNow.Add(CodeLifetime)
                }, CodeLifetime);
                cache.Set("PwdReset:User:" + Normalize(email), userId, CodeLifetime);
                return code;
            }
        }

        public VerifyResult Verify(string email, string code, out int userId)
        {
            userId = 0;
            lock (gate)
            {
                var key = Normalize(email);
                if (!cache.TryGetValue("PwdReset:Code:" + key, out Entry? entry) || entry == null || entry.Used)
                    return VerifyResult.NotRequested;
                if (DateTime.UtcNow > entry.ExpiresAt)
                    return VerifyResult.Expired;
                if (entry.Attempts >= MaxAttempts)
                    return VerifyResult.TooManyAttempts;

                var candidate = (code ?? string.Empty).Trim();
                if (candidate.Length != 6 || !CryptographicOperations.FixedTimeEquals(entry.CodeHash, Hash(candidate)))
                {
                    entry.Attempts++;
                    CounterFor(email).FailedAttempts++;
                    if (entry.Attempts >= MaxAttempts) entry.Used = true;
                    return entry.Attempts >= MaxAttempts ? VerifyResult.TooManyAttempts : VerifyResult.Invalid;
                }

                entry.Used = true;
                if (!cache.TryGetValue("PwdReset:User:" + key, out int id)) return VerifyResult.NotRequested;
                userId = id;
                cache.Remove("PwdReset:Code:" + key);
                cache.Remove("PwdReset:User:" + key);
                return VerifyResult.Valid;
            }
        }
    }
}
