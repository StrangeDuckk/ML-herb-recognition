namespace HerbRecognition_Web.Components.Shared
{
    /// <summary>Piktogram typu produktu – po nazwie ze słownika (polskiej albo angielskiej).</summary>
    public static class ProductTypeIcon
    {
        public static string For(string? typeName)
        {
            var name = typeName?.ToLowerInvariant() ?? "";

            if (name.Contains("syrup") || name.Contains("syrop")) return "🍯";
            if (name.Contains("tea") || name.Contains("herbat") || name.Contains("infusion") || name.Contains("napar")) return "☕";
            if (name.Contains("decoction") || name.Contains("odwar")) return "♨";
            if (name.Contains("tincture") || name.Contains("nalewka")) return "🧪";
            if (name.Contains("ointment") || name.Contains("maść") || name.Contains("masc")) return "🍵";
            if (name.Contains("extract") || name.Contains("wyciąg") || name.Contains("ekstrakt")) return "💧";
            if (name.Contains("powder") || name.Contains("proszek")) return "🥄";
            if (name.Contains("juice") || name.Contains("sok")) return "🥤";
            if (name.Contains("compress") || name.Contains("okład") || name.Contains("oklad")) return "🩹";

            return "🥣";
        }
    }
}
