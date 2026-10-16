namespace HerbRecognition_Web.Communication.DTOs
{
    public sealed class ColorOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }

        public bool IsTransparent =>
            Name.Equals("Transparent", StringComparison.OrdinalIgnoreCase);

        public string CssColor => $"rgb({R}, {G}, {B})";
    }
}
