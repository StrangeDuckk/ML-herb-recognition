using HerbRecognition_APIs.DTOs;
using HerbRecognition_APIs.Models;
using HerbRecognition_APIs.Pictures;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HerbRecognition_APIs.Services
{
    public enum PictureOwnerType { Plant, UserInput }

    public readonly record struct PictureOwner(PictureOwnerType Type, int Id);


    /// <summary>Błąd, który można pokazać użytkownikowi (400).</summary>
    public class PictureValidationException(string message) : Exception(message);

    public interface IPictureService
    {
        Task<List<PictureDTO>> GetAsync(PictureOwner owner);
        Task<List<PictureDTO>> AddAsync(PictureOwner owner, IReadOnlyList<IFormFile> uploads, CancellationToken ct);
        Task<bool> DeleteAsync(int pictureId);
        Task<bool> SetMainAsync(int pictureId);
        Task<Dictionary<int, string>> GetPlantMainThumbnailsAsync();//Miniatura zdjęcia głównego każdej rośliny
    }

    public class PictureService( HerbRecognitionDbContext data, IPictureFileStore fileStore, ImageProcessor imageProcessor) : IPictureService
    {
        public const int MaxPictures = 3;
        public const long MaxFileBytes = 8 * 1024 * 1024;

        public async Task<List<PictureDTO>> GetAsync(PictureOwner owner)
        {
            var rows = await OwnedBy(owner)
                .AsNoTracking()
                .OrderByDescending(p => p.CzyGlowneZdjecie)
                .ThenBy(p => p.Id)
                .ToListAsync();

            return rows.Select(ToDto).ToList();
        }

        public async Task<List<PictureDTO>> AddAsync(
            PictureOwner owner, IReadOnlyList<IFormFile> uploads, CancellationToken ct)
        {
            if (uploads.Count == 0)
                throw new PictureValidationException("Nie wybrano żadnego zdjęcia.");

            if (!await OwnerExistsAsync(owner, ct))
                throw new KeyNotFoundException(owner.Type == PictureOwnerType.Plant
                    ? $"Roślina #{owner.Id} nie istnieje."
                    : $"Zgłoszenie #{owner.Id} nie istnieje.");

            var existing = await OwnedBy(owner).CountAsync(ct);
            if (existing + uploads.Count > MaxPictures)
                throw new PictureValidationException(
                    $"Można mieć najwyżej {MaxPictures} zdjęcia. Zapisane: {existing}, " +
                    $"możesz dodać jeszcze {Math.Max(0, MaxPictures - existing)}.");

            // 1. jedno złe zdjęcie = nic nie zapisywane
            var processed = new List<ProcessedImage>();
            foreach (var upload in uploads)
            {
                if (upload.Length == 0 || upload.Length > MaxFileBytes)
                    throw new PictureValidationException(
                        $"„{upload.FileName}” jest pusty lub większy niż {MaxFileBytes / 1024 / 1024} MB.");

                try
                {
                    await using var stream = upload.OpenReadStream();
                    processed.Add(await imageProcessor.ProcessAsync(stream, ct));
                }
                catch (InvalidImageException ex)
                {
                    throw new PictureValidationException($"„{upload.FileName}”: {ex.Message}");
                }
            }

            // 2. pliki na dysk -> wiersze w bazie; przy błędzie sprzątanie plikow
            var hasMain = await OwnedBy(owner).AnyAsync(p => p.CzyGlowneZdjecie, ct);
            var savedLinks = new List<string>();

            try
            {
                foreach (var image in processed)
                {
                    var link = await fileStore.SaveAsync(image, ct);
                    savedLinks.Add(link);

                    data.Pictures.Add(new Picture
                    {
                        Plantid = owner.Type == PictureOwnerType.Plant ? owner.Id : null,
                        UserInputId = owner.Type == PictureOwnerType.UserInput ? owner.Id : null,
                        Picturelink = link,
                        CzyGlowneZdjecie = !hasMain && savedLinks.Count == 1
                    });
                }

                await data.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                foreach (var link in savedLinks)
                    fileStore.Delete(link);

                // trigger picture_max_3 – sa juz 3 zdjecia w bazie
                if (ex is DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.CheckViolation } })
                    throw new PictureValidationException($"Można mieć najwyżej {MaxPictures} zdjęcia.");

                throw;
            }

            return await GetAsync(owner);
        }

        public async Task<bool> DeleteAsync(int pictureId)
        {
            await using var transaction = await data.Database.BeginTransactionAsync();

            var picture = await data.Pictures.FirstOrDefaultAsync(p => p.Id == pictureId);
            if (picture is null) return false;

            data.Pictures.Remove(picture);
            await data.SaveChangesAsync();

            // usunięto główne -> główne zostaje najstarsze z pozostałych
            if (picture.CzyGlowneZdjecie)
            {
                var next = await OwnedBy(OwnerOf(picture)).OrderBy(p => p.Id).FirstOrDefaultAsync();
                if (next is not null)
                {
                    next.CzyGlowneZdjecie = true;
                    await data.SaveChangesAsync();
                }
            }

            await transaction.CommitAsync();

            // plik dopiero po zatwierdzeniu – przy błędzie bazy zdjęcie zostaje całe
            fileStore.Delete(picture.Picturelink);
            return true;
        }

        public async Task<bool> SetMainAsync(int pictureId)
        {
            await using var transaction = await data.Database.BeginTransactionAsync();

            var picture = await data.Pictures.AsNoTracking().FirstOrDefaultAsync(p => p.Id == pictureId);
            if (picture is null) return false;

            // dwa osobne UPDATE-y – unikalny indeks pozwala na jedno główne naraz
            await OwnedBy(OwnerOf(picture))
                .Where(p => p.CzyGlowneZdjecie)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.CzyGlowneZdjecie, false));

            await data.Pictures
                .Where(p => p.Id == pictureId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.CzyGlowneZdjecie, true));

            await transaction.CommitAsync();
            return true;
        }

        public async Task<Dictionary<int, string>> GetPlantMainThumbnailsAsync()
        {
            var rows = await data.Pictures
                .AsNoTracking()
                .Where(p => p.Plantid != null)
                .Select(p => new { PlantId = p.Plantid!.Value, p.Id, p.Picturelink, p.CzyGlowneZdjecie })
                .ToListAsync();

            return rows
                .GroupBy(r => r.PlantId)
                .ToDictionary(
                    g => g.Key,
                    g => fileStore.ThumbnailLink(g
                        .OrderByDescending(r => r.CzyGlowneZdjecie)
                        .ThenBy(r => r.Id)
                        .First()
                        .Picturelink));
        }

        // ------------------------------------------------------------------

        private IQueryable<Picture> OwnedBy(PictureOwner owner)
        {
            var id = owner.Id;
            return owner.Type == PictureOwnerType.Plant
                ? data.Pictures.Where(p => p.Plantid == id)
                : data.Pictures.Where(p => p.UserInputId == id);
        }

        private static PictureOwner OwnerOf(Picture picture) =>
            picture.Plantid is int plantId
                ? new PictureOwner(PictureOwnerType.Plant, plantId)
                : new PictureOwner(PictureOwnerType.UserInput, picture.UserInputId!.Value);

        private Task<bool> OwnerExistsAsync(PictureOwner owner, CancellationToken ct)
        {
            var id = owner.Id;
            return owner.Type == PictureOwnerType.Plant
                ? data.Plants.AnyAsync(p => p.Id == id, ct)
                : data.UserInputs.AnyAsync(u => u.Id == id, ct);
        }

        private PictureDTO ToDto(Picture p) => new()
        {
            Id = p.Id,
            Url = p.Picturelink,
            ThumbnailUrl = fileStore.ThumbnailLink(p.Picturelink),
            IsMain = p.CzyGlowneZdjecie,
            Source = p.Zrodlo
        };
    }
}
