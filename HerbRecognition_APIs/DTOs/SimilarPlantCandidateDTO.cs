namespace HerbRecognition_APIs.DTOs
{
    public class SimilarPlantCandidateDTO
    {
        public int Id { get; set; }                 // Id rośliny albo Id zgłoszenia
        public string Name { get; set; } = "";
        public string PolishName { get; set; } = "";
        public string? LatinName { get; set; }
        public string? PlantTypeName { get; set; }
        public bool IsApproved { get; set; }        // true = plant, false = userinput
    }
}
