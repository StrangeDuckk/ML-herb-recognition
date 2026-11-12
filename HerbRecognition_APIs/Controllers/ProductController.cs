using HerbRecognition_APIs.DTOs;
using HerbRecognition_APIs.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HerbRecognition_APIs.Controllers
{
    /// <summary>Publiczne produkty (przepisy) i zgłaszanie nowych.</summary>
    [ApiController]
    public class ProductController(IProductService products) : ControllerBase
    {
        // GET api/products            – biblioteka przepisów
        // GET api/products?plantId=5  – przepisy z danej rośliny (strona rośliny)
        [HttpGet("api/products")]
        public async Task<IActionResult> GetAll([FromQuery] int? plantId) =>
            Ok(await products.GetProductsAsync(plantId));

        [HttpGet("api/products/{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var product = await products.GetProductAsync(id);
            return product is null ? NotFound() : Ok(product);
        }

        // GET api/products/pending – zgłoszenia do biblioteki (przełącznik "pokaż niezweryfikowane")
        [HttpGet("api/products/pending")]
        [EnableRateLimiting("search")]
        public async Task<IActionResult> GetPending() =>
            Ok(await products.GetPendingPreviewsAsync());

        // POST api/productInput – nowe zgłoszenie produktu (kwarantanna)
        [HttpPost("api/productInput")]
        [EnableRateLimiting("submit")]
        [RequestSizeLimit(64 * 1024)]
        public async Task<IActionResult> Submit([FromBody] CreateProductDTO dto)
        {
            try
            {
                var id = await products.SubmitAsync(dto);

                return Created($"/api/productInput/{id}", new
                {
                    Id = id,
                    Message = "Zgłoszenie produktu zostało zapisane.",
                    Url = $"/api/productInput/{id}"
                });
            }
            catch (Exception ex) when (ex is ProductValidationException or KeyNotFoundException)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
