using HerbRecognition_APIs.DTOs;
using HerbRecognition_APIs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HerbRecognition_APIs.Controllers
{
    /// <summary>
    /// Panel admina: zgłoszenia produktów (api/admin/productinputs)
    /// i produkty w bazie (api/admin/products).
    /// </summary>
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminProductController(IProductService products, ILogger<AdminProductController> logger)
        : ControllerBase
    {
        // =================================================================
        //  ZGŁOSZENIA
        // =================================================================

        [HttpGet("api/admin/productinputs")]
        public async Task<IActionResult> GetSubmissions() =>
            Ok(await products.GetSubmissionSummariesAsync());

        [HttpGet("api/admin/productinputs/{id:int}")]
        public async Task<IActionResult> GetSubmission(int id)
        {
            var dto = await products.GetSubmissionAsync(id);
            return dto is null
                ? NotFound($"Zgłoszenie produktu #{id} nie istnieje albo jest nieczytelne.")
                : Ok(dto);
        }

        // "Zapisz zmiany" – zgłoszenie zostaje w kwarantannie
        [HttpPut("api/admin/productinputs/{id:int}")]
        public Task<IActionResult> UpdateSubmission(int id, [FromBody] CreateProductDTO dto) =>
            RunAsync(async () => await products.UpdateSubmissionAsync(id, dto)
                ? NoContent()
                : NotFound($"Zgłoszenie produktu #{id} nie istnieje."));

        [HttpDelete("api/admin/productinputs/{id:int}")]
        public async Task<IActionResult> DeleteSubmission(int id) =>
            await products.DeleteSubmissionAsync(id)
                ? NoContent()
                : NotFound($"Zgłoszenie produktu #{id} nie istnieje.");

        // "Dodaj do bazy"
        [HttpPost("api/admin/productinputs/{id:int}/approve")]
        public Task<IActionResult> Approve(int id, [FromBody] CreateProductDTO dto) =>
            RunAsync(async () => Ok(new { ProductId = await products.ApproveSubmissionAsync(id, dto) }));

        // =================================================================
        //  PRODUKTY W BAZIE
        // =================================================================

        [HttpGet("api/admin/products")]
        public async Task<IActionResult> GetProducts() =>
            Ok(await products.GetProductSummariesAsync());

        [HttpGet("api/admin/products/{id:int}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var dto = await products.GetProductForEditAsync(id);
            return dto is null ? NotFound($"Produkt #{id} nie istnieje.") : Ok(dto);
        }

        [HttpPut("api/admin/products/{id:int}")]
        public Task<IActionResult> UpdateProduct(int id, [FromBody] CreateProductDTO dto) =>
            RunAsync(async () => await products.UpdateProductAsync(id, dto)
                ? NoContent()
                : NotFound($"Produkt #{id} nie istnieje."));

        [HttpDelete("api/admin/products/{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id) =>
            await products.DeleteProductAsync(id)
                ? NoContent()
                : NotFound($"Produkt #{id} nie istnieje.");

        // bezpośrednie dodanie przez admina (bez kwarantanny)
        [HttpPost("api/admin/products")]
        public Task<IActionResult> CreateProduct([FromBody] CreateProductDTO dto) =>
            RunAsync(async () => Ok(new { ProductId = await products.CreateProductAsync(dto) }));

        // ------------------------------------------------------------------

        private async Task<IActionResult> RunAsync(Func<Task<IActionResult>> action)
        {
            try
            {
                return await action();
            }
            catch (Exception ex) when (ex is ProductValidationException or KeyNotFoundException)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                // szczegóły tylko w logu – komunikat bazy zdradza strukturę tabel
                logger.LogError(ex, "Nie udało się zapisać produktu");
                return BadRequest("Nie udało się zapisać produktu. Sprawdź poprawność danych.");
            }
        }
    }
}
