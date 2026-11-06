using SkiaSharp;

namespace HerbRecognition_APIs.Pictures;

public record ProcessedImage(byte[] Main, byte[] Thumbnail, int Width, int Height);

public class InvalidImageException(string message) : Exception(message);

public class ImageProcessor
{
    // 1536 px na dłuższym boku: zapas do kadrowania dla modeli wizyjnych
    // (typowe wejście 224–518 px) i ostry obraz na ekranach HiDPI.
    private const int MainMaxSide = 1536;
    private const int ThumbMaxSide = 400;
    private const int MainQuality = 80;
    private const int ThumbQuality = 70;
    private const long MaxPixels = 50_000_000;   // ochrona przed "bombami" dekompresji

    // mipmapy = gładkie zmniejszanie także przy dużej skali (np. 1536 -> 400)
    private static readonly SKSamplingOptions Sampling = new(SKFilterMode.Linear, SKMipmapMode.Linear);

    public async Task<ProcessedImage> ProcessAsync(Stream input, CancellationToken ct)
    {
        // SKCodec czyta synchronicznie – kopiujemy do pamięci (pliki mają najwyżej kilka MB)
        using var buffer = new MemoryStream();
        await input.CopyToAsync(buffer, ct);
        buffer.Position = 0;

        using var codec = SKCodec.Create(buffer)
            ?? throw new InvalidImageException("plik nie jest obsługiwanym zdjęciem (JPEG, PNG, WebP).");

        if ((long)codec.Info.Width * codec.Info.Height > MaxPixels)
            throw new InvalidImageException("zdjęcie ma zbyt dużą rozdzielczość.");

        using var decoded = SKBitmap.Decode(codec)
            ?? throw new InvalidImageException("nie udało się odczytać zdjęcia.");

        // Każdy krok zwraca null, gdy nic nie zmienia – wtedy używamy poprzedniej bitmapy.
        // Ponowne kodowanie do WebP usuwa wszystkie metadane (EXIF, GPS).
        using var rotated = ApplyOrientation(decoded, codec.EncodedOrigin);
        var oriented = rotated ?? decoded;

        using var mainResized = ResizeToFit(oriented, MainMaxSide);
        var main = mainResized ?? oriented;

        using var thumbResized = ResizeToFit(main, ThumbMaxSide);
        var thumb = thumbResized ?? main;

        return new ProcessedImage(
            EncodeWebp(main, MainQuality),
            EncodeWebp(thumb, ThumbQuality),
            main.Width,
            main.Height);
    }

    private static SKBitmap? ResizeToFit(SKBitmap source, int maxSide)
    {
        var longest = Math.Max(source.Width, source.Height);
        if (longest <= maxSide) return null;

        var scale = (double)maxSide / longest;
        var width = Math.Max(1, (int)Math.Round(source.Width * scale));
        var height = Math.Max(1, (int)Math.Round(source.Height * scale));

        var target = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
        using var canvas = new SKCanvas(target);
        using var image = SKImage.FromBitmap(source);

        canvas.DrawImage(image, new SKRect(0, 0, width, height), Sampling);
        return target;
    }

    /// <summary>Obraca/odbija zdjęcie zgodnie z orientacją EXIF. Null = orientacja już poprawna.</summary>
    private static SKBitmap? ApplyOrientation(SKBitmap source, SKEncodedOrigin origin)
    {
        if (origin == SKEncodedOrigin.TopLeft) return null;

        var swapSides = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop
                               or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;

        var width = swapSides ? source.Height : source.Width;
        var height = swapSides ? source.Width : source.Height;

        var target = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
        using var canvas = new SKCanvas(target);

        switch (origin)
        {
            case SKEncodedOrigin.TopRight:       // odbicie w poziomie
                canvas.Translate(width, 0);
                canvas.Scale(-1, 1);
                break;
            case SKEncodedOrigin.BottomRight:    // 180°
                canvas.Translate(width, height);
                canvas.RotateDegrees(180);
                break;
            case SKEncodedOrigin.BottomLeft:     // odbicie w pionie
                canvas.Translate(0, height);
                canvas.Scale(1, -1);
                break;
            case SKEncodedOrigin.LeftTop:        // transpozycja
                canvas.RotateDegrees(90);
                canvas.Scale(1, -1);
                break;
            case SKEncodedOrigin.RightTop:       // 90° w prawo (typowe zdjęcie z telefonu w pionie)
                canvas.Translate(width, 0);
                canvas.RotateDegrees(90);
                break;
            case SKEncodedOrigin.RightBottom:    // transwersja
                canvas.Translate(width, height);
                canvas.RotateDegrees(-90);
                canvas.Scale(1, -1);
                break;
            case SKEncodedOrigin.LeftBottom:     // 90° w lewo
                canvas.Translate(0, height);
                canvas.RotateDegrees(-90);
                break;
        }

        canvas.DrawBitmap(source, 0, 0);
        return target;
    }

    private static byte[] EncodeWebp(SKBitmap bitmap, int quality)
    {
        using var data = bitmap.Encode(SKEncodedImageFormat.Webp, quality)
            ?? throw new InvalidImageException("nie udało się zapisać zdjęcia.");

        return data.ToArray();
    }
}
