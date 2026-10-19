using System.ComponentModel.DataAnnotations;

namespace HerbRecognition_Web.DTOs
{
    public class CreateRootDTO
    {
        [Required]
        public int RootColorId { get; set; }
        [Required]
        public int RootSurfaceId { get; set; }
        [Required]
        public int RootThicknessId { get; set; }
        [Required]
        public bool RootIsHollow {  get; set; }
        
        [StringLength(200, MinimumLength = 2)]
        public string? RootMoreInfo { get; set; }

        [Range(1, 12)]
        public int? RootMonthOfHarvestId { get; set; }
    }
}