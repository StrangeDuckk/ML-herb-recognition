using HerbRecognition_APIs.DTOs;
using HerbRecognition_APIs.Services;
using Microsoft.AspNetCore.Mvc;

namespace HerbRecognition_APIs.Controllers
{
    [ApiController]
    [Route("api/search")]
    public class SearchController(IDbService dbService) :ControllerBase
    {
        //get api/search/similarplants?name=...
        [HttpGet("similarplants")]
        public async Task<IActionResult> SearchSimilarPlants([FromQuery] string? name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Trim().Length < 2)
                return Ok(Array.Empty<SimilarPlantCandidateDTO>());

            return Ok(await dbService.SearchSimilarCandidatesAsync(name));
        }
    }
}
