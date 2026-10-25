using HerbRecognition_APIs.DTOs;
using HerbRecognition_APIs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HerbRecognition_APIs.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController(IDbService dbService, ILogger<AdminController> logger) : ControllerBase
    {
        [HttpPost("dictionaries/{dictionary}")]
        public async Task<IActionResult> CreateItemAsync(string dictionary, [FromBody] DictionaryItemDTO dto)
        {
            try
            {
                var id = await dbService.CreateDictionaryItemAsync(dictionary, dto);
                return Ok(new { Id = id });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Nie udało się dodać wpisu do słownika {Dictionary}", dictionary);
                return Conflict("Nie udało się dodać wpisu – możliwe, że taka wartość już istnieje.");
            }
        }

        [HttpPut("dictionaries/{dictionary}/{id:int}")]
        public async Task<IActionResult> Update(string dictionary, int id, [FromBody] DictionaryItemDTO dto)
        {
            try
            {
                return await dbService.UpdateDictionaryItemAsync(dictionary, id, dto)
                    ? NoContent()
                    : NotFound("Nie znaleziono wpisu.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Nie udało się zmienić wpisu {Id} w słowniku {Dictionary}", id, dictionary);
                return Conflict("Nie udało się zapisać zmiany – możliwe, że taka wartość już istnieje.");
            }
        }

        [HttpDelete("dictionaries/{dictionary}/{id:int}")]
        public async Task<IActionResult> Delete(string dictionary, int id)
        {
            try
            {
                return await dbService.DeleteDictionaryItemAsync(dictionary, id)
                    ? NoContent()
                    : NotFound("Nie znaleziono wpisu.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateException)
            {
                return Conflict("Nie można usunąć – ta wartość jest używana przez zapisane rośliny.");
            }
        }
    }
}