using BLL.Audit;
using BLL.Services;
using DAL.EF.Tables;
using NewsArchive.Tests.Support;

namespace NewsArchive.Tests.Services
{
    public class AuditServiceTests
    {
        private readonly FakeFactory data = new();
        private readonly List<AuditLog> saved = new();

        private AuditService Service(int? actor, string? ip = "203.0.113.7")
        {
            data.AuditLogs.On("Add", a =>
            {
                saved.Add((AuditLog)a[0]!);
                return null;
            });
            return new AuditService(data.Instance, new FixedActor(actor, ip));
        }

        [Fact]
        public void Entries_take_the_actor_and_ip_from_the_request()
        {
            Service(1).Record(AuditActions.TagCreated, AuditEntities.Tag, 20, "Agriculture");
            var entry = Assert.Single(saved);
            Assert.Equal(1, entry.UserId);
            Assert.Equal("203.0.113.7", entry.IpAddress);
            Assert.Equal("Agriculture", entry.Details);
        }

        [Fact]
        public void An_explicit_actor_wins_over_the_request()
        {
            Service(null).Record(AuditActions.SignedIn, AuditEntities.Security, 5, null, actorId: 5);
            Assert.Equal(5, saved[0].UserId);
        }

        [Fact]
        public void Long_values_are_cut_to_fit_the_columns()
        {
            Service(1, new string('9', 80)).Record(new string('A', 90), new string('B', 90), null, new string('d', 900));
            var entry = saved[0];
            Assert.Equal(50, entry.Action.Length);
            Assert.Equal(30, entry.EntityType.Length);
            Assert.Equal(AuditRules.MaxDetailsLength, entry.Details!.Length);
            Assert.Equal(45, entry.IpAddress!.Length);
        }

        [Fact]
        public void Unknown_filter_types_fall_back_to_all()
        {
            string? passed = "unset";
            data.AuditLogs.On("Get", a =>
            {
                passed = (string?)a[0];
                a[4] = 0;
                return new List<AuditLog>();
            });
            new AuditService(data.Instance, new FixedActor(1)).Get("<script>", null, 1, 30, out _);
            Assert.Null(passed);
            new AuditService(data.Instance, new FixedActor(1)).Get("comment", null, 1, 30, out _);
            Assert.Equal(AuditEntities.Comment, passed);
        }
    }
}
