using HerbRecognition_Web.Communication.DTOs;
using HerbRecognition_Web.Components.Pages;

namespace HerbRecognition_Web.Communication.Services
{
    public class PlantApiService //komunikacja z api
    {
        private readonly HttpClient _httpClient;

        public PlantApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<GetPreviewPlantDTO>> GetAllPlantsAsync()
        {
            var plants = await _httpClient.GetFromJsonAsync<List<GetPreviewPlantDTO>>(
                "api/plants");

            return plants ?? new List<GetPreviewPlantDTO>();
        }
        public async Task<GetPlantDTO> GetPlantByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<GetPlantDTO>(
                    $"api/plants/{id}"
                );
        }
    }
}
