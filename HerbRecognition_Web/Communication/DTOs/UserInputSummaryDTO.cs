namespace HerbRecognition_Web.Communication.DTOs
{
    public class UserInputSummaryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PolishName { get; set; } = "";
        public string? LatinName { get; set; }
        public string? PlantTypeName { get; set; }
        public List<string> Parts { get; set; } = new();
        public int SimilarCount { get; set; }
        public bool IsCorrupted { get; set; }   // JSON nie dał się odczytać – można tylko usunąć
    }
}
