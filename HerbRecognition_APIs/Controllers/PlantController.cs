using HerbRecognition_APIs.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HerbRecognition_APIs.Controllers
{
    [ApiController]
    [Route("api/plants")]
    public class PlantController(IDbService dbService, ISubmissionService submissions) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetPlants()
        {
            return Ok(await dbService.GetPlantsAsync());
        }

        // było "/all" – ukośnik na początku robił z tego adres https://host/all, poza api/plants
        [HttpGet("all")]
        public async Task<IActionResult> GetAllPlants()
        {
            return Ok(await dbService.GetAllPlantsAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPlantsByIdAsync(int id)
        {
            var plant = await dbService.GetPlantByIdAsync(id);

            return plant is null ? NotFound() : Ok(plant);
        }

        [HttpGet("{id:int}/pending-similar")]
        public async Task<IActionResult> GetPendingSimilar(int id)
        {
            return Ok(await dbService.GetPendingSimilarToPlantAsync(id));
        }

        // GET api/plants/pending - wszstkie zgloszenia
        // GET api/plants/pending?name=mięta – zgłoszenia pasujące do nazwy (wyszukiwarka)
        [HttpGet("pending")]
        [EnableRateLimiting("search")]
        public async Task<IActionResult> GetPendingPlants([FromQuery] string? name)
        {
            if (name is not null && name.Length > LikePattern.MaxSearchLength)
                return BadRequest($"Nazwa może mieć maksymalnie {LikePattern.MaxSearchLength} znaków.");

            return Ok(await submissions.GetPendingPreviewsAsync(name));
        }

        [HttpGet("{name}")]
        [EnableRateLimiting("search")]
        public async Task<IActionResult> GetPlantsByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length > LikePattern.MaxSearchLength)
                return BadRequest($"Nazwa musi mieć od 1 do {LikePattern.MaxSearchLength} znaków.");

            var plants = await dbService.GetPlantByNameAsync(name);

            return plants.Any() ? Ok(plants) : NotFound();
        }
    }
}