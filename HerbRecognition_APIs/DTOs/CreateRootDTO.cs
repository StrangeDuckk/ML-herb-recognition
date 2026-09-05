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
    }
}