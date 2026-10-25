using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace NewsArchive.Seo
{
    public static class PageMetaExtensions
    {
        private const string Key = "PageMeta";

        public static PageMeta Meta(this ViewDataDictionary viewData)
        {
            if (viewData[Key] is PageMeta meta) return meta;
            meta = new PageMeta();
            viewData[Key] = meta;
            return meta;
        }
    }
}
