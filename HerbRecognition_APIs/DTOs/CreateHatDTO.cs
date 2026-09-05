using System.ComponentModel.DataAnnotations;

namespace HerbRecognition_APIs.DTOs
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
    }
}