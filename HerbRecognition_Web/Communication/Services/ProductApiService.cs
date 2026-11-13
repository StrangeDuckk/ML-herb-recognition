using System.Net;
using System.Text.Json;
using HerbRecognition_Web.Communication.DTOs;

namespace HerbRecognition_Web.Communication.Services
{
    /// <summary>Publiczne operacje na produktach (przepisach) i ich słownikach.</summary>
    public class ProductApiService(HttpClient http)
    {
        // ------------------------------------------------------------------
        //  Słowniki
        // ------------------------------------------------------------------

        public async Task<List<ProductTypeOption>> GetProductTypesAsync() =>
            await http.GetFromJsonAsync<List<ProductTypeOption>>("api/dictionaries/productType")
            ?? new List<ProductTypeOption>();

        public async Task<List<DictionaryEntry>> GetDictionaryAsync(ProductDictionary kind)
        {
            var (idField, nameField, descriptionField) = kind.JsonFields();

            var items = await http.GetFromJsonAsync<List<JsonElement>>($"api/dictionaries/{kind.Endpoint()}")
                        ?? new List<JsonElement>();

            return items
                .Select(e => new DictionaryEntry(
                    Prop(e, idField)?.GetInt32() ?? 0,
                    Prop(e, nameField)?.GetString() ?? "",
                    descriptionField is null ? null : Prop(e, descriptionField)?.GetString()))
                .Where(e => e.Id > 0)
                .OrderBy(e => e.Name, StringComparer.Create(new System.Globalization.CultureInfo("pl-PL"), true))
                .ToList();
        }

        // ------------------------------------------------------------------
        //  Produkty
        // ------------------------------------------------------------------

        public async Task<List<ProductPreviewDTO>> GetProductsAsync(int? plantId = null) =>
            await http.GetFromJsonAsync<List<ProductPreviewDTO>>(
                plantId is int id ? $"api/products?plantId={id}" : "api/products")
            ?? new List<ProductPreviewDTO>();

        public async Task<List<ProductPreviewDTO>> GetPendingProductsAsync(int? plantId = null) =>
            await http.GetFromJsonAsync<List<ProductPreviewDTO>>(
                plantId is int id ? $"api/products/pending?plantId={id}" : "api/products/pending")
            ?? new List<ProductPreviewDTO>();

        public async Task<GetProductDTO?> GetProductAsync(int id)
        {
            using var response = await http.GetAsync($"api/products/{id}");
            if (response.StatusCode == HttpStatusCode.NotFound) return null;

            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<GetProductDTO>();
        }

        /// <summary>Zdjęcia zatwierdzonego produktu (główne pierwsze).</summary>
        public async Task<List<PictureDTO>> GetProductPicturesAsync(int productId) =>
            await http.GetFromJsonAsync<List<PictureDTO>>($"api/products/{productId}/pictures")
            ?? new List<PictureDTO>();

        /// <summary>Nowe zgłoszenie produktu (kwarantanna). Zwraca Id zgłoszenia.</summary>
        public async Task<int> SubmitAsync(CreateProductDTO dto)
        {
            using var response = await http.PostAsJsonAsync("api/productInput", dto);
            await EnsureSuccessAsync(response);

            var created = await response.Content.ReadFromJsonAsync<CreatedResponse>();
            return created?.Id ?? throw new InvalidOperationException("API nie zwróciło numeru zgłoszenia.");
        }

        private sealed record CreatedResponse(int Id);

        // ------------------------------------------------------------------

        /// <summary>Błąd API jako czytelny komunikat (API zwraca zwykły tekst przy 400).</summary>
        internal static async Task EnsureSuccessAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode) return;

            var body = (await response.Content.ReadAsStringAsync()).Trim().Trim('"');
            throw new HttpRequestException(
                body.Length is > 0 and <= 500 && !body.StartsWith('{')
                    ? body
                    : $"Błąd API ({(int)response.StatusCode}). Spróbuj ponownie.");
        }

        private static JsonElement? Prop(JsonElement element, string name)
        {
            foreach (var p in element.EnumerateObject())
                if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
                    return p.Value;
            return null;
        }
    }
}
