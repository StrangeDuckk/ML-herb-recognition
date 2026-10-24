using HerbRecognition_Web.Communication.DTOs;

namespace HerbRecognition_Web.Communication.Services
{
    public class UserInputApiService// polaczenie do api user input
    {

        private readonly HttpClient _httpClient;

        public UserInputApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<int> PostUserInputPlantAsync(CreatePlantDTO dto)
        {
            using var response = await _httpClient.PostAsJsonAsync(
                    "api/userInput",
                    dto
            );

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API zwróciło {(int)response.StatusCode}: {responseBody}");
            }

            var created = await response.Content.ReadFromJsonAsync<CreatedResponse>();
            return created?.Id ?? throw new InvalidOperationException("Brak Id w odpowiedzi API.");

        }

        private sealed record CreatedResponse(int Id, string Message, string Url);
    }
}
