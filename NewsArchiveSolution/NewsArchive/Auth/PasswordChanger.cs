using System.Globalization;
using BLL.DTOs;
using BLL.Interfaces;

namespace NewsArchive.Auth
{
    public class PasswordChanger
    {
        public const string ResetSessionKey = "PwdReset:VerifiedUntil";
        public static readonly TimeSpan ResetWindow = TimeSpan.FromMinutes(10);

        private readonly IAuthService authService;
        private readonly LoginThrottle throttle;

        public PasswordChanger(IAuthService authService, LoginThrottle throttle)
        {
            this.authService = authService;
            this.throttle = throttle;
        }

        public enum Outcome
        {
            Changed,
            WrongCurrentPassword,
            Locked,
            Failed
        }

        public static void MarkResetVerified(ISession session)
        {
            session.SetString(ResetSessionKey, DateTime.UtcNow.Add(ResetWindow).ToString("o", CultureInfo.InvariantCulture));
        }

        public static bool IsResetVerified(ISession session)
        {
            var value = session.GetString(ResetSessionKey);
            return value != null
                && DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var until)
                && DateTime.UtcNow <= until;
        }

        public async Task<Outcome> ChangeAsync(ISession session, int userId, string? email, ChangePasswordDTO form)
        {
            if (throttle.IsLocked(email)) return Outcome.Locked;

            var viaReset = IsResetVerified(session);
            if (!viaReset && !authService.VerifyPassword(userId, form.CurrentPassword))
            {
                throttle.RecordFailure(email);
                return Outcome.WrongCurrentPassword;
            }

            if (!authService.ChangePassword(userId, form.PasswordHash)) return Outcome.Failed;

            session.Remove(ResetSessionKey);
            throttle.Reset(email);
            await authService.RevokeAllAsync(userId);
            return Outcome.Changed;
        }
    }
}
