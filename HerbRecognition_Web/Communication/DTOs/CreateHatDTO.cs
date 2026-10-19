using System.ComponentModel.DataAnnotations;

namespace HerbRecognition_Web.DTOs
{
    public class CreateHatDTO
    {
        [Required]
        public int HatColorId { get; set; }
        
        [Required]
        public int HatShapeId { get; set; }
        [Required]
        public int HatThicknessId { get; set; }
        [Required]
        public int HatSurfaceId { get; set; }
        [Required]
        public bool HatHasSpots { get; set; }
        [Required]
        public bool HatHasGills { get; set; }

        [Required]
        public bool HatHasRing { get; set; }

        
        [StringLength(200, MinimumLength = 2)]
        public string? HatMoreInfo { get; set; }

        [Range(1, 12)] 
        public int? HatMonthOfHarvestId { get; set; }
    }
}