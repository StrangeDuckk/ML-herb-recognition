using HerbRecognition_APIs.Models;
using HerbRecognition_APIs.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// -- logowanie ----
if (string.IsNullOrWhiteSpace(builder.Configuration["AdminAuth:Username"]) ||
    string.IsNullOrWhiteSpace(builder.Configuration["AdminAuth:Password"]))
{
    throw new InvalidOperationException(
        "Brak AdminAuth:Username lub AdminAuth:Password w konfiguracji.");
}

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"];

if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
{
    throw new InvalidOperationException(
        "Brak klucza Jwt:Key (minimum 32 znaki) w konfiguracji.");
}

// -- limit rozmiaru rzadania ----
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 1 * 1024 * 1024;   // 1 MB (domyœlnie 30 MB)
    options.AddServerHeader = false;                       // nie zdradzamy nazwy serwera
});

// -- jwt ----
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

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1) // domyœlnie 5 min tolerancji po wygaœniêciu
        };
    });

builder.Services.AddAuthorization();

// -- ograniczenie liczby rzadan -----
static string ClientKey(HttpContext context) =>
    context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
        await context.HttpContext.Response.WriteAsync(
            "Zbyt wiele ¿¹dañ. Spróbuj ponownie za chwilê.", token);
    };

    // ka¿de ¿¹danie: 600 na minutê z jednego adresu
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(ClientKey(context), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 600,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));

    // logowanie: 5 prób na 5 minut – zgadywanie has³a staje siê niepraktyczne
    options.AddPolicy("login", context =>
        RateLimitPartition.GetFixedWindowLimiter(ClientKey(context), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(5),
            QueueLimit = 0
        }));

    // nowe zg³oszenia: 30 na 10 minut
    options.AddPolicy("submit", context =>
        RateLimitPartition.GetFixedWindowLimiter(ClientKey(context), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 30,
            Window = TimeSpan.FromMinutes(10),
            QueueLimit = 0
        }));

    // wyszukiwanie (porównuje wszystkie roœliny): 120 na minutê
    options.AddPolicy("search", context =>
        RateLimitPartition.GetFixedWindowLimiter(ClientKey(context), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 120,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
});


// --- uslugi w tym api i dbcontext ------
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<HerbRecognitionDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddScoped<IDbService, DbService>();
builder.Services.AddScoped<ISubmissionService, SubmissionService>();
builder.Services.AddScoped<IPlantSearchService, PlantSearchService>();

// --- CORS -----
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorPolicy", policy =>
    {
        policy
            .WithOrigins("https://localhost:7092")
            .WithMethods("GET", "POST", "PUT", "DELETE")
            .WithHeaders("Content-Type", "Authorization");
    });
});

var app = builder.Build();

// --- pipeline aplikacji ----------------------
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


// nag³ówki bezpieczeñstwa dla ka¿dej odpowiedzi
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

app.UseHttpsRedirection();

app.UseCors("BlazorPolicy");
app.UseRateLimiter();
app.UseAuthentication(); // token odczyt
app.UseAuthorization();  // czy ma dostep do zawartosci

app.MapControllers();

app.Run();
