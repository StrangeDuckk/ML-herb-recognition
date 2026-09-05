using HerbRecognition_APIs.DTOs;
using HerbRecognition_APIs.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HerbRecognition_APIs.Controllers
{
    [ApiController]
    [Route("api/dictionaries")]
    public class DisctionariesController(IDbService dbService) : ControllerBase
    {
        [HttpGet("plantType")]
        public async Task<ActionResult<GetPlantTypeDTO>> GetAllPlantType ()
        {
            return Ok(await dbService.GetAllPlantTypesAsync());
        }

        //TODO -> wszystkie slowniki na get, NIE set
        //[HttpGet("color")]
        //public async Task<ActionResult<GetColorDTO>> GetAllColorDto()
        //{
        //    return Ok(await dbService.GetAllColorAsync());
        //}
        //[HttpGet("color/{id:int}")]
        //public async Task<ActionResult<GetColorDTO>> GetAllColorDto(int id)
        //{
        //    return Ok(await dbService.GetAllColorAsync());
        //}

        //[HttpGet("surface")]
        //[HttpGet("thickness")]
        //[HttpGet("shape")]
        //[HttpGet("flavour")]
        //[HttpGet("poisonability")]
    }

}
