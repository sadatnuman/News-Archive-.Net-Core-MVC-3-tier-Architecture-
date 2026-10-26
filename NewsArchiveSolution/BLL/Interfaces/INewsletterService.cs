using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface INewsletterService
    {
        SubscribeResultDTO Subscribe(string? email, string language);

        ConfirmResultDTO Confirm(string? token);

        SubscriberDTO? GetByUnsubscribeToken(string? token);

        bool Unsubscribe(string? token);

        List<SubscriberDTO> Get(int page, int pageSize, out int total);

        NewsletterStatsDTO GetStats();

        NewsletterDigestDTO BuildDigest(string language);

        void MarkSent(IEnumerable<int> subscriberIds, string language, int stories);

        bool Remove(int subscriberId);

        int PurgeStale();
    }
}
