namespace HerbRecognition_Web.Communication.Services
{
    public class AdminAuthService(HttpClient http)
    {
        public string? Token { get; private set; }
        public DateTime? ExpiresAt { get; private set; }
        public bool IsLoggedIn => Token is not null && ExpiresAt > DateTime.UtcNow;

        public async Task<bool> LoginAsync(string username, string password)
        {
            using var response = await http.PostAsJsonAsync("api/auth/login",
                new { Username = username, Password = password });

            if (!response.IsSuccessStatusCode)
                return false;

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            Token = result?.Token;
            ExpiresAt = result?.ExpiresAt;
            return IsLoggedIn;
        }

        public void Logout()
        {
            Token = null;
            ExpiresAt = null;
        }

        private sealed record LoginResponse(string Token, DateTime ExpiresAt);
    }
}
