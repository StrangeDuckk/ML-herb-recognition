namespace HerbRecognition_Web.Communication.DTOs
{
    // karta zgłoszenia (niezweryfikowanej rośliny) w bibliotece
    public class PendingPlantPreviewDTO
    {
        public int Id { get; set; }                 // Id zgłoszenia z userinput
        public string Name { get; set; } = "";
        public string PolishName { get; set; } = "";
        public string? LatinName { get; set; }
        public string? Subriquet { get; set; }
        public string? PlantTypeName { get; set; }
        public string? MonthOfHarvestName { get; set; }
    }
}
