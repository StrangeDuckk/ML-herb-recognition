namespace HerbRecognition_APIs.Models
{
    public class PlantSimilarPlant
    {
        public int PlantId { get; set; }

        public Plant Plant { get; set; } = null!;

        public int SimilarPlantId { get; set; }

        public Plant SimilarPlant { get; set; } = null!;
    }
}
