using System.ComponentModel.DataAnnotations;

namespace HerbRecognition_APIs.DTOs
{
    public class CreatePlantDTO
    {
        // ------------ mandatory ------------
        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; } = null!;

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string PolishName { get; set; } = null!;

        [StringLength(50, MinimumLength = 2)]
        public string? LatinName { get; set; }

        [StringLength(50, MinimumLength = 2)]
        public string? Subriquet {  get; set; }

        // ----- type -----
        [Required]
        public int PlantTypeID{ get; set; }

        // ----- sap -----
        public CreateSapDTO? CreateSapDTO { get; set; }

        // ----- Root -----
        public CreateRootDTO? CreateRootDTO { get; set; }

        // ----- stalk -----
        [Required]
        public CreateStalkDTO CreateStalkDTO { get; set; } = null!;

        // ----- occurance -----
        public int OccuranceId { get; set; }

        // ----- hat -----
        public CreateHatDTO? CreateHatDTO { get; set; }

        // ----- leaf -----
        public CreateLeafDTO? CreateLeafDTO { get; set; }

        // ----- flower -----
        public CreateFlowerDTO? CreateFlowerDTO { get; set; }

        // ----- fruit -----
        public CreateFruitDTO? CreateFruitDTO { get; set; }

        // ----- similarPlants ----- wiele 
        public List<int>? SimilarPlantIds { get; set; } //same idiki

        // ----- poisonability -----
        public int? PoisonabilityId { get; set; }
    }
}
