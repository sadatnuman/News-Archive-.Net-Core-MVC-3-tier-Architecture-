using System.Globalization;
using System.Text;

namespace BLL
{
    public static class SlugHelper
    {
        private const int MaxLength = 80;

        public static string Generate(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "article";

            var normalized = text.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            var lastWasDash = false;

            foreach (var c in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category == UnicodeCategory.NonSpacingMark && c < 'ऀ')
                    continue;

                if (char.IsLetterOrDigit(c) || category == UnicodeCategory.NonSpacingMark || category == UnicodeCategory.SpacingCombiningMark)
                {
                    sb.Append(c);
                    lastWasDash = false;
                }
                else if (!lastWasDash && sb.Length > 0)
                {
                    sb.Append('-');
                    lastWasDash = true;
                }

                if (sb.Length >= MaxLength)
                    break;
            }

            var slug = sb.ToString().Normalize(NormalizationForm.FormC).Trim('-');
            return slug.Length == 0 ? "article" : slug;
        }
    }
}
