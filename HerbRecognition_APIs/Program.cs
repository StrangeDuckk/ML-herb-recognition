using HerbRecognition_APIs.Models;
using HerbRecognition_APIs.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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

// --- CORS -----
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorPolicy", policy =>
    {
        policy
            .WithOrigins("https://localhost:7092")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// --- pipeline aplikacji ----------------------
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("BlazorPolicy");
app.UseAuthentication(); // token odczyt
app.UseAuthorization();  // czy ma dostep do zawartosci

app.MapControllers();

app.Run();
