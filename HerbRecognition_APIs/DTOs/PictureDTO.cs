namespace HerbRecognition_APIs.DTOs
{
    public class PictureDTO
    {
        public int Id { get; set; }
        public string Url { get; set; } = "";
        public string ThumbnailUrl { get; set; } = "";
        public bool IsMain { get; set; }
        public string? Source { get; set; }
    }
}
