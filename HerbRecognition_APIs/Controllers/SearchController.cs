using HerbRecognition_APIs.DTOs;
using HerbRecognition_APIs.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HerbRecognition_APIs.Controllers
{
    [ApiController]
    [Route("api/search")]
    [EnableRateLimiting("search")]
    public class SearchController(
        IDbService dbService,
        ISubmissionService submissions,
        IPlantSearchService plantSearch) : ControllerBase
    {
        private const int MaxResolveIds = 50;

        // GET api/search/similarplants?name=...
        [HttpGet("similarplants")]
        public async Task<IActionResult> SearchSimilarPlants([FromQuery] string? name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Trim().Length < 2)
                return Ok(Array.Empty<SimilarPlantCandidateDTO>());

            if (name.Length > LikePattern.MaxSearchLength)
                return BadRequest($"Fraza może mieć maksymalnie {LikePattern.MaxSearchLength} znaków.");

            return Ok(await dbService.SearchSimilarCandidatesAsync(name));
        }

        // GET api/search/similarplants/resolve?plantIds=2&plantIds=8&userInputIds=16
        [HttpGet("similarplants/resolve")]
        public async Task<IActionResult> ResolveSimilarPlants(
            [FromQuery] List<int>? plantIds,
            [FromQuery] List<int>? userInputIds)
        {
            if ((plantIds?.Count ?? 0) + (userInputIds?.Count ?? 0) > MaxResolveIds)
                return BadRequest($"Można pobrać maksymalnie {MaxResolveIds} roślin naraz.");

            return Ok(await submissions.ResolveSimilarAsync(plantIds, userInputIds));
        }

        // POST api/search/form
        [HttpPost("form")]
        [RequestSizeLimit(64 * 1024)]
        public async Task<IActionResult> SearchByForm([FromBody] PlantSearchCriteriaDTO criteria)
        {
            return Ok(await plantSearch.SearchAsync(criteria));
        }
    }
}