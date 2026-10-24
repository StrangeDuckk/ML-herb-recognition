using HerbRecognition_Web.Communication.DTOs;
using System.Net;
using System.Net.Http.Headers;

namespace HerbRecognition_Web.Communication.Services
{
    public class AdminApiService(HttpClient http, AdminAuthService auth)
    {
        // --------- słowniki ------------
        public Task CreateAsync(string dictionary, DictionaryItemDTO item) =>
            SendAsync(HttpMethod.Post, $"api/admin/dictionaries/{dictionary}", item);

        public Task UpdateAsync(string dictionary, DictionaryItemDTO item) =>
            SendAsync(HttpMethod.Put, $"api/admin/dictionaries/{dictionary}/{item.Id}", item);

        public Task DeleteAsync(string dictionary, int id) =>
            SendAsync(HttpMethod.Delete, $"api/admin/dictionaries/{dictionary}/{id}", null);

        // --------- zgłoszenia (userinput) ------------
        public async Task<List<UserInputSummaryDTO>> GetSubmissionsAsync() =>
            await SendAsync<List<UserInputSummaryDTO>>(HttpMethod.Get, "api/admin/userinputs", null)
            ?? new List<UserInputSummaryDTO>();

        public async Task<CreatePlantDTO> GetSubmissionAsync(int id) =>
            await SendAsync<CreatePlantDTO>(HttpMethod.Get, $"api/admin/userinputs/{id}", null)
            ?? throw new InvalidOperationException($"API nie zwróciło zgłoszenia #{id}.");

        public Task UpdateSubmissionAsync(int id, CreatePlantDTO dto) =>
            SendAsync(HttpMethod.Put, $"api/admin/userinputs/{id}", dto);

        public Task DeleteSubmissionAsync(int id) =>
            SendAsync(HttpMethod.Delete, $"api/admin/userinputs/{id}", null);

        /// <summary>Dodaje zgłoszenie do bazy. Zwraca Id nowej rośliny.</summary>
        public async Task<int> ApproveSubmissionAsync(int id, CreatePlantDTO dto)
        {
            var result = await SendAsync<ApproveResult>(HttpMethod.Post, $"api/admin/userinputs/{id}/approve", dto);
            return result?.PlantId ?? throw new InvalidOperationException("API nie zwróciło Id nowej rośliny.");
        }

        private sealed record ApproveResult(int PlantId);

        // --------- rośliny w bazie (plant) ------------
        public async Task<List<UserInputSummaryDTO>> GetPlantsAsync() =>
            await SendAsync<List<UserInputSummaryDTO>>(HttpMethod.Get, "api/admin/plants", null)
            ?? new List<UserInputSummaryDTO>();

        public async Task<CreatePlantDTO> GetPlantAsync(int id) =>
            await SendAsync<CreatePlantDTO>(HttpMethod.Get, $"api/admin/plants/{id}", null)
            ?? throw new InvalidOperationException($"API nie zwróciło rośliny #{id}.");

        public Task UpdatePlantAsync(int id, CreatePlantDTO dto) =>
            SendAsync(HttpMethod.Put, $"api/admin/plants/{id}", dto);

        public Task DeletePlantAsync(int id) =>
            SendAsync(HttpMethod.Delete, $"api/admin/plants/{id}", null);

        // --------- wspólne ------------
        private async Task SendAsync(HttpMethod method, string url, object? body)
        {
            using var response = await SendCoreAsync(method, url, body);
        }

        private async Task<T?> SendAsync<T>(HttpMethod method, string url, object? body)
        {
            using var response = await SendCoreAsync(method, url, body);
            return await response.Content.ReadFromJsonAsync<T>();
        }

        private async Task<HttpResponseMessage> SendCoreAsync(HttpMethod method, string url, object? body)
        {
            if (!auth.IsLoggedIn)
                throw new UnauthorizedAccessException();

            using var request = new HttpRequestMessage(method, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

            if (body is not null)
                request.Content = JsonContent.Create(body);

            var response = await http.SendAsync(request);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                response.Dispose();
                throw new UnauthorizedAccessException();
            }

            if (!response.IsSuccessStatusCode)
            {
                var text = await response.Content.ReadAsStringAsync();
                var status = (int)response.StatusCode;
                response.Dispose();

                throw new HttpRequestException(string.IsNullOrWhiteSpace(text)
                    ? $"Błąd API ({status})."
                    : text);
            }

            return response;
        }
    }
}
