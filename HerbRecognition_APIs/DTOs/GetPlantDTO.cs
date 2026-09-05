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
        public GetPlantTypeDTO PlantTypeDTO { get; set; } = null!;

        // ----- sap -----
        public GetSapDTO? GetSapDTO { get; set; }

        // ----- Root -----
        public GetRootDTO? GetRootDTO { get; set; }

        // ----- stalk -----
        public GetStalkDTO GetStalkDTO { get; set; } = null!;

        // ----- occurance -----
        public GetOccuranceDTO Occurance { get; set; } = null!;

        // ----- hat -----
        public GetHatDTO? GetHatDTO { get; set; }

        // ----- leaf -----
        public GetLeafDTO? GetLeafDTO { get; set; }

        // ----- flower -----
        public GetFlowerDTO? GetFlowerDTO { get; set; }

        // ----- fruit -----
        public GetFruitDTO? GetFruitDTO { get; set; }

        // ----- similarPlants ----- wiele
        public List<GetSimilarPlantDTO>? SimiliarPlantDTO { get; set; }

        // ----- poisonability -----
        public string? PoisonabilityDescription { get; set; }
    }
}
