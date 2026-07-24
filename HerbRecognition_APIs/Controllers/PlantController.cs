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
    }
}
