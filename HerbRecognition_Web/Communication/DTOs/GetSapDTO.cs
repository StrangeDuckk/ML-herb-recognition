namespace HerbRecognition_Web.Communication.DTOs
{
    public class GetSapDTO
    {
        public string SapColor { get; set; } = null!;
        public bool SapLeavesStains { get; set; }
        public bool SapSticky { get; set; }
        public string? SapMoreInfo { get; set; }
    }
}