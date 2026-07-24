namespace HerbRecognition_APIs.DTOs
{
    public class GetLeafDTO
    {
        public bool LeafHasStripes { get; set; }
        public bool LeafHasSpots { get; set; }
        public bool LeafHasHoles { get; set; }
        public string LeafShapeName { get; set; } = null!;
        public string LeafColorName { get; set; } = null!;
        public string LeafSurfaceName { get; set; } = null!;
        public decimal LeafLength { get; set; }
        public string LeafThicknessName { get; set; } = null!;
        public string LeafFlavourName { get; set; } = null!;
    }
}
