using System.Net;
using System.Text;
using BLL;
using BLL.DTOs;
using BLL.Email;
using Microsoft.Extensions.Options;
using NewsArchive.Seo;

namespace NewsArchive.Email
{
    public class MailComposer
    {
        private readonly SeoUrls urls;
        private readonly SiteOptions site;

        public MailComposer(SeoUrls urls, IOptions<SiteOptions> site)
        {
            this.urls = urls;
            this.site = site.Value;
        }

        private static bool Bn(string language) => language == AppLanguages.Bangla;

        private static string Text(string language, string english, string bangla) => Bn(language) ? bangla : english;

        private string Brand(string language) => Text(language, site.Name, "দ্য মেরিডিয়ান নিউজফোলিও");

        public string ConfirmUrl(string token, string language) =>
            urls.Absolute(UiLanguage.WithLanguage("/newsletter/confirm?token=" + Uri.EscapeDataString(token), language));

        public string UnsubscribeUrl(string token, string language) =>
            urls.Absolute(UiLanguage.WithLanguage("/newsletter/unsubscribe?token=" + Uri.EscapeDataString(token), language));

        private static string Html(string language, string title, string bodyHtml) =>
            $"<!doctype html><html lang=\"{language}\"><head><meta charset=\"utf-8\"><title>{WebUtility.HtmlEncode(title)}</title></head>" +
            "<body style=\"margin:0;padding:24px;background:#fbfaf7;font-family:Georgia,'Noto Serif Bengali',serif;color:#121212\">" +
            "<div style=\"max-width:600px;margin:0 auto;background:#ffffff;border-top:4px solid #c8102e;padding:24px\">" +
            bodyHtml + "</div></body></html>";

        public EmailMessage SubscriptionConfirmation(SubscribeResultDTO result)
        {
            var lang = result.LanguageCode;
            var confirm = ConfirmUrl(result.ConfirmToken!, lang);
            var unsubscribe = UnsubscribeUrl(result.UnsubscribeToken!, lang);
            var subject = Text(lang, "Confirm your newsletter subscription", "নিউজলেটার সাবস্ক্রিপশন নিশ্চিত করুন");
            var intro = Text(lang,
                "Please confirm that you want to receive the weekly digest of top stories.",
                "সপ্তাহের সেরা সংবাদের ডাইজেস্ট পেতে চাইলে সাবস্ক্রিপশন নিশ্চিত করুন।");
            var button = Text(lang, "Confirm subscription", "সাবস্ক্রিপশন নিশ্চিত করুন");
            var ignore = Text(lang,
                "If you did not ask for this, ignore this email. The link expires in 48 hours.",
                "আপনি এটি না চাইলে ইমেইলটি উপেক্ষা করুন। লিংকটি ৪৮ ঘণ্টা পর্যন্ত কাজ করবে।");

            var text = $"{Brand(lang)}\n\n{intro}\n\n{button}: {confirm}\n\n{ignore}";
            var html = Html(lang, subject,
                $"<h1 style=\"font-size:22px;margin:0 0 16px\">{WebUtility.HtmlEncode(Brand(lang))}</h1>" +
                $"<p>{WebUtility.HtmlEncode(intro)}</p>" +
                $"<p><a href=\"{WebUtility.HtmlEncode(confirm)}\" style=\"display:inline-block;background:#c8102e;color:#ffffff;padding:10px 18px;text-decoration:none;border-radius:6px\">{WebUtility.HtmlEncode(button)}</a></p>" +
                $"<p style=\"color:#6b6b6b;font-size:13px\">{WebUtility.HtmlEncode(ignore)}</p>");
            return new EmailMessage(result.Email, subject, text, html, "newsletter-confirm", unsubscribe);
        }

