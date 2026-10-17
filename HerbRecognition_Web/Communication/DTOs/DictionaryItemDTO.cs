using System.ComponentModel.DataAnnotations;

namespace HerbRecognition_Web.Communication.DTOs
{
    public class DictionaryItemDTO
    {
        public int Id { get; set; }
        [Required, StringLength(100, MinimumLength = 1)]
        public string? Name { get; set; } = "";
        public int? value { get; set; }

        // tylko dla kolorów
        [Range(0, 255)] public int? R { get; set; }
        [Range(0, 255)] public int? G { get; set; }
        [Range(0, 255)] public int? B { get; set; }
    }
}
