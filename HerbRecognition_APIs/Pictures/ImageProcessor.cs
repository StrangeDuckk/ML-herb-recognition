using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace HerbRecognition_APIs.Pictures;

public record ProcessedImage(byte[] Main, byte[] Thumbnail, int Width, int Height);

public class InvalidImageException(string message) : Exception(message);

public class ImageProcessor
{
    // 1536 px na dłuższym boku: zapas do kadrowania dla wizji maszynowej
    private const int MainMaxSide = 1536;
    private const int ThumbMaxSide = 400;
    private const long MaxPixels = 50_000_000;   // ochrona przed "bombami" dekompresji

    private static readonly WebpEncoder MainEncoder = new()
    {
        FileFormat = WebpFileFormatType.Lossy,
        Quality = 80,
    };

    private static readonly WebpEncoder ThumbEncoder = new()
    {
        FileFormat = WebpFileFormatType.Lossy,
        Quality = 70,
    };

    public async Task<ProcessedImage> ProcessAsync(Stream input, CancellationToken ct)
    {
        try
        {
            var info = await Image.IdentifyAsync(input, ct);
            if ((long)info.Width * info.Height > MaxPixels)
                throw new InvalidImageException("zdjęcie ma zbyt dużą rozdzielczość.");

            input.Position = 0;
            using var image = await Image.LoadAsync(new DecoderOptions { MaxFrames = 1 }, input, ct);

            // obrót wg EXIF, potem usunięcie metadanych (m.in. GPS)
            image.Mutate(x => x.AutoOrient());
            image.Metadata.ExifProfile = null;
            image.Metadata.XmpProfile = null;
            image.Metadata.IptcProfile = null;

            if (Math.Max(image.Width, image.Height) > MainMaxSide)
                image.Mutate(x => Fit(x, MainMaxSide));

            using var mainStream = new MemoryStream();
            await image.SaveAsWebpAsync(mainStream, MainEncoder, ct);

            using var thumb = Math.Max(image.Width, image.Height) > ThumbMaxSide
                ? image.Clone(x => Fit(x, ThumbMaxSide))
                : image.Clone(_ => { });

            using var thumbStream = new MemoryStream();
            await thumb.SaveAsWebpAsync(thumbStream, ThumbEncoder, ct);

            return new ProcessedImage(mainStream.ToArray(), thumbStream.ToArray(), image.Width, image.Height);
        }
        catch (Exception ex) when (ex is UnknownImageFormatException or InvalidImageContentException)
        {
            throw new InvalidImageException("plik nie jest obsługiwanym zdjęciem (JPEG, PNG, WebP).");
        }
    }

    private static void Fit(IImageProcessingContext ctx, int maxSide) =>
        ctx.Resize(new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new Size(maxSide, maxSide),
        });
}
