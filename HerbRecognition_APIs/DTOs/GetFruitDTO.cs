namespace HerbRecognition_APIs.DTOs
{
    public class GetFruitDTO
    {
        public string? FruitFlavourName { get; set; }
        public string FruitShapeName { get; set; } = null!;
        public string FruitColorName { get; set; } = null!;
        public string FruitSurfaceName { get; set; } = null!;
        public string FruitThicnkessName { get; set; } = null!;
    }
}
