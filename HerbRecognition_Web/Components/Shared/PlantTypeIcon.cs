using System.Globalization;

namespace HerbRecognition_Web.Components.Shared
{
    /// <summary>Piktogram typu rośliny – po nazwie typu ze słownika (biblioteka i szczegóły).</summary>
    public static class PlantTypeIcon
    {
        private static readonly CultureInfo Polish = new("pl-PL");

        public static string For(string? typeName)
        {
            var name = typeName?.ToLower(Polish) ?? "";

            if (name.Contains("grzyb")) return "🍄";
            if (name.Contains("drzew")) return "🌳";
            if (name.Contains("zioł") || name.Contains("ziel")) return "🌿";

            return "🌱";
        }
    }
}
 