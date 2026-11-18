using System.Globalization;
using HerbRecognition_Web.Components.Shared;

namespace HerbRecognition_Web.Communication.Services
{
    /// <summary>Wspólne pomocniki filtrów bibliotek roślin i przepisów.</summary>
    public static class LibraryFilter
    {
        private static readonly CultureInfo Polish = new("pl-PL");
        private static readonly StringComparer PolishOrder = StringComparer.Create(Polish, ignoreCase: true);

        /// <summary>
        /// Opcje listy z wartości kafelków: każda wartość raz, z liczbą kafelków, alfabetycznie.
        /// valuesPerItem – wartości jednego kafelka (np. wszystkie jego choroby).
        /// </summary>
        public static List<FilterSelect.Option> OptionsMany<T>(
            IEnumerable<T> items,
            Func<T, IEnumerable<string?>> valuesPerItem,
            Func<string, int>? order = null)
        {
            var counts = new Dictionary<string, int>(PolishOrder);

            foreach (var item in items)
            {
                // ta sama wartość dwa razy na jednym kafelku liczy się raz
                foreach (var value in valuesPerItem(item)
                             .Where(v => !string.IsNullOrWhiteSpace(v))
                             .Select(v => v!.Trim())
                             .Distinct(PolishOrder))
                {
                    counts[value] = counts.GetValueOrDefault(value) + 1;
                }
            }

            var sorted = order is null
                ? counts.OrderBy(c => c.Key, PolishOrder)
                : counts.OrderBy(c => order(c.Key)).ThenBy(c => c.Key, PolishOrder);

            return sorted.Select(c => new FilterSelect.Option(c.Key, c.Key, c.Value)).ToList();
        }

        /// <summary>Jedna wartość na kafelek (np. typ rośliny).</summary>
        public static List<FilterSelect.Option> Options<T>(
            IEnumerable<T> items,
            Func<T, string?> valuePerItem,
            Func<string, int>? order = null) =>
            OptionsMany(items, x => new[] { valuePerItem(x) }, order);

        /// <summary>Czy wartość pasuje do filtra (null = filtr wyłączony).</summary>
        public static bool Is(string? wanted, string? actual) =>
            wanted is null || (actual is not null && PolishOrder.Equals(wanted, actual.Trim()));

        /// <summary>Czy kafelek ma wartość z filtra (null = filtr wyłączony).</summary>
        public static bool Has(string? wanted, IEnumerable<string?> actual) =>
            wanted is null || actual.Any(a => a is not null && PolishOrder.Equals(wanted, a.Trim()));

        public static bool Same(string a, string b) => PolishOrder.Equals(a, b);

        // ------------------------------------------------------------------
        //  Miesiące zbioru – w kolejności kalendarza, nie alfabetu
        // ------------------------------------------------------------------

        private static readonly string[] MonthStems =
        {
            "stycz", "lut", "marz", "kwie", "maj", "czerw",
            "lip", "sierp", "wrze", "paźdz", "listop", "grud"
        };

        /// <summary>Numer pierwszego miesiąca w nazwie (np. "maj–czerwiec" = 5); bez miesiąca na końcu.</summary>
        public static int MonthOrder(string name)
        {
            var text = name.ToLower(Polish);
            var best = int.MaxValue;
            var bestPosition = int.MaxValue;

            for (var i = 0; i < MonthStems.Length; i++)
            {
                var position = text.IndexOf(MonthStems[i], StringComparison.Ordinal);
                if (position >= 0 && position < bestPosition)
                {
                    bestPosition = position;
                    best = i + 1;
                }
            }

            return best;
        }

        // ------------------------------------------------------------------
        //  Trujące / bezpieczne
        // ------------------------------------------------------------------

        public const string Poisonous = "poisonous";
        public const string Safe = "safe";

        public static readonly IReadOnlyList<FilterSelect.Option> PoisonOptions = new[]
        {
            new FilterSelect.Option(Safe, "Bezpieczne (nietrujące)"),
            new FilterSelect.Option(Poisonous, "Trujące")
        };

        /// <summary>Roślina bez wpisu o trujących właściwościach = nietrująca.
        /// Wpis w stylu "nietrująca" / "bezpieczna" też traktujemy jako bezpieczny.</summary>
        public static bool IsPoisonous(string? poisonabilityDescription)
        {
            if (string.IsNullOrWhiteSpace(poisonabilityDescription)) return false;

            var text = poisonabilityDescription.ToLower(Polish);
            return !(text.Contains("nietruj") || text.Contains("bezpieczn") || text.StartsWith("brak"));
        }

        public static bool MatchesPoison(string? wanted, bool isPoisonous) => wanted switch
        {
            Poisonous => isPoisonous,
            Safe => !isPoisonous,
            _ => true
        };
    }
}
