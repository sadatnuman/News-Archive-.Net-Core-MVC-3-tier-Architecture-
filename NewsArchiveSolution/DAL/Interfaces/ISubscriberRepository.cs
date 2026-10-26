using DAL.EF.Tables;

namespace DAL.Interfaces
{
    public interface ISubscriberRepository
    {
        Subscriber? GetByEmail(string email);

        Subscriber? GetByConfirmHash(string confirmTokenHash);

        Subscriber? GetByUnsubscribeToken(string token);

        bool Create(Subscriber subscriber);

        bool Save(Subscriber subscriber);

        List<Subscriber> GetActive(string? language);

        List<Subscriber> Get(int page, int pageSize, out int total);

        Dictionary<string, int> CountActiveByLanguage();

        int CountPending();

        int MarkSent(IEnumerable<int> subscriberIds, DateTime sentAt);

        int DeleteStalePending(DateTime createdBefore);

        bool Delete(int subscriberId);
    }
}
