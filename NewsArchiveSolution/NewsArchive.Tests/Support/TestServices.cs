using AutoMapper;
using BLL.Audit;
using BLL.Caching;
using BLL.Interfaces;
using BLL.Mapping;
using DAL.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace NewsArchive.Tests.Support
{
    public static class TestServices
    {
        private static readonly Lazy<IMapper> SharedMapper = new(() =>
            new ServiceCollection().AddLogging().AddAutoMapper(typeof(MappingProfile)).BuildServiceProvider().GetRequiredService<IMapper>());

        public static IMapper Mapper => SharedMapper.Value;

        public static IAppCache NoCache { get; } = new NoCache();
    }

    public class FixedActor : IActorContext
    {
        public FixedActor(int? userId, string? ip = "203.0.113.7")
        {
            UserId = userId;
            IpAddress = ip;
        }

        public int? UserId { get; }

        public string? IpAddress { get; }
    }

    public class RecordingAudit : IAuditService
    {
        public List<(string Action, string EntityType, int? EntityId, string? Details, int? ActorId)> Entries { get; } = new();

        public void Record(string action, string entityType, int? entityId = null, string? details = null, int? actorId = null)
        {
            Entries.Add((action, entityType, entityId, details, actorId));
        }

        public List<BLL.DTOs.AuditEntryDTO> Get(string? entityType, int? userId, int page, int pageSize, out int total)
        {
            total = 0;
            return new();
        }

        public int PurgeOld() => 0;
    }

    public class FakeFactory
    {
        public Fake<IArticleRepository> Articles { get; }
        public Fake<ICommentRepository> Comments { get; }
        public Fake<ISubscriberRepository> Subscribers { get; }
        public Fake<IReadingHistoryRepository> History { get; }
        public Fake<IAuditLogRepository> AuditLogs { get; }
        public Fake<ITagRepository> Tags { get; }
        public IDataAccessFactory Instance { get; }

        public FakeFactory()
        {
            var (articles, a) = Fake<IArticleRepository>.Create();
            var (comments, c) = Fake<ICommentRepository>.Create();
            var (subscribers, s) = Fake<ISubscriberRepository>.Create();
            var (history, h) = Fake<IReadingHistoryRepository>.Create();
            var (audit, l) = Fake<IAuditLogRepository>.Create();
            var (tags, t) = Fake<ITagRepository>.Create();
            Articles = a;
            Comments = c;
            Subscribers = s;
            History = h;
            AuditLogs = l;
            Tags = t;

            var (factory, f) = Fake<IDataAccessFactory>.Create();
            f.On(nameof(IDataAccessFactory.GetArticleRepository), _ => articles)
             .On(nameof(IDataAccessFactory.GetCommentRepository), _ => comments)
             .On(nameof(IDataAccessFactory.GetSubscriberRepository), _ => subscribers)
             .On(nameof(IDataAccessFactory.GetReadingHistoryRepository), _ => history)
             .On(nameof(IDataAccessFactory.GetAuditLogRepository), _ => audit)
             .On(nameof(IDataAccessFactory.GetTagRepository), _ => tags);
            Instance = factory;
        }
    }
}
