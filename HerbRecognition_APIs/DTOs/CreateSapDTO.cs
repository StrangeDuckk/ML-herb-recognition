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
    }
}