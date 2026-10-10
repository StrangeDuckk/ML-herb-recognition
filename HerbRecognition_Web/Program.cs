using HerbRecognition_Web.Components;
using HerbRecognition_Web.Communication.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var apiBaseAddress = new Uri("https://localhost:7155/");

// wspó³dzielony klient API.
builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = apiBaseAddress;
});

builder.Services.AddScoped(sp =>
sp.GetRequiredService<IHttpClientFactory>()
.CreateClient("Api"));

// Us³ugi korzystaj¹ce z tego samego HttpClient.
builder.Services.AddScoped<PlantApiService>();
builder.Services.AddScoped<UserInputApiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
