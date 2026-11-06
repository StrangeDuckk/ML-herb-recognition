namespace HerbRecognition_Web.Communication.Services
{
    /// <summary>
    /// Adresy z bazy wstawiane do src/href. Blazor koduje tekst, ale NIE blokuje
    /// adresów typu javascript: w href – dlatego przepuszczamy tylko nasze pliki
    /// i zwykłe https.
    /// </summary>
    public static class SafeUrl
    {
        private const string LocalPrefix = "/media/pictures/";

        public static string? Image(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;

            if (url.StartsWith(LocalPrefix, StringComparison.Ordinal))
                return url.Contains("..") ? null : url;

            return Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps
                ? url
                : null;
        }
    }
}
