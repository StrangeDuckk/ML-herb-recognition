namespace HerbRecognition_Web.Communication.Services
{
    public class WikimediaSearchResponse
    {
        public WikimediaQuery? Query { get; set; }
    }

    public class WikimediaQuery
    {
        public Dictionary<string, WikimediaPage>? Pages { get; set; }
    }

    public class WikimediaPage
    {
        public List<WikimediaImageInfo>? ImageInfo { get; set; }
    }

    public class WikimediaImageInfo
    {
        public string? Url { get; set; }
        public string? ThumbUrl { get; set; }
    }
}