using BLL;
using BLL.Services;
using DAL.EF;
using DAL.EF.Tables;
using NewsArchive.Tests.Support;

namespace NewsArchive.Tests.Services
{
    public class RecommendationServiceTests
    {
        private readonly FakeFactory data = new();
        private readonly List<int> read = new();

        private static Article Story(int id, int category, int author, string title, params int[] tags) => new()
        {
            ArticleId = id,
            Title = title,
            Summary = title,
            Content = string.Empty,
            CategoryId = category,
            AuthorId = author,
            LanguageCode = "en",
            StatusId = ArticleStatusIds.Published,
            PublishDate = DateTime.Now.AddDays(-id),
            ViewsCount = 10,
            Tags = tags.Select(t => new Tag { TagId = t, Name = "tag" + t }).ToList()
        };

        private RecommendationService Service(Article source, params Article[] candidates)
        {
            data.Articles
                .On("GetManyWithTags", a => ((IEnumerable<int>)a[0]!).Contains(source.ArticleId) ? new List<Article> { source } : candidates.Where(c => ((IEnumerable<int>)a[0]!).Contains(c.ArticleId)).ToList())
                .On("GetRecommendationCandidates", a => candidates.Where(c => !((IEnumerable<int>)a[1]!).Contains(c.ArticleId)).ToList())
                .On("Get", a => candidates.Concat(new[] { source }).FirstOrDefault(c => c.ArticleId == (int)a[0]!));
            data.History.On("GetRecentArticleIds", _ => read.ToList());
            return new RecommendationService(data.Instance, TestServices.Mapper);
        }

        [Fact]
        public void Stories_sharing_topics_rank_above_unrelated_popular_ones()
        {
            var source = Story(1, 2, 7, "Coastal storm warning stations", 3, 19);
            var related = Story(5, 9, 8, "Port warning flags for storms", 3);
            var unrelated = Story(2, 4, 9, "Startup funding round");
            var result = Service(source, related, unrelated).ForArticle(1, 5);
            Assert.Equal(5, result[0].Article.ArticleId);
            Assert.Equal(RecommendationReason.SharedTopic, result[0].Reason);
            Assert.Equal(3, result[0].ReasonTagId);
            Assert.Equal(RecommendationReason.Popular, result.Single(r => r.Article.ArticleId == 2).Reason);
        }

        [Fact]
        public void The_current_story_is_never_recommended()
        {
            var source = Story(1, 2, 7, "Rail services", 1);
            var result = Service(source, Story(3, 2, 7, "Rail fares", 1)).ForArticle(1, 5);
            Assert.DoesNotContain(result, r => r.Article.ArticleId == 1);
        }

        [Fact]
        public void Stories_already_read_drop_below_unread_ones()
        {
            var source = Story(1, 2, 7, "Rail services evening", 1);
            var readStory = Story(3, 2, 7, "Rail services evening extended", 1);
            var unread = Story(4, 2, 7, "Rail services evening schedule", 1);
            read.Add(3);
            var result = Service(source, readStory, unread).ForArticle(1, 5, userId: 4);
            Assert.Equal(4, result[0].Article.ArticleId);
        }

        [Fact]
        public void Unpublished_source_gets_no_recommendations()
        {
            var source = Story(1, 2, 7, "Draft story", 1);
            source.StatusId = ArticleStatusIds.Draft;
            Assert.Empty(Service(source, Story(2, 2, 7, "Other", 1)).ForArticle(1, 5));
        }

        [Fact]
        public void Reader_profile_needs_enough_history()
        {
            var source = Story(1, 2, 7, "Rail", 1);
            read.Add(1);
            var service = Service(source, Story(2, 2, 7, "Rail fares", 1));
            Assert.Empty(service.ForReader(4, null, "en", 4, minHistory: 2));
            Assert.NotEmpty(service.ForReader(4, null, "en", 4));
        }

        [Fact]
        public void Reading_an_unpublished_story_is_not_recorded()
        {
            var draft = Story(1, 2, 7, "Draft");
            draft.StatusId = ArticleStatusIds.Pending;
            var service = Service(draft);
            Assert.False(service.RecordRead(4, 1));
            Assert.Equal(0, data.History.CountOf("Record"));
        }
    }
}
