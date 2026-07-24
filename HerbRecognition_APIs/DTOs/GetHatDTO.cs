namespace HerbRecognition_APIs.DTOs
{
    public class GetHatDTO
    {
        public string HatColorName { get; set; } = null!;
        public string HatShapeName { get; set; } = null!;
        public string HatThicknessName { get; set; } = null!;
        public string HatSurfaceName { get; set; } = null!;
        public bool HatHasSpots { get; set; }
        public bool HatHasGills { get; set; }
    }
}
