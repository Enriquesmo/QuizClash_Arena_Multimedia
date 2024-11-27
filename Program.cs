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
using System.Text.Json;

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
.AddCookie() // Usar cookies para almacenar la sesi�n de autenticaci�n
.AddOAuth("Twitch", options =>
{
    options.ClientId = builder.Configuration["Twitch:ClientId"];
    options.ClientSecret = builder.Configuration["Twitch:ClientSecret"];
    options.CallbackPath = new PathString("/auth/callback");
    options.AuthorizationEndpoint = "https://id.twitch.tv/oauth2/authorize";
    options.TokenEndpoint = "https://id.twitch.tv/oauth2/token";
    options.UserInformationEndpoint = "https://api.twitch.tv/helix/users";
    options.Scope.Add("user:edit:broadcast");
    options.Scope.Add("channel:manage:broadcast");
    options.Scope.Add("channel:read:stream_key");

    options.SaveTokens = true;

    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");  // Mapeamos el ID
    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "display_name"); // Mapeamos el nombre para User.Identity.Name
    options.ClaimActions.MapJsonKey("login", "login");               // Login (nombre corto)
    options.ClaimActions.MapJsonKey("profile_image", "profile_image_url");

    options.Events = new OAuthEvents
    {
        OnCreatingTicket = async context =>
        {
            // Obtenemos la informaci�n del usuario desde la API de Twitch
            var request = new HttpRequestMessage(HttpMethod.Get, options.UserInformationEndpoint);
            request.Headers.Add("Authorization", $"Bearer {context.AccessToken}");
            request.Headers.Add("Client-Id", options.ClientId);

            var response = await context.Backchannel.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var userData = user.RootElement.GetProperty("data")[0];

            // Agregamos claims adicionales
            context.Identity.AddClaim(new Claim(ClaimTypes.Name, userData.GetProperty("display_name").GetString()));
            context.Identity.AddClaim(new Claim("brodcasterId", userData.GetProperty("id").GetString()));
        },
        OnRemoteFailure = context =>
        {
            Console.WriteLine($"Error en OAuth: {context.Failure.Message}");
            context.HandleResponse();
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

// ====================================================================
// Endpoints
// ====================================================================

app.MapGet("/start-stream", async (HttpContext context, IHttpClientFactory clientFactory, TwitchApiService twitchApiService) =>
{
    var accessToken = context.Request.Cookies["access_token"];
    var broadcasterId = context.Request.Cookies["brodcasterId"];

    if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(broadcasterId))
    {
        return Results.BadRequest("No se encuentran los datos necesarios.");
    }

    try
    {
        // Obtener la clave de transmisi�n
        var streamKey = await twitchApiService.GetStreamKeyAsync(
            accessToken,
            builder.Configuration["Twitch:ClientId"],
            broadcasterId
        );

        // Redirigir a la p�gina Key_transmision con la clave como par�metro
        return Results.Redirect($"/Key_transmision?streamKey={streamKey}&broadcasterId={broadcasterId}");
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Error al obtener la clave de transmisi�n: {ex.Message}");
    }
});



app.MapGet("/signin-twitch", async (HttpContext context) =>
{
    await context.ChallengeAsync("Twitch", new AuthenticationProperties
    {
        RedirectUri = "/prueba"
    });
});
app.MapGet("/prueba2", async (HttpContext context) =>
{
    await context.ChallengeAsync("Twitch", new AuthenticationProperties
    {
        RedirectUri = "/start-stream"
    });
});


// Ejecutar la aplicaci�n
app.Run();

public class UserInfoResponse
{
    public List<UserInfo> Data { get; set; }
}

public class UserInfo
{
    public string Id { get; set; }
    public string Login { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string ProfileImageUrl { get; set; }
}