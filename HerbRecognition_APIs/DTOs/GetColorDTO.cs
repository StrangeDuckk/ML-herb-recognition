namespace HerbRecognition_APIs.DTOs
{
    public class GetColorDTO
    {
        public int ColorId { get; set; }
        public string ColorName { get; set; } = null!;
        public int ColorR { get; set; }
        public int ColorG { get; set; }
        public int ColorB { get; set; }
    }
}
