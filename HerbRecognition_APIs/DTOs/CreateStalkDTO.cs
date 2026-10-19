using System.ComponentModel.DataAnnotations;

namespace HerbRecognition_APIs.DTOs
{
    public class CreateStalkDTO
    {
        [Required]
        public int StalkShapeId { get; set; }
        [Required]
        public int StalkColorId { get; set; }
        [Required]
        public int StalkSurfaceId { get; set; }
        [Required]
        public bool StalkIsHollow { get; set; }

        [Range(1, 12)]
        public int? StalkMonthOfHarvestId { get; set; }

        [StringLength(200, MinimumLength = 2)]
        public string? StalkMoreInfo { get; set; }
    }
}