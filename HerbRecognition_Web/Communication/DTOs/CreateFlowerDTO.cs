using System.ComponentModel.DataAnnotations;

namespace HerbRecognition_Web.DTOs
{
    public class CreateFlowerDTO
    {
        [Required]
        public decimal FlowerSizeInCm { get; set; }
        [Required]
        public int FlowerColorId { get; set; }
        [Required]
        public int FlowerShapeId { get; set; }
        [Required]
        public int? FlowerFlavourId { get; set; }
        [Required]
        [Range(0,5)]
        public int FlowerScentPower { get; set; }
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string? FlowerMoreInfo { get; set; }
    }
}