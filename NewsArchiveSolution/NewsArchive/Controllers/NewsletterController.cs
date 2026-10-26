using BLL;
using BLL.Email;
using BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;
using NewsArchive.Auth;
using NewsArchive.Email;

namespace NewsArchive.Controllers
{
    public class NewsletterController : AppController
    {
        private readonly INewsletterService newsletterService;
        private readonly IEmailSender emailSender;
        private readonly MailComposer mail;
        private readonly IStringLocalizer<SharedResource> L;

        public NewsletterController(INewsletterService newsletterService, IEmailSender emailSender, MailComposer mail, IStringLocalizer<SharedResource> localizer)
        {
            L = localizer;
            this.newsletterService = newsletterService;
            this.emailSender = emailSender;
            this.mail = mail;
        }

        private bool WantsJson => Request.Headers.Accept.ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase);

        [HttpPost("newsletter/subscribe")]
        [EnableRateLimiting(ApiOptions.NewsletterRateLimit)]
        public async Task<IActionResult> Subscribe(string? email, string? returnUrl)
        {
            var result = newsletterService.Subscribe(email, CurrentLanguage);
            if (result.Status == SubscribeStatus.ConfirmationNeeded)
                await emailSender.SendAsync(mail.SubscriptionConfirmation(result));

            var ok = result.Status != SubscribeStatus.InvalidEmail;
            if (WantsJson)
            {
                var text = ok ? L["Thanks! Check your inbox and confirm your subscription."] : L["Please enter a valid email address."];
                return ok ? Json(new { ok, message = text.Value }) : BadRequest(new { ok, message = text.Value });
            }

            if (ok)
                Flash("success", "Thanks! Check your inbox and confirm your subscription.");
            else
                Flash("danger", "Please enter a valid email address.");
            return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? LocalRedirect(returnUrl)
                : RedirectToAction("Index", "Article");
        }

        [HttpGet("newsletter/confirm")]
        public IActionResult Confirm(string? token)
        {
            var result = newsletterService.Confirm(token);
            ViewBag.UnsubscribeUrl = result.UnsubscribeToken == null ? null : mail.UnsubscribeUrl(result.UnsubscribeToken, result.LanguageCode);
            return View(result);
        }

        [HttpGet("newsletter/unsubscribe")]
        public IActionResult Unsubscribe(string? token)
        {
            ViewBag.Token = token;
            return View(newsletterService.GetByUnsubscribeToken(token));
        }

        [HttpPost("newsletter/unsubscribe")]
        [IgnoreAntiforgeryToken]
        [EnableRateLimiting(ApiOptions.NewsletterRateLimit)]
        public IActionResult UnsubscribeConfirmed(string? token)
        {
            var done = newsletterService.Unsubscribe(token);
            ViewBag.Done = done;
            ViewBag.Token = token;
            return View("Unsubscribe", newsletterService.GetByUnsubscribeToken(token));
        }
    }
}
