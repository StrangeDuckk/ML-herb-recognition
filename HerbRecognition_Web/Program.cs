using HerbRecognition_Web.Components;
using HerbRecognition_Web.Communication.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// =====================================================================
//  Adres API
//  Lokalnie: appsettings.Development.json -> "ApiBaseUrl": "https://localhost:7155/"
//  Serwer:   zmienna środowiskowa ApiBaseUrl=http://api:8080/ (sieć wewnętrzna Dockera)
// =====================================================================

var apiBaseUrl = builder.Configuration["ApiBaseUrl"];

if (string.IsNullOrWhiteSpace(apiBaseUrl))
    throw new InvalidOperationException("Brak ApiBaseUrl w konfiguracji.");

builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api"));

// usługi korzystające z tego samego HttpClient
builder.Services.AddScoped<PlantApiService>();
builder.Services.AddScoped<UserInputApiService>();

// autoryzacja panelu admina
builder.Services.AddScoped<AdminAuthService>();
builder.Services.AddScoped<AdminApiService>();

// =====================================================================
//  Klucze Data Protection w trwałym folderze
//  Bez tego każde wdrożenie tworzyłoby nowe klucze, a otwarte formularze
//  i połączenia użytkowników przestawałyby działać.
// =====================================================================

var keysPath = builder.Configuration["DataProtection:KeysPath"];

if (!string.IsNullOrWhiteSpace(keysPath))
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
        .SetApplicationName("HerbRecognition_Web");
}

// =====================================================================
//  Za serwerem proxy (Traefik w Coolify obsługuje HTTPS)
//  Dzięki temu aplikacja wie, że użytkownik przyszedł przez https.
// =====================================================================

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // proxy działa w sieci Dockera pod zmiennym adresem
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();   // musi być pierwsze

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();   // na serwerze przekierowanie na HTTPS robi Traefik
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// =====================================================================
//  Zdjęcia roślin: https://twojadomena.pl/images/nazwa-pliku.webp
// =====================================================================

var imagesPath = builder.Configuration["Images:Path"];

if (!string.IsNullOrWhiteSpace(imagesPath) && Directory.Exists(imagesPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(imagesPath),
        RequestPath = "/images"
    });
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
