namespace HerbRecognition_Web.Communication.DTOs
{
    public class GetSimilarPlantDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string PolishName { get; set; } = null!;
    }
}