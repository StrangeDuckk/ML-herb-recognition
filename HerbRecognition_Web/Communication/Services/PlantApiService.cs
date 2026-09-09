using HerbRecognition_Web.Communication.DTOs;

namespace HerbRecognition_Web.Communication.Services
{
    public class PlantApiService //komunikacja z api
    {
        private readonly HttpClient _httpClient;

        public PlantApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<PlantDTO>> GetAllPlantsAsync()
        {
            var plants = await _httpClient.GetFromJsonAsync<List<PlantDTO>>(
                "api/plants");

            return plants ?? new List<PlantDTO>();
        }
    }
}
