using HerbRecognition_APIs.DTOs;
using HerbRecognition_APIs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HerbRecognition_APIs.Controllers
{
    [ApiController]
    [Route("api/admin/plants")]
    [Authorize(Roles = "Admin")]
    public class AdminPlantController(ISubmissionService submissions, ILogger<AdminPlantController> logger)
        : ControllerBase
    {
        // GET api/admin/plants
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await submissions.GetPlantSummariesAsync());
        }

        // GET api/admin/plants/8
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var dto = await submissions.GetPlantForEditAsync(id);

            return dto is null ? NotFound($"Roślina #{id} nie istnieje.") : Ok(dto);
        }

        // PUT api/admin/plants/8
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreatePlantDTO dto)
        {
            try
            {
                return await submissions.UpdatePlantAsync(id, dto)
                    ? NoContent()
                    : NotFound($"Roślina #{id} nie istnieje.");
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                // szczegóły tylko w logu serwera – komunikat bazy zdradza strukturę tabel
                logger.LogError(ex, "Nie udało się zapisać rośliny {Id}", id);
                return BadRequest("Nie udało się zapisać rośliny. Sprawdź poprawność danych.");
            }
        }

        // DELETE api/admin/plants/8
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                return await submissions.DeletePlantAsync(id)
                    ? NoContent()
                    : NotFound($"Roślina #{id} nie istnieje.");
            }
            catch (DbUpdateException ex)
            {
                logger.LogWarning(ex, "Nie udało się usunąć rośliny {Id}", id);
                return Conflict("Nie można usunąć rośliny – ma przypisane zdjęcia lub produkty. Usuń je najpierw.");
            }
        }

        // POST api/admin/plants
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePlantDTO dto)
        {
            try
            {
                var plantId = await submissions.CreatePlantAsync(dto);
                return Ok(new { PlantId = plantId });
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Nie udało się dodać rośliny");
                return BadRequest("Nie udało się zapisać rośliny. Sprawdź poprawność danych.");
            }
        }
    }
}