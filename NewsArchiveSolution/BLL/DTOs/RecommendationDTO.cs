using System.Text.Json.Serialization;

namespace BLL.DTOs
{
    public class RecommendationDTO
    {
        public ArticleDTO Article { get; set; } = null!;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RecommendationReason Reason { get; set; }

        public int? ReasonTagId { get; set; }

        public double Score { get; set; }

        public TagDTO? ReasonTag => ReasonTagId.HasValue ? Article.Tags.FirstOrDefault(t => t.TagId == ReasonTagId) : null;
    }
}
