using HerbRecognition_APIs.DTOs;
using HerbRecognition_APIs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HerbRecognition_APIs.Controllers
{
    [ApiController]
    [Route("api/userInput")]
    public class UserInputController(IDbService dbService) : ControllerBase
    {
        [HttpPost]
        [EnableRateLimiting("submit")]       // ochrona przed zasypaniem bazy zgłoszeniami
        [RequestSizeLimit(64 * 1024)]        // zgłoszenie to kilka KB – 64 KB z dużym zapasem
        public async Task<IActionResult> CreateUserInputPlantAsync([FromBody] CreatePlantDTO dto)
        {
            try
            {
                var submissionId = await dbService.PostUserInputPlantAsync(dto);

                return Created($"/api/userInput/{submissionId}", new
                {
                    Id = submissionId,
                    Message = "Zgloszenie zostalo zapisane.",
                    Url = $"/api/userInput/{submissionId}"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Pełne dane zgłoszenia tylko dla admina – wcześniej każdy mógł odczytać
        // dowolne zgłoszenie, zgadując kolejne numery.
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserInput(int id)
        {
            var submission = await dbService.GetUserInputPlantAsync(id);

            return submission is null
                ? NotFound("Nie znaleziono zgłoszenia o podanym id.")
                : Ok(submission);
        }
    }
}