namespace HerbRecognition_Web.Communication.DTOs
{
    public class PictureDTO
    {
        public int Id { get; set; }
        public string Url { get; set; } = "";
        public string ThumbnailUrl { get; set; } = "";
        public bool IsMain { get; set; }
        public string? Source { get; set; }
    }

    public enum PictureOwnerKind { Plant, Submission, Product, ProductSubmission }

    /// <summary>Zdjęcie wybrane w przeglądarce, jeszcze niewysłane do API.</summary>
    public sealed record PhotoUpload(string FileName, byte[] Data, string PreviewUrl);
}
