using HerbRecognition_APIs.DTOs;
using HerbRecognition_APIs.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
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
        public async Task<IActionResult> GetPlantsById(int id)
        {
            var plant = await dbService.GetPlantByIdAsync(id);

            if (plant == null)
                return NotFound();

            return Ok(plant);
        }

        //[HttpPost]//todo w przyszlosci tez patch albo put
        //public async Task<IActionResult> CreatePlant([FromBody] CreatePlantDTO dto)
        //{
        //    var plant = await dbService.CreatePlantAsync(dto);

        //    return CreatedAtAction(
        //        nameof(GetPlantsById),
        //        new {id =  plant.Id},
        //        plant
        //    );
        //}
    }
}
