using HerbRecognition_APIs.DTOs;
using HerbRecognition_APIs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HerbRecognition_APIs.Controllers
{
    [ApiController]
    [Route("api/admin/userinputs")]
    [Authorize(Roles = "Admin")]
    public class AdminUserInputController(ISubmissionService submissions, ILogger<AdminUserInputController> logger)
        : ControllerBase
    {
        // GET api/admin/userinputs
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await submissions.GetSummariesAsync());
        }

        // GET api/admin/userinputs/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var dto = await submissions.GetAsync(id);

            return dto is null
                ? NotFound($"Zgłoszenie #{id} nie istnieje albo jest nieczytelne.")
                : Ok(dto);
        }

        // PUT api/admin/userinputs/5  – "Zapisz zmiany"
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreatePlantDTO dto)
        {
            try
            {
                return await submissions.UpdateAsync(id, dto)
                    ? NoContent()
                    : NotFound($"Zgłoszenie #{id} nie istnieje.");
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE api/admin/userinputs/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            return await submissions.DeleteAsync(id)
                ? NoContent()
                : NotFound($"Zgłoszenie #{id} nie istnieje.");
        }

        // POST api/admin/userinputs/5/approve  – "Dodaj do bazy"
        [HttpPost("{id:int}/approve")]
        public async Task<IActionResult> Approve(int id, [FromBody] CreatePlantDTO dto)
        {
            try
            {
                var plantId = await submissions.ApproveAsync(id, dto);
                return Ok(new { PlantId = plantId });
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Nie udało się zatwierdzić zgłoszenia {Id}", id);
                return BadRequest("Nie udało się zapisać rośliny. Sprawdź poprawność danych.");
            }
        }
    }
}