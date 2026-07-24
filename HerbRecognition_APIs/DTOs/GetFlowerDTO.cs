namespace HerbRecognition_APIs.DTOs
{
    public class GetFlowerDTO
    {
        public decimal FlowerSizeInCm { get; set; }
        public string FlowerColorName { get; set; } = null!;
        public string FlowerShapeName { get; set; } = null!;
        public string? FlowerFlavourName { get; set; }
        public int? FlowerScentPower { get; set; }
    }
}
