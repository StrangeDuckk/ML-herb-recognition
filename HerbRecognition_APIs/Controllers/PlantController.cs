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
            var plants = await dbService.GetAllPlantsAsync();

            return Ok(plants);
        }
        //[HttpGet("{id: int}")]
        //public async Task<IActionResult> GetPlantsById(int id)//todo
        //{
        //    throw new NotImplementedException();

        //    var plants = await dbService.GetPlantByIdAsync(id);

        //    return Ok(plants);
        //}

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
