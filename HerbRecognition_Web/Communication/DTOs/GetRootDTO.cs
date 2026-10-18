namespace HerbRecognition_Web.Communication.DTOs
{
    public class GetRootDTO
    {
        public string RootColorName { get; set; } = null!;
        public string RootSurfaceName { get; set; } = null!;
        public string RootThicknessName { get; set; } = null!;
        public bool RootIsHollow { get; set; }
    }
}