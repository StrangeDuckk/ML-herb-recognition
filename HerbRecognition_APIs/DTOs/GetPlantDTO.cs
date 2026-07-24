namespace HerbRecognition_APIs.DTOs
{
    public class GetPlantDTO
    {
        // ----- mandatory -----
        public string Name { get; set; } = null!;
        public string PolishName { get; set; } = null!;
        public string? LatinName { get; set; }
        public string? Subriquet { get; set; }

        // ----- type -----
        public string PlantTypeName { get; set; } = null!;

        // ----- sap -----
        public GetSapDTO? GetSapDTO { get; set; }

        // ----- Root -----
        public GetRootDTO? GetRootDTO { get; set; }

        // ----- stalk -----
        public GetStalkDTO GetStalkDTO { get; set; } = null!;

        // ----- occurance -----
        public string Occurance { get; set; } = null!;

        // ----- hat -----
        public GetHatDTO? GetHatDTO { get; set; }

        // ----- leaf -----
        public GetLeafDTO? GetLeafDTO { get; set; }

        // ----- flower -----
        public GetFlowerDTO? GetFlowerDTO { get; set; }

        // ----- fruit -----
        public GetFruitDTO? GetFruitDTO { get; set; }

        // ----- similarPlants ----- wiele
        public List<SimilarPlantDTO>? SimiliarPlantDTO { get; set; }

        // ----- poisonability -----
        public string? PoisonabilityDescription { get; set; }
    }
}
