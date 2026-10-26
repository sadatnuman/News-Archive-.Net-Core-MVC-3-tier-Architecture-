using BLL;
using BLL.Audit;
using BLL.Services;
using DAL.EF;
using DAL.EF.Tables;
using NewsArchive.Tests.Support;

namespace NewsArchive.Tests.Services
{
    public class CommentServiceTests
    {
        private readonly FakeFactory data = new();
        private readonly RecordingAudit audit = new();
        private int reports;
        private bool added = true;

        private CommentService Service(int? actor = 4)
        {
            data.Comments
                .On("Get", a => (int)a[0]! == 10 ? new Comment { CommentId = 10, ArticleId = 1, UserId = 5, Text = "Visible comment" }
                    : (int)a[0]! == 11 ? new Comment { CommentId = 11, ArticleId = 1, UserId = 5, Text = "Hidden", IsHidden = true }
                    : (int)a[0]! == 12 ? new Comment { CommentId = 12, ArticleId = 2, UserId = 5, Text = "On a draft" }
                    : null)
                .On("AddReport", _ => added)
                .On("CountReports", _ => reports)
                .On("SetHidden", _ => true)
                .On("ClearReports", _ => 2)
                .On("Delete", _ => true);
            data.Articles.On("Get", a => (int)a[0]! == 1
                ? new Article { ArticleId = 1, StatusId = ArticleStatusIds.Published, Title = "t", Summary = "s", Content = "c" }
                : new Article { ArticleId = 2, StatusId = ArticleStatusIds.Draft, Title = "t", Summary = "s", Content = "c" });
            return new CommentService(data.Instance, TestServices.Mapper, audit, new FixedActor(actor), TestServices.NoCache);
        }

        [Fact]
        public void Unknown_reason_is_rejected_before_touching_the_database()
        {
            Assert.Equal(ReportResult.InvalidReason, Service().Report(10, 4, 42, null));
            Assert.Equal(0, data.Comments.CountOf("AddReport"));
        }

        [Fact]
        public void Readers_cannot_report_their_own_comment()
        {
            Assert.Equal(ReportResult.OwnComment, Service().Report(10, 5, 1, null));
        }

        [Fact]
        public void Hidden_missing_or_unpublished_comments_cannot_be_reported()
        {
            var service = Service();
            Assert.Equal(ReportResult.NotFound, service.Report(11, 4, 1, null));
            Assert.Equal(ReportResult.NotFound, service.Report(99, 4, 1, null));
            Assert.Equal(ReportResult.NotFound, service.Report(12, 4, 1, null));
        }

        [Fact]
        public void A_second_report_by_the_same_reader_is_refused()
        {
            added = false;
            Assert.Equal(ReportResult.AlreadyReported, Service().Report(10, 4, 1, null));
        }

        [Fact]
        public void First_report_is_stored_with_a_trimmed_note()
        {
            reports = 1;
            Assert.Equal(ReportResult.Reported, Service().Report(10, 4, 1, "  " + new string('n', 350)));
            var call = data.Comments.Calls.Single(c => c.Method == "AddReport");
            Assert.Equal(ModerationRules.MaxNoteLength, ((string)call.Args[3]!).Length);
            Assert.Equal(0, data.Comments.CountOf("SetHidden"));
        }

        [Fact]
        public void Third_report_hides_the_comment_and_is_audited()
        {
            reports = 3;
            Assert.Equal(ReportResult.AutoHidden, Service().Report(10, 4, 2, null));
            var hide = data.Comments.Calls.Single(c => c.Method == "SetHidden");
            Assert.True((bool)hide.Args[1]!);
            Assert.Null(hide.Args[2]);
            Assert.Contains(audit.Entries, e => e.Action == AuditActions.CommentAutoHidden && e.EntityId == 10);
        }

        [Fact]
        public void Hiding_clears_reports_and_records_the_moderator()
        {
            Assert.True(Service(1).Hide(10, 1));
            Assert.Equal(1, data.Comments.CountOf("ClearReports"));
            Assert.Contains(audit.Entries, e => e.Action == AuditActions.CommentHidden && e.ActorId == 1 && e.Details!.StartsWith("2 reports cleared"));
        }

        [Fact]
        public void Deleting_someone_elses_comment_is_audited_but_deleting_your_own_is_not()
        {
            Service(1).Delete(10);
            Assert.Single(audit.Entries, e => e.Action == AuditActions.CommentRemoved);
            audit.Entries.Clear();
            Service(5).Delete(10);
            Assert.Empty(audit.Entries);
        }

        [Theory]
        [InlineData("x")]
        [InlineData("   ")]
        public void Too_short_comments_are_not_saved(string text)
        {
            Assert.False(Service().Create(new BLL.DTOs.CommentDTO { ArticleId = 1, UserId = 4, Text = text, CreatedDate = DateTime.Now }));
            Assert.Equal(0, data.Comments.CountOf("Create"));
        }
    }
}
