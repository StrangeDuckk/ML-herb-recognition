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
    }
}