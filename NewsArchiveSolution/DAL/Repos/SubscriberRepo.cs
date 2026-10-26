using DAL.EF;
using DAL.EF.Tables;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repos
{
    internal sealed class SubscriberRepo : ISubscriberRepository
    {
        private readonly NewsArchiveDbContext db;

        public SubscriberRepo(NewsArchiveDbContext db)
        {
            this.db = db;
        }

        private IQueryable<Subscriber> Active() =>
            db.Subscribers.AsNoTracking().Where(s => s.ConfirmedAt != null && s.UnsubscribedAt == null);

        public Subscriber? GetByEmail(string email)
        {
            return db.Subscribers.AsNoTracking().FirstOrDefault(s => s.Email == email);
        }

        public Subscriber? GetByConfirmHash(string confirmTokenHash)
        {
            return db.Subscribers.AsNoTracking().FirstOrDefault(s => s.ConfirmTokenHash == confirmTokenHash);
        }

        public Subscriber? GetByUnsubscribeToken(string token)
        {
            return db.Subscribers.AsNoTracking().FirstOrDefault(s => s.UnsubscribeToken == token);
        }

        public bool Create(Subscriber subscriber)
        {
            db.Subscribers.Add(subscriber);
            try
            {
                var saved = db.SaveChanges() > 0;
                db.Entry(subscriber).State = EntityState.Detached;
                return saved;
            }
            catch (DbUpdateException)
            {
                db.ChangeTracker.Clear();
                return false;
            }
        }

        public bool Save(Subscriber subscriber)
        {
            return db.Subscribers
                .Where(s => s.SubscriberId == subscriber.SubscriberId)
                .ExecuteUpdate(u => u
                    .SetProperty(s => s.LanguageCode, subscriber.LanguageCode)
                    .SetProperty(s => s.ConfirmTokenHash, subscriber.ConfirmTokenHash)
                    .SetProperty(s => s.UnsubscribeToken, subscriber.UnsubscribeToken)
                    .SetProperty(s => s.CreatedAt, subscriber.CreatedAt)
                    .SetProperty(s => s.ConfirmedAt, subscriber.ConfirmedAt)
                    .SetProperty(s => s.UnsubscribedAt, subscriber.UnsubscribedAt)) > 0;
        }

        public List<Subscriber> GetActive(string? language)
        {
            var query = Active();
            if (!string.IsNullOrEmpty(language))
                query = query.Where(s => s.LanguageCode == language);
            return query.OrderBy(s => s.SubscriberId).ToList();
        }

        public List<Subscriber> Get(int page, int pageSize, out int total)
        {
            var query = db.Subscribers.AsNoTracking();
            total = query.Count();
            return query.OrderByDescending(s => s.CreatedAt)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
        }

        public Dictionary<string, int> CountActiveByLanguage()
        {
            return Active()
                .GroupBy(s => s.LanguageCode)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionary(x => x.Key, x => x.Count);
        }

        public int CountPending()
        {
            return db.Subscribers.Count(s => s.ConfirmedAt == null && s.UnsubscribedAt == null);
        }

        public int MarkSent(IEnumerable<int> subscriberIds, DateTime sentAt)
        {
            var ids = subscriberIds.Distinct().ToList();
            return db.Subscribers
                .Where(s => ids.Contains(s.SubscriberId))
                .ExecuteUpdate(u => u.SetProperty(s => s.LastSentAt, sentAt));
        }

        public int DeleteStalePending(DateTime createdBefore)
        {
            return db.Subscribers
                .Where(s => s.ConfirmedAt == null && s.CreatedAt < createdBefore)
                .ExecuteDelete();
        }

        public bool Delete(int subscriberId)
        {
            return db.Subscribers.Where(s => s.SubscriberId == subscriberId).ExecuteDelete() > 0;
        }
    }
}
