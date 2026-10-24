using HerbRecognition_APIs.Services;
using Microsoft.AspNetCore.Mvc;

namespace HerbRecognition_APIs.Controllers
{
    [ApiController]
    [Route("api/plants")]
    public class PlantController(IDbService dbService, ISubmissionService submissions) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetPlants()
        {
            var plants = await dbService.GetPlantsAsync();

            return Ok(plants);
        }

        [HttpGet("/all")]
        public async Task<IActionResult> GetAllPlants()
        {
            var plants = await dbService.GetAllPlantsAsync();

            return Ok(plants);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPlantsByIdAsync(int id)
        {
            var plant = await dbService.GetPlantByIdAsync(id);

            if (plant == null)
                return NotFound();

            return Ok(plant);
        }

        [HttpGet("{id:int}/pending-similar")] //zwrocenie oddzielnie podobnych roslin
        public async Task<IActionResult> GetPendingSimilar(int id)
        {
            return Ok(await dbService.GetPendingSimilarToPlantAsync(id));
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetPlantsByNameAsync(string name)
        {
            var plants = await dbService.GetPlantByNameAsync(name);

            if (!plants.Any())
                return NotFound();

            return Ok(plants);
        }

        // GET api/plants/pending
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingPlants()
        {
            return Ok(await submissions.GetPendingPreviewsAsync());
        }
    }
}