        public EmailMessage Digest(NewsletterDigestDTO digest, NewsletterRecipientDTO recipient, IReadOnlyDictionary<int, string> sections)
        {
            var lang = digest.LanguageCode;
            var unsubscribe = UnsubscribeUrl(recipient.UnsubscribeToken, lang);
            var subject = Text(lang, $"This week in {site.Name}", "এই সপ্তাহের সেরা সংবাদ");
            var intro = Text(lang, "The most important stories of the past week.", "গত সপ্তাহের গুরুত্বপূর্ণ সংবাদগুলো।");
            var read = Text(lang, "Read the story", "সংবাদটি পড়ুন");
            var footer = Text(lang, "You receive this because you subscribed to the newsletter.", "আপনি নিউজলেটারে সাবস্ক্রাইব করেছেন বলে এই ইমেইল পাচ্ছেন।");
            var leave = Text(lang, "Unsubscribe", "সাবস্ক্রিপশন বাতিল করুন");

            var text = new StringBuilder().AppendLine(Brand(lang)).AppendLine().AppendLine(intro).AppendLine();
            var html = new StringBuilder()
                .Append($"<h1 style=\"font-size:22px;margin:0 0 6px\">{WebUtility.HtmlEncode(Brand(lang))}</h1>")
                .Append($"<p style=\"color:#454545;margin:0 0 20px\">{WebUtility.HtmlEncode(intro)}</p>");

            foreach (var story in digest.Stories)
            {
                var link = urls.Absolute(SeoUrls.Article(story));
                var section = sections.TryGetValue(story.CategoryId, out var name) ? name : string.Empty;
                text.AppendLine($"{(section.Length > 0 ? section.ToUpperInvariant() + " | " : string.Empty)}{story.Title}")
                    .AppendLine(story.Summary)
                    .AppendLine(link)
                    .AppendLine();
                html.Append("<div style=\"border-top:1px solid #e7e4dd;padding:14px 0\">")
                    .Append(section.Length > 0 ? $"<div style=\"color:#c8102e;font:600 11px Arial,sans-serif;letter-spacing:.08em;text-transform:uppercase\">{WebUtility.HtmlEncode(section)}</div>" : string.Empty)
                    .Append($"<h2 style=\"font-size:18px;margin:4px 0 6px\"><a href=\"{WebUtility.HtmlEncode(link)}\" style=\"color:#121212;text-decoration:none\">{WebUtility.HtmlEncode(story.Title)}</a></h2>")
                    .Append($"<p style=\"margin:0 0 6px;color:#454545\">{WebUtility.HtmlEncode(story.Summary)}</p>")
                    .Append($"<a href=\"{WebUtility.HtmlEncode(link)}\" style=\"color:#c8102e;font:600 13px Arial,sans-serif\">{WebUtility.HtmlEncode(read)} &rarr;</a></div>");
            }

            text.AppendLine(footer).AppendLine($"{leave}: {unsubscribe}");
            html.Append($"<p style=\"color:#6b6b6b;font-size:12px;border-top:1px solid #e7e4dd;padding-top:14px\">{WebUtility.HtmlEncode(footer)} " +
                        $"<a href=\"{WebUtility.HtmlEncode(unsubscribe)}\" style=\"color:#6b6b6b\">{WebUtility.HtmlEncode(leave)}</a></p>");
            return new EmailMessage(recipient.Email, subject, text.ToString(), Html(lang, subject, html.ToString()), "newsletter-digest", unsubscribe);
        }

        public EmailMessage PasswordResetCode(string email, string code, string language, TimeSpan validFor)
        {
            var minutes = (int)validFor.TotalMinutes;
            var subject = Text(language, $"Your {site.Name} verification code", "আপনার যাচাইকরণ কোড");
            var line = Text(language, $"Your verification code is {code}. It expires in {minutes} minutes.",
                $"আপনার যাচাইকরণ কোড {code}। কোডটি {minutes} মিনিট পর্যন্ত কাজ করবে।");
            var warn = Text(language, "If you did not ask to reset your password, you can ignore this email. Never share this code.",
                "আপনি পাসওয়ার্ড বদলাতে না চাইলে ইমেইলটি উপেক্ষা করুন। কোডটি কাউকে জানাবেন না।");
            var html = Html(language, subject,
                $"<h1 style=\"font-size:22px;margin:0 0 16px\">{WebUtility.HtmlEncode(Brand(language))}</h1>" +
                $"<p>{WebUtility.HtmlEncode(line)}</p>" +
                $"<p style=\"font:700 28px monospace;letter-spacing:6px\">{WebUtility.HtmlEncode(code)}</p>" +
                $"<p style=\"color:#6b6b6b;font-size:13px\">{WebUtility.HtmlEncode(warn)}</p>");
            return new EmailMessage(email, subject, $"{Brand(language)}\n\n{line}\n\n{warn}", html, "password-reset");
        }
    }
}
