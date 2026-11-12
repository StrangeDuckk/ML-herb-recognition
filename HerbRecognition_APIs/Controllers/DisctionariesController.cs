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
        [HttpGet("occurance")]
        public async Task<ActionResult<GetOccuranceDTO>> GetAllDictOccurancesAsync()
        {
            return Ok(await dbService.GetAllDictOccurancesAsync());
        }

        [HttpGet("plantType")]
        public async Task<ActionResult<GetPlantTypeDTO>> GetAllPlantType ()
        {
            return Ok(await dbService.GetAllDictPlantTypesAsync());
        }

        [HttpGet("shape")]
        public async Task<ActionResult<GetShapeDTO>> GetAllDictShapeAsync()
        {
            return Ok(await dbService.GetAllDictShapesAsync());
        }

        [HttpGet("flavour")]
        public async Task<ActionResult<GetFlavourDTO>> GetAllDictFlavourAsync()
        {
            return Ok(await dbService.GetAllDictFlavoursAsync());
        }

        [HttpGet("poisonability")]
        public async Task<ActionResult<GetPoisonabilityDTO>> GetAllDictPoisonabilitiesAsync()
        {
            return Ok(await dbService.GetAllDictPoisonabilitiesAsync());
        }

        [HttpGet("surface")]
        public async Task<ActionResult<GetSurfaceDTO>> GetAllDictSurfacesAsync()
        {
            return Ok(await dbService.GetAllDictSurfacesAsync());
        }

        [HttpGet("thickness")]
        public async Task<ActionResult<GetThicknessDTO>> GetAllDictThicknessesAsync()
        {
            return Ok(await dbService.GetAllDictThicknessesAsync());
        }

        [HttpGet("color")]
        public async Task<ActionResult<GetColorDTO>> GetAllColorDtoAsync()
        {
            return Ok(await dbService.GetAllColorAsync());
        }

        [HttpGet("monthOfHarvest")]
        public async Task<ActionResult<GetOccuranceDTO>> GetAllDictMonthOfHarvestAsync()
        {
            return Ok(await dbService.GetAllDictMonthOfHarvestAsync());
        }
        [HttpGet("productType")]
        public async Task<IActionResult> GetProductTypes() =>
            Ok(await dbService.GetAllDictProductTypesAsync());

        [HttpGet("healthProperty")]
        public async Task<IActionResult> GetHealthProperties() =>
            Ok(await dbService.GetAllDictHealthPropertiesAsync());

        [HttpGet("activeSubstance")]
        public async Task<IActionResult> GetActiveSubstances() =>
            Ok(await dbService.GetAllDictActiveSubstancesAsync());

        [HttpGet("disease")]
        public async Task<IActionResult> GetDiseases() =>
            Ok(await dbService.GetAllDictDiseasesAsync());

        [HttpGet("contraindication")]
        public async Task<IActionResult> GetContraindications() =>
            Ok(await dbService.GetAllDictContraindicationsAsync());
    }
}
