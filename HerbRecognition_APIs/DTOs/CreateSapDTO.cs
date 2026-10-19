using System.ComponentModel.DataAnnotations;

namespace HerbRecognition_APIs.DTOs
{
    public class CreateSapDTO
    {
        [Required]
        public int SapColorId { get; set; }

        [Required]
        public bool SapLeavesStain { get; set; }
        [Required]
        public bool SapSticky {  get; set; }

        [Range(1, 12)]
        public int? SapMonthOfHarvestId { get; set; }

        [StringLength(200, MinimumLength = 2)]
        public string? SapMoreInfo { get; set; }
    }
}