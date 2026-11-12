using HerbRecognition_Web.Communication.DTOs;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace HerbRecognition_Web.Communication.Services
{
    public class PictureApiException(string message) : Exception(message);

    /// <summary>Publiczne operacje: zdjęcia do nowego zgłoszenia i pobieranie plików.</summary>
    public partial class PictureApiService(HttpClient http)
    {
        public Task<List<PictureDTO>> UploadForSubmissionAsync(
            int submissionId, IReadOnlyList<PhotoUpload> photos, CancellationToken ct = default) =>
            UploadAsync(http, $"api/userInput/{submissionId}/pictures", photos, bearerToken: null, ct);
        public Task<List<PictureDTO>> UploadForProductSubmissionAsync(
            int productSubmissionId, IReadOnlyList<PhotoUpload> photos, CancellationToken ct = default) =>
            UploadAsync(http, $"api/productInput/{productSubmissionId}/pictures", photos, bearerToken: null, ct);

        /// <summary>Odpowiedź z plikiem albo null (zła nazwa / brak pliku). Wywołujący zwalnia odpowiedź.</summary>
        public async Task<HttpResponseMessage?> GetFileAsync(string fileName, CancellationToken ct)
        {
            if (!FileName().IsMatch(fileName)) return null;

            var response = await http.GetAsync(
                $"api/pictures/files/{fileName}", HttpCompletionOption.ResponseHeadersRead, ct);

            if (response.IsSuccessStatusCode) return response;

            response.Dispose();
            return null;
        }

        // ------------------------------------------------------------------

        internal static async Task<List<PictureDTO>> UploadAsync(
            HttpClient http, string url, IReadOnlyList<PhotoUpload> photos, string? bearerToken, CancellationToken ct)
        {
            using var content = new MultipartFormDataContent();

            foreach (var photo in photos)
            {
                var file = new ByteArrayContent(photo.Data);
                file.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                content.Add(file, "photos", photo.FileName);
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            if (bearerToken is not null)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            using var response = await http.SendAsync(request, ct);
            await EnsureSuccessAsync(response, ct);

            return await response.Content.ReadFromJsonAsync<List<PictureDTO>>(ct) ?? [];
        }

        internal static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
        {
            if (response.IsSuccessStatusCode) return;

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                throw new UnauthorizedAccessException();

            var body = (await response.Content.ReadAsStringAsync(ct)).Trim().Trim('"');

            // API zwraca czytelny komunikat (400/404/429); długie treści to błędy techniczne
            throw new PictureApiException(
                body.Length is > 0 and <= 300 ? body : "Nie udało się zapisać zdjęć. Spróbuj ponownie.");
        }

        [GeneratedRegex(@"^[0-9a-f]{32}(_thumb)?\.webp$")]
        private static partial Regex FileName();
    }

    /// <summary>Operacje admina: lista, dodawanie, usuwanie, zdjęcie główne.</summary>
    public class AdminPictureApiService(HttpClient http, AdminAuthService auth)
    {
        public async Task<List<PictureDTO>> GetAsync(PictureOwnerKind owner, int ownerId, CancellationToken ct = default)
        {
            using var request = Authorized(HttpMethod.Get, $"{OwnerPath(owner, ownerId)}/pictures");
            using var response = await http.SendAsync(request, ct);
            await PictureApiService.EnsureSuccessAsync(response, ct);

            return await response.Content.ReadFromJsonAsync<List<PictureDTO>>(ct) ?? [];
        }

        public Task<List<PictureDTO>> UploadAsync(
            PictureOwnerKind owner, int ownerId, IReadOnlyList<PhotoUpload> photos, CancellationToken ct = default) =>
            PictureApiService.UploadAsync(http, $"{OwnerPath(owner, ownerId)}/pictures", photos, Token, ct);

        public async Task DeleteAsync(int pictureId, CancellationToken ct = default)
        {
            using var request = Authorized(HttpMethod.Delete, $"api/pictures/{pictureId}");
            using var response = await http.SendAsync(request, ct);
            await PictureApiService.EnsureSuccessAsync(response, ct);
        }

        public async Task SetMainAsync(int pictureId, CancellationToken ct = default)
        {
            using var request = Authorized(HttpMethod.Put, $"api/pictures/{pictureId}/main");
            using var response = await http.SendAsync(request, ct);
            await PictureApiService.EnsureSuccessAsync(response, ct);
        }

        // ------------------------------------------------------------------

        private static string OwnerPath(PictureOwnerKind owner, int id) => owner switch
        {
            PictureOwnerKind.Plant => $"api/plants/{id}",
            PictureOwnerKind.Submission => $"api/userInput/{id}",
            PictureOwnerKind.Product => $"api/products/{id}",
            _ => $"api/productInput/{id}"
        };

        private string Token =>
            auth.Token ?? throw new UnauthorizedAccessException();

        private HttpRequestMessage Authorized(HttpMethod method, string url)
        {
            var request = new HttpRequestMessage(method, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);
            return request;
        }
    }
}
