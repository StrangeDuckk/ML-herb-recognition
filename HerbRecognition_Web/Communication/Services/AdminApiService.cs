using HerbRecognition_Web.Communication.DTOs;
using System.Net;
using System.Net.Http.Headers;

namespace HerbRecognition_Web.Communication.Services
{
    public class AdminApiService(HttpClient http, AdminAuthService auth)
    {
        // --------- nizej edycja slownikow ------------
        public Task CreateAsync(string dictionary, DictionaryItemDTO item) =>
            SendAsync(HttpMethod.Post, $"api/admin/dictionaries/{dictionary}", item);

        public Task UpdateAsync(string dictionary, DictionaryItemDTO item) =>
            SendAsync(HttpMethod.Put, $"api/admin/dictionaries/{dictionary}/{item.Id}", item);

        public Task DeleteAsync(string dictionary, int id) =>
            SendAsync(HttpMethod.Delete, $"api/admin/dictionaries/{dictionary}/{id}", null);

        private async Task SendAsync(HttpMethod method, string url, object? body)
        {
            if (!auth.IsLoggedIn)
                throw new UnauthorizedAccessException();

            using var request = new HttpRequestMessage(method, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

            if (body is not null)
                request.Content = JsonContent.Create(body);

            using var response = await http.SendAsync(request);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                throw new UnauthorizedAccessException();

            if (!response.IsSuccessStatusCode)
            {
                var text = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(string.IsNullOrWhiteSpace(text)
                    ? $"Błąd API ({(int)response.StatusCode})."
                    : text);
            }
        }
    }
}
