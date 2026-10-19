using System.ComponentModel.DataAnnotations;

namespace HerbRecognition_Web.DTOs
{
    public class CreateSapDTO
    {
        [Required]
        public int SapColorId { get; set; }

        [Required]
        public bool SapLeavesStain { get; set; }
        [Required]
        public bool SapSticky {  get; set; }
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string? SapMoreInfo { get; set; }
    }
}