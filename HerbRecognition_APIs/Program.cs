using HerbRecognition_APIs.Models;
using HerbRecognition_APIs.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// =====================================================================
//  Konfiguracja
//  Lokalnie:   appsettings.Development.json (nie trafia do repozytorium)
//  Serwer:     zmienne środowiskowe, np. AdminAuth__Password, Jwt__Key,
//              ConnectionStrings__DefaultConnection (ustawiane w docker-compose / Coolify)
// =====================================================================

if (string.IsNullOrWhiteSpace(builder.Configuration["AdminAuth:Username"]) ||
    string.IsNullOrWhiteSpace(builder.Configuration["AdminAuth:Password"]))
{
    throw new InvalidOperationException(
        "Brak AdminAuth:Username lub AdminAuth:Password w konfiguracji.");
}

if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("DefaultConnection")))
{
    throw new InvalidOperationException(
        "Brak ConnectionStrings:DefaultConnection w konfiguracji.");
}

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"];

if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
{
    throw new InvalidOperationException(
        "Brak klucza Jwt:Key (minimum 32 znaki) w konfiguracji.");
}

// -- limit rozmiaru żądania ----
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 1 * 1024 * 1024;   // 1 MB (domyślnie 30 MB)
    options.AddServerHeader = false;                       // nie zdradzamy nazwy serwera
});

// -- JWT ----
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],

            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },

            ValidateLifetime = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

// -- ograniczenie liczby żądań ----
// Na serwerze API jest dostępne tylko z wewnątrz (z kontenera strony), więc dla API
// wszyscy użytkownicy mają ten sam adres. Limity są ustawione tak, żeby nie blokowały
// normalnego ruchu całej strony, a nadal hamowały nadużycia (np. zgadywanie hasła).
static string ClientKey(HttpContext context) =>
    context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
        await context.HttpContext.Response.WriteAsync(
            "Zbyt wiele żądań. Spróbuj ponownie za chwilę.", token);
    };

    // wszystkie żądania: 3000 na minutę
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(ClientKey(context), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 3000,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));

    // logowanie admina: 5 prób na 5 minut
    options.AddPolicy("login", context =>
        RateLimitPartition.GetFixedWindowLimiter(ClientKey(context), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(5),
            QueueLimit = 0
        }));

    // nowe zgłoszenia: 60 na 10 minut (dla całej strony)
    options.AddPolicy("submit", context =>
        RateLimitPartition.GetFixedWindowLimiter(ClientKey(context), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 60,
            Window = TimeSpan.FromMinutes(10),
            QueueLimit = 0
        }));

    // wyszukiwanie: 600 na minutę (dla całej strony)
    options.AddPolicy("search", context =>
        RateLimitPartition.GetFixedWindowLimiter(ClientKey(context), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 600,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
});

// -- usługi ----
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();   // błędy 500 bez szczegółów technicznych

builder.Services.AddDbContext<HerbRecognitionDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IDbService, DbService>();
builder.Services.AddScoped<ISubmissionService, SubmissionService>();
builder.Services.AddScoped<IPlantSearchService, PlantSearchService>();

// -- CORS: adresy z konfiguracji (Cors:AllowedOrigins) ----
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? new[] { "https://localhost:7092" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorPolicy", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .WithMethods("GET", "POST", "PUT", "DELETE")
            .WithHeaders("Content-Type", "Authorization");
    });
});

var app = builder.Build();

// =====================================================================
//  Pipeline
// =====================================================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseHttpsRedirection();   // lokalnie HTTPS; na serwerze API działa po HTTP w sieci wewnętrznej
}
else
{
    app.UseExceptionHandler();   // ogólny ProblemDetails zamiast stack trace
}

// nagłówki bezpieczeństwa dla każdej odpowiedzi
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "no-referrer";
    headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
    headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

    await next();
});

app.UseCors("BlazorPolicy");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
