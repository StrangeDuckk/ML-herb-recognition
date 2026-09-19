namespace HerbRecognition_Web.Communication.DTOs
{
    public class GetPreviewPlantDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PolishName { get; set; } = string.Empty;
        public string LatinName { get; set; } = string.Empty;
        public string? Subriquet { get; set; }

        public GetPlantTypeDTO? PlantTypeDTO { get; set; }

        public string? PoisonabilityDescription { get; set; }
    }
}
