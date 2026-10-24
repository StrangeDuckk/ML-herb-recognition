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

        public async Task<List<GetPlantDTO>> GetPlantsByNameAsync(string name)
        {
            var plants = await _httpClient.GetFromJsonAsync<List<GetPlantDTO>>(
                    $"api/plants/{Uri.EscapeDataString(name)}"//dzieki uri moga byc spacje
                );

            return plants ?? new List<GetPlantDTO>();
        }

        public async Task<List<SimilarPlantCandidateDTO>> SearchSimilarCandidatesAsync(string name, CancellationToken cancellation = default)
        {
            var resoult = await _httpClient.GetFromJsonAsync<List<SimilarPlantCandidateDTO>>(
                    $"api/search/similarplants?name={Uri.EscapeDataString(name)}",
                    cancellation
                );

            return resoult ?? new List<SimilarPlantCandidateDTO>();
        }
        public async Task<List<SimilarPlantCandidateDTO>> GetPendingSimilarAsync(int plantId)
        {
            var result = await _httpClient.GetFromJsonAsync<List<SimilarPlantCandidateDTO>>(
                $"api/plants/{plantId}/pending-similar");

            return result ?? new List<SimilarPlantCandidateDTO>();
        }
        public async Task<List<SimilarPlantCandidateDTO>> ResolveSimilarAsync(IEnumerable<int>? plantIds, IEnumerable<int>? userInputIds) //id na nazwy dla similarplants
        {
            var parts = (plantIds ?? Enumerable.Empty<int>()).Select(id => $"plantIds={id}")
                .Concat((userInputIds ?? Enumerable.Empty<int>()).Select(id => $"userInputIds={id}"))
                .ToList();

            if (parts.Count == 0)
                return new List<SimilarPlantCandidateDTO>();

            return await _httpClient.GetFromJsonAsync<List<SimilarPlantCandidateDTO>>(
                       $"api/search/similarplants/resolve?{string.Join("&", parts)}")
                   ?? new List<SimilarPlantCandidateDTO>();
        }
    }
}
