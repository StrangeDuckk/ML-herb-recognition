using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace HerbRecognition_APIs.Pictures;

public class PictureStorageOptions
{
    /// <summary>Folder na pliki. Lokalnie względny, na serwerze wolumen, np. /app/data/pictures.</summary>
    public string RootPath { get; set; } = "App_Data/pictures";

    /// <summary>Adres, pod którym strona (Web) serwuje pliki przeglądarce.</summary>
    public string PublicBaseUrl { get; set; } = "/media/pictures";
}

public interface IPictureFileStore
{
    /// <summary>Zapisuje zdjęcie i miniaturę. Zwraca wartość do kolumny picturelink.</summary>
    Task<string> SaveAsync(ProcessedImage image, CancellationToken ct);

    /// <summary>Usuwa plik i miniaturę. Zewnętrzne linki (http...) są pomijane.</summary>
    void Delete(string pictureLink);

    /// <summary>Miniatura dla linku. Dla zewnętrznych linków – ten sam link.</summary>
    string ThumbnailLink(string pictureLink);

    /// <summary>Ścieżka pliku do wysłania. Null, gdy nazwa jest nieprawidłowa.</summary>
    string? GetPhysicalPath(string fileName);
}

public sealed partial class PictureFileStore : IPictureFileStore
{
    private readonly string _root;
    private readonly string _baseUrl;
    private readonly ILogger<PictureFileStore> _logger;

    public PictureFileStore(
        IOptions<PictureStorageOptions> options,
        IHostEnvironment env,
        ILogger<PictureFileStore> logger)
    {
        _root = Path.GetFullPath(Path.Combine(env.ContentRootPath, options.Value.RootPath));
        _baseUrl = options.Value.PublicBaseUrl.TrimEnd('/');
        _logger = logger;

        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveAsync(ProcessedImage image, CancellationToken ct)
    {
        var name = Guid.NewGuid().ToString("N");
        var mainPath = Path.Combine(_root, $"{name}.webp");
        var thumbPath = Path.Combine(_root, $"{name}_thumb.webp");

        try
        {
            await File.WriteAllBytesAsync(mainPath, image.Main, ct);
            await File.WriteAllBytesAsync(thumbPath, image.Thumbnail, ct);
        }
        catch
        {
            TryDelete(mainPath);
            TryDelete(thumbPath);
            throw;
        }

        return $"{_baseUrl}/{name}.webp";
    }

    public void Delete(string pictureLink)
    {
        var name = LocalFileName(pictureLink);
        if (name is null) return;

        TryDelete(Path.Combine(_root, name));
        TryDelete(Path.Combine(_root, ThumbName(name)));
    }

    public string ThumbnailLink(string pictureLink) =>
        LocalFileName(pictureLink) is { } name ? $"{_baseUrl}/{ThumbName(name)}" : pictureLink;

    public string? GetPhysicalPath(string fileName) =>
        AnyFileName().IsMatch(fileName) ? Path.Combine(_root, fileName) : null;

    private string? LocalFileName(string pictureLink)
    {
        var prefix = _baseUrl + "/";
        if (!pictureLink.StartsWith(prefix, StringComparison.Ordinal)) return null;

        var name = pictureLink[prefix.Length..];
        return MainFileName().IsMatch(name) ? name : null;
    }

    private static string ThumbName(string mainName) =>
        mainName[..^".webp".Length] + "_thumb.webp";

    private void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Nie udało się usunąć pliku {Path}", path);
        }
    }

    [GeneratedRegex(@"^[0-9a-f]{32}\.webp$")]
    private static partial Regex MainFileName();

    [GeneratedRegex(@"^[0-9a-f]{32}(_thumb)?\.webp$")]
    private static partial Regex AnyFileName();
}
