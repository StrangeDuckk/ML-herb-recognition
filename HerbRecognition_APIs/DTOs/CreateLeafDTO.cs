using System.ComponentModel.DataAnnotations;

namespace HerbRecognition_APIs.DTOs
{
    public class CreateLeafDTO
    {
        [Required]
        public bool LeafStripes { get; set; }
        [Required]
        public bool LeafSpots { get; set; }
        [Required]
        public bool LeafHoles { get; set; }
        [Required]
        public int LeafShapeId { get; set; }
        [Required]
        public int LeafColorId { get; set; }
        [Required]
        public int LeafSurfaceId { get; set; }
        [Required]
        public decimal LeafLength { get; set; }
        [Required]
        public int LeafThicknessId { get; set; }
        [Required]
        public int? LeafFlavourId { get; set; }
    }
}