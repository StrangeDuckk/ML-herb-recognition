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
            var response = await _httpClient.PostAsJsonAsync(
                    "api/userInput",
                    dto
                ); ;

            response.EnsureSuccessStatusCode();

            var resoult = await response.Content.ReadFromJsonAsync<CreatedUserInputDTO>();

            return resoult?.Id ?? throw new InvalidOperationException("API nie zwrocilo ID zlgoszenia");
        }
    }
}
