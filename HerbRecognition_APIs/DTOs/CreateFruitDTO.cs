using System.ComponentModel.DataAnnotations;

namespace HerbRecognition_APIs.DTOs
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
    }
}