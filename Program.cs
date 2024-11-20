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

// Configuración de autenticación con Twitch
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "Twitch";  // Usamos "Twitch" como el esquema de desafío predeterminado
})
.AddCookie()  // Usar cookies para almacenar la sesión de autenticación
.AddOAuth("Twitch", options =>
{
    options.ClientId = builder.Configuration["Twitch:ClientId"]; // Asegúrate de tener el ClientId y ClientSecret en tu archivo appsettings.json
    options.ClientSecret = builder.Configuration["Twitch:ClientSecret"];
    options.CallbackPath = new PathString("/auth/callback"); // La ruta que Twitch debe redirigir después de la autenticación

    // URLs de OAuth de Twitch
    options.AuthorizationEndpoint = "https://id.twitch.tv/oauth2/authorize";
    options.TokenEndpoint = "https://id.twitch.tv/oauth2/token";
    options.UserInformationEndpoint = "https://api.twitch.tv/helix/users";

    options.SaveTokens = true;  // Guarda los tokens para futuras solicitudes



    // Manejar errores de autenticación
    options.Events = new OAuthEvents
    {
        OnRemoteFailure = context =>
        {
            Console.WriteLine($"Error en OAuth: {context.Failure.Message}");
            context.HandleResponse();  // Detiene la propagación del error
            return Task.CompletedTask;
        }
    };
});

// Agregar Razor Pages y SignalR
builder.Services.AddRazorPages();
builder.Services.AddSignalR();  // Agregar SignalR

// ====================================================================
// Construir la aplicación
// ====================================================================
var app = builder.Build();

// Configuración de excepciones y HSTS para producción
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Configuración de middleware para redirección HTTPS y servir archivos estáticos
app.UseHttpsRedirection();
app.UseStaticFiles();  // Asegúrate de que esto está habilitado


// Configurar el pipeline de autenticación y autorización
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Mapeo de Razor Pages
app.MapRazorPages();

// Configuración del Hub de SignalR
app.MapHub<GameHub>("/gameHub");  // Aquí mapeamos el Hub para SignalR

// Configurar el callback para la autenticación de Twitch
app.MapGet("/prueba", async (HttpContext context) =>
{
    var authenticateResult = await context.AuthenticateAsync("Twitch");

    if (!authenticateResult.Succeeded)
    {
        return Results.BadRequest("No se pudo autenticar con Twitch.");
    }

    // Aquí puedes acceder a los datos del usuario autenticado
    var user = authenticateResult.Principal;
    var name = user?.Identity?.Name;
    var profileImageUrl = user?.FindFirst("urn:twitch:profile_image_url")?.Value;

    return Results.Ok($"¡Hola, {name}! <img src='{profileImageUrl}' alt='Profile Image' />");
});
app.MapGet("/signin-twitch", async (HttpContext context) =>
{
// Inicia el desafío de autenticación con Twitch
  await context.ChallengeAsync("Twitch", new AuthenticationProperties
 {
   RedirectUri = "/prueba"  // Después de iniciar sesión, el usuario será redirigido a esta URL
});
});

// Ejecutar la aplicación
app.Run();
