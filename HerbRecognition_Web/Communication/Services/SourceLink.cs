using System.Text.RegularExpressions;

namespace HerbRecognition_Web.Communication.Services
{
    /// <summary>
    /// Źródło przepisu to jedno pole tekstowe: adres strony, autor albo "przepis własny".
    /// Jeśli w tekście jest adres (https://…, http://…, www.… albo coś.pl),
    /// ten fragment staje się linkiem. Reszta tekstu zostaje zwykłym tekstem.
    /// Link może mieć tylko http/https – nic innego (np. javascript:) nie przejdzie.
    /// </summary>
    public static partial class SourceLink
    {
        /// <summary>Tekst przed linkiem, link (pełny adres + to, co wpisał użytkownik) i tekst po linku.</summary>
        public sealed record Parts(string Before, string? Href, string? LinkText, string After);

        // nawiasy i cudzysłowy wokół adresu oraz kropka/przecinek na końcu zdania nie są częścią adresu
        private static readonly char[] LeadingPunctuation = { '(', '[', '"', '„', '\'' };
        private static readonly char[] TrailingPunctuation = { '.', ',', ';', ':', '!', '?', ')', ']', '"', '”', '\'' };

        /// <summary>Dzieli źródło na tekst i (najwyżej jeden) link.</summary>
        public static Parts Split(string? source)
        {
            var text = source?.Trim() ?? "";
            if (text.Length == 0) return new Parts("", null, null, "");

            foreach (Match token in TokenRegex().Matches(text))
            {
                var value = token.Value;
                var lead = value.Length - value.TrimStart(LeadingPunctuation).Length;
                var raw = value[lead..].TrimEnd(TrailingPunctuation);
                if (raw.Length == 0) continue;

                var href = ToHref(raw);
                if (href is null) continue;

                var start = token.Index + lead;
                return new Parts(
                    text[..start],
                    href,
                    raw,
                    text[(start + raw.Length)..]);
            }

            return new Parts(text, null, null, "");
        }

        /// <summary>Pełny adres (https://…) albo null, gdy fragment nie jest adresem.</summary>
        public static string? ToHref(string token)
        {
            string candidate;

            if (token.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                token.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                candidate = token;
            }
            else if (token.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ||
                     PolishDomainRegex().IsMatch(token))
            {
                candidate = "https://" + token;
            }
            else
            {
                return null;
            }

            if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri)) return null;
            if (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp) return null;
            if (!uri.Host.Contains('.')) return null;

            return uri.AbsoluteUri;
        }

        /// <summary>Czy w tekście jest adres, który zostanie pokazany jako link.</summary>
        public static bool HasLink(string? source) => Split(source).Href is not null;

        // ciąg znaków bez spacji
        [GeneratedRegex(@"\S+")]
        private static partial Regex TokenRegex();

        // np. ziola.pl, www.przepisy.pl/syrop, babcia-zosia.blog.pl/napar?x=1
        [GeneratedRegex(@"^[a-z0-9ąćęłńóśźż-]+(\.[a-z0-9ąćęłńóśźż-]+)*\.pl(/\S*)?$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex PolishDomainRegex();
    }
}
