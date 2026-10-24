namespace HerbRecognition_APIs.DTOs
{
    public class UserInputDetailsDTO
    {
        public int Id { get; set; }
        public CreatePlantDTO Data { get; set; } = null!;
        public List<SimilarPlantCandidateDTO> SimilarPlants { get; set; } = new();
    }
}
