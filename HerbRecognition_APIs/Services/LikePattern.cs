namespace HerbRecognition_APIs.Services
{
    /// <summary>
    /// Bezpieczne wzorce LIKE / ILIKE.
    /// Parametry EF i FromSqlInterpolated chronią przed SQL injection, ale znaki
    /// % i _ wpisane przez użytkownika nadal działałyby jak symbole wieloznaczne
    /// ("%" = wszystko). Tutaj zamieniamy je na zwykłe znaki.
    /// </summary>
    public static class LikePattern
    {
        public const string EscapeCharacter = "\\";

        public const int MaxSearchLength = 100;

        /// <summary>"50%_x" -> "%50\%\_x%" (szukaj jako fragmentu)</summary>
        public static string Contains(string text) => $"%{Escape(Limit(text))}%";

        public static string Escape(string text) =>
            text.Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_");

        public static string Limit(string text)
        {
            text = text.Trim();
            return text.Length > MaxSearchLength ? text[..MaxSearchLength] : text;
        }
    }
}
