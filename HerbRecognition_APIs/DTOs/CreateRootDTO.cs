using System.ComponentModel.DataAnnotations;

namespace HerbRecognition_APIs.DTOs
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
        public bool RootIsHollow { get; set; }
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string? RootMoreInfo { get; set; }
    }
}