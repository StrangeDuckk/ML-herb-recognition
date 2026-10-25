using HerbRecognition_APIs.DTOs;
using HerbRecognition_APIs.Services;
using Microsoft.AspNetCore.Mvc;

namespace HerbRecognition_APIs.Controllers
{
    [ApiController]
    [Route("api/search")]
    public class SearchController(
        IDbService dbService,
        ISubmissionService submissions,
        IPlantSearchService plantSearch) : ControllerBase
    {
        // GET api/search/similarplants?name=...
        [HttpGet("similarplants")]
        public async Task<IActionResult> SearchSimilarPlants([FromQuery] string? name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Trim().Length < 2)
                return Ok(Array.Empty<SimilarPlantCandidateDTO>());

            return Ok(await dbService.SearchSimilarCandidatesAsync(name));
        }

        // GET api/search/similarplants/resolve?plantIds=2&plantIds=8&userInputIds=16
        // zamienia zapisane Id na nazwy – potrzebne przy wczytaniu zgłoszenia do edycji
        [HttpGet("similarplants/resolve")]
        public async Task<IActionResult> ResolveSimilarPlants(
            [FromQuery] List<int>? plantIds,
            [FromQuery] List<int>? userInputIds)
        {
            return Ok(await submissions.ResolveSimilarAsync(plantIds, userInputIds));
        }

        // POST api/search/form  – wyszukiwanie przez formularz z procentem dopasowania
        [HttpPost("form")]
        public async Task<IActionResult> SearchByForm([FromBody] PlantSearchCriteriaDTO criteria)
        {
            return Ok(await plantSearch.SearchAsync(criteria));
        }
    }
}
