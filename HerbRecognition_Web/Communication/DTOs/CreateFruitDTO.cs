using System.ComponentModel.DataAnnotations;

namespace HerbRecognition_Web.DTOs
{
    public class CreateFruitDTO
    {
        [Required]
        public int? FruitFlavourId { get; set; }
        [Required]
        public int FruitShapeId { get; set; }
        [Required]
        public int FruitColorId { get; set; }
        [Required]
        public int FruitSurfaceId { get; set; }
        [Required]
        public int FruitThicknessId { get; set; }
        
        [StringLength(200, MinimumLength = 2)]
        public string? FruitMoreInfo { get; set; }

        [Range(1, 12)] 
        public int? FruitMonthOfHarvestId { get; set; }
    }
}