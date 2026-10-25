using BLL;
using Microsoft.AspNetCore.Http.Extensions;
using System.Globalization;

namespace NewsArchive.Seo
{
    public static class UiLanguage
    {
        public const string QueryKey = "lang";

        public static string Current => AppLanguages.Normalize(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);

        public static bool IsBangla => Current == AppLanguages.Bangla;

        public static string WithLanguage(string pathAndQuery, string language)
        {
            var separator = pathAndQuery.IndexOf('?');
            var path = separator < 0 ? pathAndQuery : pathAndQuery[..separator];
            var query = separator < 0 ? new QueryBuilder() : new QueryBuilder(
                Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(pathAndQuery[separator..])
                    .Where(p => !p.Key.Equals(QueryKey, StringComparison.OrdinalIgnoreCase))
                    .SelectMany(p => p.Value.Select(v => new KeyValuePair<string, string>(p.Key, v ?? string.Empty))));

            if (language != AppLanguages.English)
                query.Add(QueryKey, language);

            return path + query.ToQueryString();
        }

        public static string SwitchUrl(HttpRequest request, string language)
        {
            var target = WithLanguage(request.Path + request.QueryString, language);
            if (language == AppLanguages.English)
                target += (target.Contains('?') ? "&" : "?") + QueryKey + "=" + AppLanguages.English;
            return target;
        }

        public static string Localize(string path) => WithLanguage(path, Current);
    }
}
