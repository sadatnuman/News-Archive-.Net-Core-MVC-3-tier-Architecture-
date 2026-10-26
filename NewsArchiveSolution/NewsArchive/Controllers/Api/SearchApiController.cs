using BLL;
using BLL.DTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NewsArchive.Seo;

namespace NewsArchive.Controllers.Api
{
    [Route("api/search")]
    public class SearchApiController : ApiControllerBase
    {
        private readonly ISearchService searchService;
        private readonly ICategoryService categoryService;

        public SearchApiController(ISearchService searchService, ICategoryService categoryService)
        {
            this.searchService = searchService;
            this.categoryService = categoryService;
        }

        public record SuggestionItem(string Type, string Title, string Url, string? Meta, string? Image, string? Lang);

        public record SuggestionResponse(string Query, string SearchUrl, List<SuggestionItem> Items);

        [HttpGet("suggest")]
        [ProducesResponseType(typeof(SuggestionResponse), StatusCodes.Status200OK)]
        public IActionResult Suggest([FromQuery] string? q, [FromQuery] string? lang, [FromQuery] int count = 6)
        {
            var language = ParseLanguage(lang) ?? UiLanguage.Current;
            var result = searchService.Suggest(q, language, count);
            var categories = categoryService.Get().ToDictionary(c => c.CategoryId, c => c.LocalName(language));

            var items = new List<SuggestionItem>();
            items.AddRange(result.Stories.Select(a => new SuggestionItem("story", a.Title, SeoUrls.Article(a),
                categories.TryGetValue(a.CategoryId, out var section) ? section : null, SeoUrls.Image(a.ImagePath), a.LanguageCode)));
            items.AddRange(result.Topics.Select(t => new SuggestionItem("topic", t.LocalName(language), SeoUrls.Tag(t.TagId, t.Slug, language), null, null, language)));
            items.AddRange(result.Sections.Select(c => new SuggestionItem("section", c.LocalName(language), SeoUrls.Category(c.CategoryId, c.Slug, language), null, null, language)));
            items.AddRange(result.Authors.Select(a => new SuggestionItem("author", a.Name, SeoUrls.Author(a.UserId, a.Name, language), null, SeoUrls.Image(a.AvatarPath), null)));

            var searchUrl = SeoUrls.Page("/search?keyword=" + Uri.EscapeDataString(result.Query), language);
            return Ok(new SuggestionResponse(result.Query, searchUrl, items));
        }
    }
}
