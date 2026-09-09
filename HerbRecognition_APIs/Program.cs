using HerbRecognition_APIs.Models;
using HerbRecognition_APIs.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<HerbRecognitionDbContext>();

    var entity = db.Model.FindEntityType(typeof(PlantSimilarPlant));

    Console.WriteLine("=== PROPERTIES ===");

    foreach (var property in entity!.GetProperties())
    {
        Console.WriteLine(
            $"Property: {property.Name}, Column: {property.GetColumnName()}");
    }

    Console.WriteLine("=== FOREIGN KEYS ===");

    foreach (var foreignKey in entity.GetForeignKeys())
    {
        Console.WriteLine(
            $"FK: {string.Join(", ", foreignKey.Properties.Select(p => p.Name))}" +
            $" -> {foreignKey.PrincipalEntityType.ClrType.Name}");
    }

    Console.WriteLine("=== NAVIGATIONS ===");

    foreach (var navigation in entity.GetNavigations())
    {
        Console.WriteLine(
            $"Navigation: {navigation.Name}, " +
            $"FK: {string.Join(", ", navigation.ForeignKey.Properties.Select(p => p.Name))}");
    }
}


app.Run();
