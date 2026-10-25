namespace HerbRecognition_APIs.DTOs
{
    public class PlantSearchResponseDTO
    {
        /// <summary>Ile cech podał użytkownik. 0 = formularz był pusty.</summary>
        public int CriteriaCount { get; set; }
        public int MinMatchPercent { get; set; }
        public int CheckedCount { get; set; }      // ile roślin i zgłoszeń porównano
        public List<PlantSearchResultDTO> Results { get; set; } = new();
    }

    public class PlantSearchResultDTO
    {
        public int Id { get; set; }                 // Id rośliny albo Id zgłoszenia
        public bool IsPending { get; set; }         // true = zgłoszenie z userinput
        public string Name { get; set; } = "";
        public string PolishName { get; set; } = "";
        public string? LatinName { get; set; }
        public string? Subriquet { get; set; }
        public string? PlantTypeName { get; set; }
        public string? MonthOfHarvestName { get; set; }

        public int MatchPercent { get; set; }
        public int MatchedCount { get; set; }       // ile podanych cech się zgadza
        public int ComparedCount { get; set; }      // ile cech porównano
    }
}
