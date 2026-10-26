using BLL.Audit;
using BLL.DTOs;
using BLL.Interfaces;
using BLL.Security;
using DAL.EF.Tables;
using DAL.Interfaces;
using AutoMapper;

namespace BLL.Services
{
    public class NewsletterService : INewsletterService
    {
        private readonly ISubscriberRepository repo;
        private readonly IArticleRepository articleRepo;
        private readonly IMapper mapper;
        private readonly IAuditService audit;

        public NewsletterService(IDataAccessFactory factory, IMapper mapper, IAuditService audit)
        {
            repo = factory.GetSubscriberRepository();
            articleRepo = factory.GetArticleRepository();
            this.mapper = mapper;
            this.audit = audit;
        }

        private static SubscriberDTO ToDto(Subscriber s) => new()
        {
            SubscriberId = s.SubscriberId,
            Email = s.Email,
            LanguageCode = s.LanguageCode,
            CreatedAt = s.CreatedAt,
            ConfirmedAt = s.ConfirmedAt,
            UnsubscribedAt = s.UnsubscribedAt,
            LastSentAt = s.LastSentAt
        };

        public SubscribeResultDTO Subscribe(string? email, string language)
        {
            var normalized = NewsletterRules.NormalizeEmail(email);
            var lang = AppLanguages.Normalize(language);
            if (normalized == null) return new SubscribeResultDTO { Status = SubscribeStatus.InvalidEmail, LanguageCode = lang };

            var now = DateTime.Now;
            var existing = repo.GetByEmail(normalized);
            var result = new SubscribeResultDTO { Email = normalized, LanguageCode = lang };

            if (existing != null && existing.ConfirmedAt != null && existing.UnsubscribedAt == null)
            {
                if (existing.LanguageCode != lang)
                {
                    existing.LanguageCode = lang;
                    repo.Save(existing);
                }
                result.Status = SubscribeStatus.AlreadySubscribed;
                return result;
            }

            if (existing != null && existing.ConfirmedAt == null && existing.UnsubscribedAt == null && now - existing.CreatedAt < NewsletterRules.ResendDelay)
            {
                result.Status = SubscribeStatus.TooSoon;
                return result;
            }

            var confirmToken = PasswordHasher.NewHexToken();
            var unsubscribeToken = PasswordHasher.NewHexToken();
            bool saved;
            if (existing == null)
            {
                saved = repo.Create(new Subscriber
                {
                    Email = normalized,
                    LanguageCode = lang,
                    ConfirmTokenHash = PasswordHasher.HashToken(confirmToken),
                    UnsubscribeToken = unsubscribeToken,
                    CreatedAt = now
                });
            }
            else
            {
                existing.LanguageCode = lang;
                existing.ConfirmTokenHash = PasswordHasher.HashToken(confirmToken);
                existing.UnsubscribeToken = unsubscribeToken;
                existing.CreatedAt = now;
                existing.ConfirmedAt = null;
                existing.UnsubscribedAt = null;
                saved = repo.Save(existing);
            }

            if (!saved)
            {
                result.Status = SubscribeStatus.TooSoon;
                return result;
            }

            result.Status = SubscribeStatus.ConfirmationNeeded;
            result.ConfirmToken = confirmToken;
            result.UnsubscribeToken = unsubscribeToken;
            return result;
        }

        public ConfirmResultDTO Confirm(string? token)
        {
            if (string.IsNullOrWhiteSpace(token) || token.Length > 128) return new ConfirmResultDTO { Status = ConfirmStatus.NotFound };

            var subscriber = repo.GetByConfirmHash(PasswordHasher.HashToken(token.Trim().ToUpperInvariant()));
            if (subscriber == null || subscriber.UnsubscribedAt != null) return new ConfirmResultDTO { Status = ConfirmStatus.NotFound };

            var result = new ConfirmResultDTO { Email = subscriber.Email, LanguageCode = subscriber.LanguageCode, UnsubscribeToken = subscriber.UnsubscribeToken };
            if (subscriber.ConfirmedAt != null)
            {
                result.Status = ConfirmStatus.AlreadyConfirmed;
                return result;
            }
            if (DateTime.Now - subscriber.CreatedAt > NewsletterRules.ConfirmWindow)
            {
                result.Status = ConfirmStatus.Expired;
                return result;
            }

            subscriber.ConfirmedAt = DateTime.Now;
            result.Status = repo.Save(subscriber) ? ConfirmStatus.Confirmed : ConfirmStatus.NotFound;
            return result;
        }

        public SubscriberDTO? GetByUnsubscribeToken(string? token)
        {
            if (string.IsNullOrWhiteSpace(token) || token.Length > 128) return null;
            var subscriber = repo.GetByUnsubscribeToken(token.Trim().ToUpperInvariant());
            return subscriber == null ? null : ToDto(subscriber);
        }

        public bool Unsubscribe(string? token)
        {
            if (string.IsNullOrWhiteSpace(token) || token.Length > 128) return false;
            var subscriber = repo.GetByUnsubscribeToken(token.Trim().ToUpperInvariant());
            if (subscriber == null) return false;
            if (subscriber.UnsubscribedAt != null) return true;

            subscriber.UnsubscribedAt = DateTime.Now;
            return repo.Save(subscriber);
        }

        public List<SubscriberDTO> Get(int page, int pageSize, out int total)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 30;
            return repo.Get(page, pageSize, out total).Select(ToDto).ToList();
        }

        public NewsletterStatsDTO GetStats()
        {
            var counts = repo.CountActiveByLanguage();
            return new NewsletterStatsDTO
            {
                ActiveEnglish = counts.GetValueOrDefault(AppLanguages.English),
                ActiveBangla = counts.GetValueOrDefault(AppLanguages.Bangla),
                Pending = repo.CountPending()
            };
        }

        public NewsletterDigestDTO BuildDigest(string language)
        {
            var lang = AppLanguages.Normalize(language);
            var now = DateTime.Now;
            var stories = articleRepo.GetByPeriod(now.AddDays(-NewsletterRules.DigestDays), now.AddMinutes(1), lang, 1, NewsletterRules.DigestStories, out _);
            return new NewsletterDigestDTO
            {
                LanguageCode = lang,
                Stories = mapper.Map<List<ArticleDTO>>(stories),
                Recipients = repo.GetActive(lang)
                    .Select(s => new NewsletterRecipientDTO { SubscriberId = s.SubscriberId, Email = s.Email, UnsubscribeToken = s.UnsubscribeToken })
                    .ToList()
            };
        }

        public void MarkSent(IEnumerable<int> subscriberIds, string language, int stories)
        {
            var ids = subscriberIds.ToList();
            var count = ids.Count == 0 ? 0 : repo.MarkSent(ids, DateTime.Now);
            audit.Record(AuditActions.NewsletterSent, AuditEntities.Newsletter, null,
                $"{AppLanguages.NativeName(AppLanguages.Normalize(language))}: {stories} stories to {count} subscribers");
        }

        public bool Remove(int subscriberId)
        {
            if (!repo.Delete(subscriberId)) return false;
            audit.Record(AuditActions.SubscriberRemoved, AuditEntities.Newsletter, subscriberId);
            return true;
        }

        public int PurgeStale()
        {
            return repo.DeleteStalePending(DateTime.Now - NewsletterRules.PendingRetention);
        }
    }
}
