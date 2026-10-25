using System.Security.Cryptography;
using System.Text;

namespace BLL.Security
{
    public static class PasswordHasher
    {
        private const string Scheme = "PBKDF2";
        private const int Iterations = 100_000;
        private const int SaltSize = 16;
        private const int KeySize = 32;

        public static string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
            return $"{Scheme}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
        }

        public static bool Verify(string password, string storedHash, out bool needsUpgrade)
        {
            needsUpgrade = false;
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash)) return false;

            var parts = storedHash.Split('$');
            if (parts.Length == 4 && parts[0] == Scheme && int.TryParse(parts[1], out var iterations))
            {
                try
                {
                    var salt = Convert.FromBase64String(parts[2]);
                    var expected = Convert.FromBase64String(parts[3]);
                    var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
                    needsUpgrade = iterations < Iterations;
                    return CryptographicOperations.FixedTimeEquals(actual, expected);
                }
                catch (FormatException)
                {
                    return false;
                }
            }

            if (storedHash.Length == 32)
            {
                var legacy = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(password)));
                var matches = CryptographicOperations.FixedTimeEquals(
                    Encoding.ASCII.GetBytes(legacy.ToLowerInvariant()),
                    Encoding.ASCII.GetBytes(storedHash.ToLowerInvariant()));
                needsUpgrade = matches;
                return matches;
            }

            return false;
        }

        public const string StrongPasswordPattern = @"^(?=.*[A-Za-z])(?=.*\d).{8,}$";

        private static readonly string DummyHash = Hash(Convert.ToBase64String(RandomNumberGenerator.GetBytes(16)));

        public static void SimulateVerify(string? password)
        {
            Verify(password ?? string.Empty, DummyHash, out _);
        }

        public static string Stamp(string passwordHash)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(passwordHash)))[..16];
        }

        public static string NewToken()
        {
            return Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        }

        public static string HashToken(string token)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        }

        private static string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }
}
