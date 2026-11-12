using System.ComponentModel.DataAnnotations;

namespace HerbRecognition_APIs.DTOs
{
    public class DictionaryItemDTO
    {
        public int Id { get; set; }

        // faktyczny limit zależy od słownika (np. kolor 50, przeciwwskazanie 1000) –
        // sprawdza go DbService na podstawie konfiguracji kolumny
        [Required, StringLength(1000, MinimumLength = 1)]
        public string? Name { get; set; } = "";

        public int? value { get; set; }

        // tylko dla chorób – objawy
        [StringLength(4000)]
        public string? Description { get; set; }

        // tylko dla kolorów
        [Range(0, 255)] public int? R { get; set; }
        [Range(0, 255)] public int? G { get; set; }
        [Range(0, 255)] public int? B { get; set; }
    }
}
