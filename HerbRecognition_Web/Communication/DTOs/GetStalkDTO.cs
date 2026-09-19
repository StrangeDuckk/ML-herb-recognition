namespace HerbRecognition_Web.Communication.DTOs
{
    public class GetStalkDTO
    {
        public string StalkShapeName { get; set; } = null!;
        public string StalkColorName { get; set; } = null!;
        public string StalkSurfaceName { get; set; } = null!;
        public bool StalkIsHollow { get; set; }
    }
}