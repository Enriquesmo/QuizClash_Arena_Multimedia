using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.FileProviders;
using QuizClash_Arena_Multimedia.Hubs;
using QuizClash_Arena_Multimedia.Services;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// ====================================================================
// Agregar servicios
// ====================================================================

// Registrar el servicio para Twitch (Servicio HTTP para API de Twitch)
builder.Services.AddHttpClient<TwitchApiService>();  // Esto registra el servicio para interactuar con la API de Twitch

// Configuraci�n de autenticaci�n con Twitch
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "Twitch";  // Usamos "Twitch" como el esquema de desaf�o predeterminado
})
.AddCookie()  // Usar cookies para almacenar la sesi�n de autenticaci�n
.AddOAuth("Twitch", options =>
{
    options.ClientId = builder.Configuration["Twitch:ClientId"]; // Aseg�rate de tener el ClientId y ClientSecret en tu archivo appsettings.json
    options.ClientSecret = builder.Configuration["Twitch:ClientSecret"];
    options.CallbackPath = new PathString("/auth/callback"); // La ruta que Twitch debe redirigir despu�s de la autenticaci�n

    // URLs de OAuth de Twitch
    options.AuthorizationEndpoint = "https://id.twitch.tv/oauth2/authorize";
    options.TokenEndpoint = "https://id.twitch.tv/oauth2/token";
    options.UserInformationEndpoint = "https://api.twitch.tv/helix/users";

    options.SaveTokens = true;  // Guarda los tokens para futuras solicitudes



    // Manejar errores de autenticaci�n
    options.Events = new OAuthEvents
    {
        OnRemoteFailure = context =>
        {
            Console.WriteLine($"Error en OAuth: {context.Failure.Message}");
            context.HandleResponse();  // Detiene la propagaci�n del error
            return Task.CompletedTask;
        }
    };
});

// Agregar Razor Pages y SignalR
builder.Services.AddRazorPages();
builder.Services.AddSignalR();  // Agregar SignalR

// ====================================================================
// Construir la aplicaci�n
// ====================================================================
var app = builder.Build();

// Configuraci�n de excepciones y HSTS para producci�n
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Configuraci�n de middleware para redirecci�n HTTPS y servir archivos est�ticos
app.UseHttpsRedirection();
app.UseStaticFiles();  // Aseg�rate de que esto est� habilitado


// Configurar el pipeline de autenticaci�n y autorizaci�n
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Mapeo de Razor Pages
app.MapRazorPages();

// Configuraci�n del Hub de SignalR
app.MapHub<GameHub>("/gameHub");  // Aqu� mapeamos el Hub para SignalR

// Configurar el callback para la autenticaci�n de Twitch
app.MapGet("/prueba", async (HttpContext context) =>
{
    var authenticateResult = await context.AuthenticateAsync("Twitch");

    if (!authenticateResult.Succeeded)
    {
        return Results.BadRequest("No se pudo autenticar con Twitch.");
    }

    // Aqu� puedes acceder a los datos del usuario autenticado
    var user = authenticateResult.Principal;
    var name = user?.Identity?.Name;
    var profileImageUrl = user?.FindFirst("urn:twitch:profile_image_url")?.Value;

    return Results.Ok($"�Hola, {name}! <img src='{profileImageUrl}' alt='Profile Image' />");
});
app.MapGet("/signin-twitch", async (HttpContext context) =>
{
// Inicia el desaf�o de autenticaci�n con Twitch
  await context.ChallengeAsync("Twitch", new AuthenticationProperties
 {
   RedirectUri = "/prueba"  // Despu�s de iniciar sesi�n, el usuario ser� redirigido a esta URL
});
});

// Ejecutar la aplicaci�n
app.Run();
