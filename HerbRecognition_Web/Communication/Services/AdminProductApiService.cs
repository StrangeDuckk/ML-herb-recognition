using System.Net;
using System.Net.Http.Headers;
using HerbRecognition_Web.Communication.DTOs;

namespace HerbRecognition_Web.Communication.Services
{
    /// <summary>Operacje admina na produktach, zgłoszeniach produktów i wpisach słowników.</summary>
    public class AdminProductApiService(HttpClient http, AdminAuthService auth)
    {
        // --------- zgłoszenia (productinput) ------------

        public async Task<List<ProductPreviewDTO>> GetSubmissionsAsync() =>
            await SendAsync<List<ProductPreviewDTO>>(HttpMethod.Get, "api/admin/productinputs", null)
            ?? new List<ProductPreviewDTO>();

        public async Task<CreateProductDTO> GetSubmissionAsync(int id) =>
            await SendAsync<CreateProductDTO>(HttpMethod.Get, $"api/admin/productinputs/{id}", null)
            ?? throw new InvalidOperationException($"API nie zwróciło zgłoszenia #{id}.");

        public Task UpdateSubmissionAsync(int id, CreateProductDTO dto) =>
            SendAsync(HttpMethod.Put, $"api/admin/productinputs/{id}", dto);

        public Task DeleteSubmissionAsync(int id) =>
            SendAsync(HttpMethod.Delete, $"api/admin/productinputs/{id}", null);

        /// <summary>Zgłoszenie -> produkt w bazie. Zwraca Id produktu.</summary>
        public async Task<int> ApproveSubmissionAsync(int id, CreateProductDTO dto)
        {
            var result = await SendAsync<ProductIdResult>(HttpMethod.Post, $"api/admin/productinputs/{id}/approve", dto);
            return result?.ProductId ?? throw new InvalidOperationException("API nie zwróciło Id produktu.");
        }

        // --------- produkty w bazie ------------

        public async Task<List<ProductPreviewDTO>> GetProductsAsync() =>
            await SendAsync<List<ProductPreviewDTO>>(HttpMethod.Get, "api/admin/products", null)
            ?? new List<ProductPreviewDTO>();

        public async Task<CreateProductDTO> GetProductAsync(int id) =>
            await SendAsync<CreateProductDTO>(HttpMethod.Get, $"api/admin/products/{id}", null)
            ?? throw new InvalidOperationException($"API nie zwróciło produktu #{id}.");

        public Task UpdateProductAsync(int id, CreateProductDTO dto) =>
            SendAsync(HttpMethod.Put, $"api/admin/products/{id}", dto);

        public Task DeleteProductAsync(int id) =>
            SendAsync(HttpMethod.Delete, $"api/admin/products/{id}", null);

        /// <summary>Bezpośrednie dodanie produktu przez admina. Zwraca Id produktu.</summary>
        public async Task<int> CreateProductAsync(CreateProductDTO dto)
        {
            var result = await SendAsync<ProductIdResult>(HttpMethod.Post, "api/admin/products", dto);
            return result?.ProductId ?? throw new InvalidOperationException("API nie zwróciło Id produktu.");
        }

        // --------- słowniki: nowy wpis z Id (okno "Przypisz") ------------

        public async Task<int> CreateDictionaryEntryAsync(string dictionary, DictionaryItemDTO item)
        {
            var result = await SendAsync<IdResult>(HttpMethod.Post, $"api/admin/dictionaries/{dictionary}", item);
            return result?.Id ?? throw new InvalidOperationException("API nie zwróciło Id nowego wpisu.");
        }

        private sealed record ProductIdResult(int ProductId);
        private sealed record IdResult(int Id);

        // --------- wspólne (jak w AdminApiService) ------------

        private async Task SendAsync(HttpMethod method, string url, object? body)
        {
            using var response = await SendCoreAsync(method, url, body);
        }

        private async Task<T?> SendAsync<T>(HttpMethod method, string url, object? body)
        {
            using var response = await SendCoreAsync(method, url, body);
            return await response.Content.ReadFromJsonAsync<T>();
        }

        private async Task<HttpResponseMessage> SendCoreAsync(HttpMethod method, string url, object? body)
        {
            if (!auth.IsLoggedIn)
                throw new UnauthorizedAccessException();

            using var request = new HttpRequestMessage(method, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

            if (body is not null)
                request.Content = JsonContent.Create(body);

            var response = await http.SendAsync(request);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                response.Dispose();
                throw new UnauthorizedAccessException();
            }

            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    await ProductApiService.EnsureSuccessAsync(response);
                }
                finally
                {
                    response.Dispose();
                }
            }

            return response;
        }
    }
}
