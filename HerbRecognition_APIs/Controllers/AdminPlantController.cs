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
    public class AdminPlantController(ISubmissionService submissions) : ControllerBase
    {
        // GET api/admin/plants
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await submissions.GetPlantSummariesAsync());
        }

        // GET api/admin/plants/8  – roślina w formacie formularza
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var dto = await submissions.GetPlantForEditAsync(id);

            return dto is null
                ? NotFound($"Roślina #{id} nie istnieje.")
                : Ok(dto);
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
                return BadRequest($"Nie udało się zapisać rośliny: {ex.InnerException?.Message ?? ex.Message}");
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
            catch (DbUpdateException)
            {
                return Conflict("Nie można usunąć rośliny – ma przypisane zdjęcia lub produkty. Usuń je najpierw.");
            }
        }

        // POST api/admin/plants  – dodanie rośliny bez zgłoszenia
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
                return BadRequest($"Nie udało się zapisać rośliny: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
    }
}