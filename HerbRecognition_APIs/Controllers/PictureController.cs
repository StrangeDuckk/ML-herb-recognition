using HerbRecognition_APIs.Pictures;
using HerbRecognition_APIs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HerbRecognition_APIs.Controllers
{
    [ApiController]
    public class PictureController(IPictureService pictures, IPictureFileStore fileStore) : ControllerBase
    {
        // Kestrel ma globalnie 1 MB – tu podnosimy limit tylko dla uploadu.
        // Strona zmniejsza zdjęcia do 2048 px, więc 3 sztuki to zwykle 1–4 MB.
        private const long MaxUploadBytes = 20 * 1024 * 1024;

        // ----- pliki (Web pobiera je i oddaje przeglądarce pod /media/pictures) -----

        [HttpGet("api/pictures/files/{fileName}")]
        public IActionResult GetFile(string fileName)
        {
            var path = fileStore.GetPhysicalPath(fileName);
            if (path is null || !System.IO.File.Exists(path)) return NotFound();

            return PhysicalFile(path, "image/webp");
        }

        // ----- zatwierdzone rośliny -----

        [HttpGet("api/plants/{id:int}/pictures")]
        public async Task<IActionResult> GetForPlant(int id) =>
            Ok(await pictures.GetAsync(new(PictureOwnerType.Plant, id)));

        [Authorize]
        [HttpPost("api/plants/{id:int}/pictures")]
        [RequestSizeLimit(MaxUploadBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadBytes)]
        public Task<IActionResult> UploadForPlant(int id, [FromForm] List<IFormFile> photos, CancellationToken ct) =>
            UploadAsync(new(PictureOwnerType.Plant, id), photos, ct);

        // ----- zgłoszenia -----

        [Authorize]
        [HttpGet("api/userInput/{id:int}/pictures")]
        public async Task<IActionResult> GetForSubmission(int id) =>
            Ok(await pictures.GetAsync(new(PictureOwnerType.UserInput, id)));

        // publiczny: strona wysyła zdjęcia zaraz po utworzeniu zgłoszenia
        // (API jest dostępne tylko z kontenera strony, nie z internetu)
        [HttpPost("api/userInput/{id:int}/pictures")]
        [EnableRateLimiting("submit")]
        [RequestSizeLimit(MaxUploadBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadBytes)]
        public Task<IActionResult> UploadForSubmission(int id, [FromForm] List<IFormFile> photos, CancellationToken ct) =>
            UploadAsync(new(PictureOwnerType.UserInput, id), photos, ct);

        // ----- pojedyncze zdjęcie -----

        [Authorize]
        [HttpDelete("api/pictures/{pictureId:int}")]
        public async Task<IActionResult> Delete(int pictureId) =>
            await pictures.DeleteAsync(pictureId) ? NoContent() : NotFound();

        [Authorize]
        [HttpPut("api/pictures/{pictureId:int}/main")]
        public async Task<IActionResult> SetMain(int pictureId) =>
            await pictures.SetMainAsync(pictureId) ? NoContent() : NotFound();

        // ------------------------------------------------------------------

        private async Task<IActionResult> UploadAsync(PictureOwner owner, List<IFormFile> photos, CancellationToken ct)
        {
            try
            {
                return Ok(await pictures.AddAsync(owner, photos, ct));
            }
            catch (PictureValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
