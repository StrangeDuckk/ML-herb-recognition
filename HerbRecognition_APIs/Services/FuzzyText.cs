using System.Globalization;
using System.Text;

namespace HerbRecognition_APIs.Services
{
    /// <summary>
    /// Porównywanie tekstu odporne na wielkość liter, polskie znaki i literówki.
    /// Używane przez zwykłą wyszukiwarkę i wyszukiwanie przez formularz.
    /// </summary>
    public static class FuzzyText
    {
        private static readonly char[] Separators =
            { ' ', '-', ',', '.', ';', ':', '/', '(', ')', '\'', '"', '„', '”', '\t', '\n' };

        /// <summary>"Liść Pokrzywy" -> "lisc pokrzywy"</summary>
        public static string Normalize(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";

            if (text.Length > 500) text = text[..500]; //obciecie do 500 znakow

            // ł nie rozkłada się w Unicode na l + znak diakrytyczny
            var lower = text.Trim().ToLowerInvariant().Replace('ł', 'l');
            var decomposed = lower.Normalize(NormalizationForm.FormD);

            var sb = new StringBuilder(decomposed.Length);
            foreach (var ch in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Czy tekst pasuje do wyszukiwanej frazy. Każde słowo frazy musi pasować
        /// do któregoś słowa tekstu (dokładnie, jako fragment albo z literówką).
        /// Dla nazw: "sosan" ~ "Sosna zwyczajna", "pokrzywa" ~ "Pokrzywa zwyczajna".
        /// </summary>
        public static bool IsSimilar(string? text, string? search)
        {
            var t = Normalize(text);
            var q = Normalize(search);
            if (t.Length == 0 || q.Length == 0) return false;

            if (t.Contains(q)) return true;

            var textWords = Words(t);
            var queryWords = Words(q);
            if (queryWords.Length == 0) return false;

            return queryWords.All(qw => textWords.Any(tw => WordMatches(tw, qw)));
        }

        /// <summary>
        /// Luźniejsze porównanie dla opisów: wystarczy, że pasuje co najmniej
        /// połowa (minShare) znaczących słów frazy.
        /// </summary>
        public static bool SharesWords(string? text, string? search, double minShare = 0.5)
        {
            var t = Normalize(text);
            var q = Normalize(search);
            if (t.Length == 0 || q.Length == 0) return false;

            if (t.Contains(q)) return true;

            var textWords = Words(t);
            var queryWords = Words(q).Where(w => w.Length >= 3).ToArray();
            if (queryWords.Length == 0) return false;

            var found = queryWords.Count(qw => textWords.Any(tw => WordMatches(tw, qw)));
            return found >= Math.Ceiling(queryWords.Length * minShare);
        }

        /// <summary>Ile literówek dopuszczamy – zależnie od długości słowa.</summary>
        public static int AllowedTypos(int length) => length switch
        {
            <= 3 => 0,   // "lis" nie może pasować do "las"
            <= 6 => 1,
            _ => 2
        };

        private static string[] Words(string normalized) =>
            normalized.Split(Separators, StringSplitOptions.RemoveEmptyEntries);

        private static bool WordMatches(string textWord, string queryWord)
        {
            // fragment słowa: "sosn" w "sosna"
            if (textWord.Contains(queryWord)) return true;

            var allowed = AllowedTypos(queryWord.Length);
            if (allowed == 0) return false;

            // całe słowo z literówką: "sosan" ~ "sosna"
            if (Math.Abs(textWord.Length - queryWord.Length) <= allowed &&
                Distance(textWord, queryWord, allowed) <= allowed)
                return true;

            // początek dłuższego słowa z literówką: "pokszyw" ~ "pokrzywa"
            if (textWord.Length > queryWord.Length &&
                Distance(textWord[..queryWord.Length], queryWord, allowed) <= allowed)
                return true;

            return false;
        }

        /// <summary>
        /// Odległość edycyjna (Damerau–Levenshtein, wariant OSA): dodanie, usunięcie,
        /// zamiana znaku oraz zamiana dwóch sąsiednich liter liczą się jako 1.
        /// Przerywa liczenie, gdy wynik na pewno przekroczy max.
        /// </summary>
        public static int Distance(string a, string b, int max = int.MaxValue)
        {
            if (Math.Abs(a.Length - b.Length) > max) return max == int.MaxValue ? max : max + 1;

            var beforePrev = new int[b.Length + 1];
            var prev = new int[b.Length + 1];
            var curr = new int[b.Length + 1];

            for (var j = 0; j <= b.Length; j++)
                prev[j] = j;

            for (var i = 1; i <= a.Length; i++)
            {
                curr[0] = i;
                var rowMin = curr[0];

                for (var j = 1; j <= b.Length; j++)
                {
                    var cost = a[i - 1] == b[j - 1] ? 0 : 1;

                    curr[j] = Math.Min(
                        Math.Min(prev[j] + 1,        // usunięcie
                                 curr[j - 1] + 1),   // dodanie
                        prev[j - 1] + cost);         // zamiana

                    // przestawione sąsiednie litery: "sosan" <-> "sosna"
                    if (i > 1 && j > 1 && a[i - 1] == b[j - 2] && a[i - 2] == b[j - 1])
                        curr[j] = Math.Min(curr[j], beforePrev[j - 2] + 1);

                    rowMin = Math.Min(rowMin, curr[j]);
                }

                if (max != int.MaxValue && rowMin > max) return max + 1;

                (beforePrev, prev, curr) = (prev, curr, beforePrev);
            }

            return prev[b.Length];
        }
    }
}
