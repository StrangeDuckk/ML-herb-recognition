using HerbRecognition_APIs.DTOs;
using HerbRecognition_APIs.Services;
using Microsoft.AspNetCore.Mvc;

namespace HerbRecognition_APIs.Controllers
{
    [ApiController]
    [Route("api/userInput")]
    public class UserInputController(IDbService dbService) : Controller
    {
        [HttpPost]
        public async Task<IActionResult> CreateUserInputPlantAsync([FromBody] CreatePlantDTO dto)
        {
            var sumbissionId = await dbService.PostUserInputPlantAsync(dto);

            return CreatedAtAction(nameof(GetUserInput), new { id = sumbissionId }, new
            {
                Id = sumbissionId,
                Message = "Zgloszenie zostalo zapisane.",
                Url = $"/api/userInput/{sumbissionId}"
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUserInput(int id)
        {
            var subbmission = await dbService.GetUserInputPlantAsync(id);

            if (subbmission == null)
            {
                return NotFound("Wystapil problem w odczytywaniu zgloszenia, nie znaleziono zgloszenia o podanym id");
            }

            return Ok(subbmission);
        }
    }
}
