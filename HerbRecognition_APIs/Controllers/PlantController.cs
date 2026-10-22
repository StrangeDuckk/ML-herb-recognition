using HerbRecognition_APIs.DTOs;
using HerbRecognition_APIs.Models;
using HerbRecognition_APIs.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using System.Text.Json;
using System.Threading.Tasks;

namespace HerbRecognition_APIs.Controllers
{
    [ApiController]
    [Route("api/plants")]
    public class PlantController(IDbService dbService) : ControllerBase
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

        //[HttpPost]
        //public async Task<IActionResult> CreatePlantAsync([FromBody] CreatePlantDTO dto)
        //{
        //    var plant = await dbService.CreatePlantAsync(dto);

        //    return CreatedAtAction(
        //        nameof(GetPlantsById),
        //        new { id = plant.Id },
        //        plant
        //    );
        //}
    }
}
